#!/usr/bin/env bash
set -Eeuo pipefail
umask 077

ARCHIVE=/var/lib/alouva-deploy/release.tar.gz
APP=/opt/alouva/app
PREV=/opt/alouva/app-prev
ROOT=/opt/alouva
exec 9>/run/lock/alouva-deploy.lock
flock -n 9 || { echo "Deployment already running" >&2; exit 1; }

# All temporary paths are private and created by root.
WORK=$(mktemp -d /root/alouva-deploy.XXXXXXXX)
STAGE="$WORK/stage"
BACKUP="$WORK/backup"
mkdir -m 700 "$STAGE" "$BACKUP"
cleanup() { rm -rf -- "$WORK"; }
trap cleanup EXIT

test -f "$ARCHIVE" && test ! -L "$ARCHIVE"
test "$(stat -c %U "$ARCHIVE")" = deploy-alouva
# Copy the untrusted upload once into a root-only directory.
cp -- "$ARCHIVE" "$WORK/release.tar.gz"

# Extract regular files and directories only; reject traversal, duplicates,
# links, special files, huge entries and oversized total archives.
python3 - "$WORK/release.tar.gz" "$STAGE" <<'PY'
import os, pathlib, sys, tarfile
archive, destination = sys.argv[1:]
total = 0
seen = set()
with tarfile.open(archive, "r:gz") as tar:
    members = tar.getmembers()
    if not members or len(members) > 5000:
        raise SystemExit("Invalid archive entry count")
    for member in members:
        path = pathlib.PurePosixPath(member.name)
        parts = tuple(p for p in path.parts if p != ".")
        if (path.is_absolute() or not parts or ".." in parts
                or not (member.isfile() or member.isdir())
                or parts in seen):
            raise SystemExit("Unsafe archive entry")
        seen.add(parts)
        total += member.size
        if member.size > 200_000_000 or total > 600_000_000:
            raise SystemExit("Archive exceeds size limit")
    for member in members:
        parts = tuple(p for p in pathlib.PurePosixPath(member.name).parts if p != ".")
        target = os.path.join(destination, *parts)
        if member.isdir():
            os.makedirs(target, mode=0o700, exist_ok=True)
        else:
            os.makedirs(os.path.dirname(target), mode=0o700, exist_ok=True)
            if os.path.lexists(target):
                raise SystemExit("Duplicate or conflicting archive path")
            source = tar.extractfile(member)
            if source is None:
                raise SystemExit("Missing archive data")
            with open(target, "xb") as out:
                while True:
                    chunk = source.read(1024 * 1024)
                    if not chunk:
                        break
                    out.write(chunk)
PY

test -s "$STAGE/Freecrmlance.Web.dll"
test -s "$STAGE/Freecrmlance.Web.runtimeconfig.json"
test -d "$STAGE/wwwroot"
test -f "$STAGE/appsettings.json"
test -d "$APP" && test ! -L "$APP"
test -d "$PREV" && test ! -L "$PREV"

# Snapshot before touching the live application.
rsync -a --delete -- "$APP/" "$BACKUP/"
test -s "$BACKUP/Freecrmlance.Web.dll"

restore() {
    echo "Deployment failed; restoring previous application" >&2
    trap - ERR
    systemctl stop alouva || true
    rsync -a --delete -- "$BACKUP/" "$APP/" || {
        echo "CRITICAL: automatic rollback failed; backup is at $BACKUP" >&2
        trap - EXIT
        exit 1
    }
    chown -R alouva:alouva "$APP"
    systemctl start alouva || true
}
trap restore ERR
systemctl stop alouva
rsync -a --delete -- "$STAGE/" "$APP/"
chown -R alouva:alouva "$APP"
systemctl start alouva

healthy=0
for i in $(seq 1 20); do
    if systemctl is-active --quiet alouva && curl --fail --silent --output /dev/null --max-time 3 http://127.0.0.1:5000/; then
        healthy=1
        break
    fi
    sleep 2
done
if [ "$healthy" -ne 1 ]; then
    echo "Health check failed" >&2
    false
fi
trap - ERR
rsync -a --delete -- "$BACKUP/" "$PREV/"
chown -R alouva:alouva "$PREV"
echo "Alouva deployed successfully"
