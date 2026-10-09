# Déploiement continu Alouva — VPS OVH

## Objectif
Automatiser chaque push sur `main` après validation de la CI .NET 9, sans reconfigurer PostgreSQL, Nginx, TLS, les sauvegardes ni les données persistantes.

## État existant (vérifié manuellement le 6 octobre 2026)
- Dépôt : `barryflux/freecrmlance`, branche `main`.
- Sources sur VPS : `/opt/alouva/src`.
- Publication actuelle : `/opt/alouva/app`.
- Service systemd : `alouva`, exécutable `/opt/dotnet/dotnet /opt/alouva/app/Freecrmlance.Web.dll`.
- Écoute interne : `127.0.0.1:5000`, derrière Nginx.
- Version déployée manuellement : merge PR #94, `e22958a`.
- CI existante : `.github/workflows/ci.yml` (restore, build, tests).

## Stratégie
1. Sur push `main`, GitHub Actions exécute la CI existante.
2. Le workflow de déploiement attend sa réussite, puis publie .NET 9 dans un artefact temporaire.
3. Transfert SSH avec compte de déploiement dédié et clé privée stockée dans GitHub Actions.
4. Sur le VPS, staging hors des données persistantes, sauvegarde de la version applicative précédente, installation des nouveaux binaires, redémarrage du service systemd.
5. Vérification HTTP locale après redémarrage ; en cas d'échec, restaurer les binaires précédents et redémarrer. Signaler l'échec du job.

## Sécurité et prérequis avant activation
- Créer un compte SSH de déploiement **non root**, à droits minimaux, et une clé dédiée ; autoriser uniquement les opérations nécessaires à la publication et au redémarrage du service.
- Vérifier `systemctl cat alouva`, les propriétaires et permissions de `/opt/alouva/app`, les chemins de configuration et stockage persistants avant d'écrire un script.
- Configurer les secrets GitHub (`VPS_HOST`, `VPS_USER`, `VPS_SSH_PRIVATE_KEY`, `VPS_SSH_HOST_KEY`), idéalement dans l'environnement GitHub `production`. La clé d'hôte doit être vérifiée indépendamment, pas récupérée aveuglément lors du déploiement.
- Limiter l'accès SSH par clé et conserver l'authentification système existante jusqu'à validation.
- Ne jamais inclure `appsettings.Production.json`, les secrets, uploads, sauvegardes ou dossiers persistants dans une opération de nettoyage/remplacement.
- Ne pas lancer automatiquement de migrations EF sans procédure de sauvegarde et validation spécifique.
- Empêcher les déploiements concurrents et journaliser la version Git déployée.
- Prévoir un test fonctionnel après la première activation et documenter la restauration.

## Critères d'acceptation
- Un push sur `main` avec CI réussie déclenche un seul déploiement.
- Une CI échouée ne déploie rien.
- Le site répond après publication ; le service `alouva` est actif.
- Les données PostgreSQL, documents, certificats et configurations sont inchangés.
- Un échec de santé déclenche une restauration de la version précédente.
- Les secrets ne figurent ni dans le dépôt ni dans les logs.

## Statut
**Conception documentée ; automatisation non activée.** Les droits SSH et chemins de persistance doivent être vérifiés sur le VPS avant de fusionner un workflow de déploiement actif.
