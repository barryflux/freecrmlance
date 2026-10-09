#!/usr/bin/env bash
set -Eeuo pipefail
umask 027
ARCHIVE=/var/lib/alouva-deploy/release.tar.gz
APP=/opt/alouva/app
PREV=/opt/alouva/app-prev
STAGE=$(mktemp -d /opt/alouva/.deploy-stage.XXXXXXXX)
BACKUP=$(mktemp -d /opt/alouva/.deploy-backup.XXXXXXXX)
LOCK=/run/lock/alouva-deploy.lock
exec 9>"$LOCK"
flock -n 9 || { echo "Another deployment is running"; exit 1; }
cleanup() { rm -rf -- "$STAGE" "$BACKUP"; }
trap cleanup EXIT
test -f "$ARCHIVE"
test ! -L "$ARCHIVE"
test "$(stat -c %U "$ARCHIVE")" = deploy-alouva
# Reject archives containing absolute paths, parent traversal, links, and special files.
python3 - "$ARCHIVE" <<'PY'
import sys,tarfile,pathlib
with tarfile.open(sys.argv[1], 'r:gz') as t:
    members=t.getmembers()
    if not members or len(members)>5000: raise SystemExit('Invalid archive size')
    for m in members:
        p=pathlib.PurePosixPath(m.name)
        if p.is_absolute() or '..' in p.parts or not (m.isfile() or m.isdir()):
            raise SystemExit('Unsafe archive entry')
        if m.size>200_000_000: raise SystemExit('Oversized file')
PY
tar -xzf "$ARCHIVE" -C "$STAGE" --no-same-owner --no-same-permissions
test -s "$STAGE/Freecrmlance.Web.dll"
test -s "$STAGE/Freecrmlance.Web.runtimeconfig.json"
test -d "$STAGE/wwwroot"
# Preserve any preexisting config file not shipped in the artifact.
test -f "$STAGE/appsettings.json"
systemctl stop alouva
rsync -a --delete "$APP/" "$BACKUP/"
restore() {
  echo "Deployment failed; restoring previous version" >&2
  rsync -a --delete "$BACKUP/" "$APP/"
  chown -R alouva:alouva "$APP"
  systemctl restart alouva || true
}
trap 'restore' ERR
rsync -a --delete "$STAGE/" "$APP/"
chown -R alouva:alouva "$APP"
systemctl start alouva
healthy=0
for i in $(seq 1 20); do
  if systemctl is-active --quiet alouva && curl --silent --show-error --output /dev/null --max-time 3 http://127.0.0.1:5000/; then
    healthy=1; break
  fi
  sleep 2
done
if [ "$healthy" -ne 1 ]; then false; fi
trap - ERR
rsync -a --delete "$BACKUP/" "$PREV/"
chown -R alouva:alouva "$PREV"
echo "Alouva deployment healthy"
