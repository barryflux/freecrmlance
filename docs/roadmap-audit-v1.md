# Alouva — Roadmap technique Audit V1

## Vision

Le module Audit est un moteur d'audit métier générique et personnalisable. Il permet à un freelance de créer ses propres modèles, créer et réaliser un audit pour un client, sauvegarder progressivement le travail, joindre des preuves, finaliser une version immuable, générer un rapport PDF et transmettre une version précise au client.

Workflow cible :

```text
Modèle personnalisé
  -> Création de l'audit
  -> Réalisation
  -> Finalisation
  -> Génération du rapport
  -> Transmission au client
  -> Conservation / historique
```

Le module respecte l'architecture existante Domain -> Application -> Infrastructure -> Web. Toutes les données sont isolées par WorkspaceId. Il ne crée ni microservice, ni base séparée, ni second système de stockage documentaire.

## Principes structurants

### Modèle et audit sont distincts

Un `AuditTemplate` est réutilisable et modifiable. Lors de la création d'un `Audit`, Alouva copie le modèle sous forme de snapshot. Toute modification ultérieure du modèle ne doit jamais modifier un audit existant.

### Audit en cours et rapport final sont distincts

Un audit reste modifiable pendant sa réalisation. La finalisation crée une `AuditReportVersion` immuable représentant exactement le contenu remis ou susceptible d'être remis au client.

Une correction après finalisation produit V2 ; V1 reste conservée.

### Intégrité

Chaque version finalisée possède une empreinte d'intégrité, par exemple SHA-256 d'un snapshot canonique. Cette empreinte ne doit pas être présentée comme une certification juridique.

### Réutilisation de l'existant

Les preuves et pièces jointes réutilisent l'infrastructure Document / IFileStorage. La transmission réutilise autant que possible les mécanismes existants de partage de documents, devis et factures.

## Modèle métier cible

```text
Workspace
  +-- AuditTemplates
  |     +-- Sections
  |           +-- Items
  |
  +-- Customers
        +-- Audits
              +-- Sections (snapshot)
              |     +-- Items (snapshot)
              |           +-- Responses
              |           +-- Evidence
              |
              +-- ReportVersions
                    +-- GeneratedDocument
                    +-- Transmissions
```

Types de réponses V1 : YesNo, CompliantNonCompliant, Text, LongText, Number, Rating, SingleChoice, MultipleChoice.

Les observations et recommandations sont indépendantes du type de réponse.

Statuts initiaux de l'audit : Draft, InProgress, Completed, Finalized. La transmission est un événement associé à une version et ne doit pas être confondue avec l'état métier de l'audit.

---

## US-034 — Audit Templates

### Objectif

Permettre au freelance de créer, modifier, réordonner, dupliquer et archiver ses modèles d'audit.

### Entités pressenties

- AuditTemplate : Id, WorkspaceId, Name, Description, CreatedAtUtc, UpdatedAtUtc.
- AuditTemplateSection : Id, AuditTemplateId, Title, Description, Position.
- AuditTemplateItem : Id, SectionId, Label, Description, ResponseType, IsRequired, Position, Configuration.

### Invariants

- Un modèle appartient à un seul Workspace.
- Les sections et critères sont ordonnés.
- Un modèle ayant déjà servi ne doit pas être supprimé physiquement depuis l'application : archivage.
- Configuration validée selon ResponseType.

### Tests clés

Isolation Workspace, ordre, validation des réponses, duplication, archivage.

---

## US-035 — Audit Foundation & Snapshot

### Objectif

Créer un audit pour un client depuis un modèle et générer une référence de type `AUD-2026-0001`.

### Données principales

Audit : Id, WorkspaceId, CustomerId, TemplateId, Reference, Title, Description, Status, StartedAtUtc, CompletedAtUtc, CreatedAtUtc.

AuditSection et AuditItem contiennent le snapshot du modèle nécessaire à l'exécution de l'audit.

### Invariant majeur

Modifier ou archiver le template après création ne change jamais le contenu de l'audit existant.

### Tests clés

Références, ownership du client, snapshot, isolation Workspace, transitions de statut.

---

## US-036 — Audit Workspace

### Objectif

Fournir l'écran de réalisation de l'audit avec navigation par sections et sauvegarde progressive.

### Réponse

AuditItemResponse contient notamment Value, Observation, Recommendation, UpdatedAtUtc et UpdatedByUserId.

### Comportements

- Sauvegarde progressive.
- Reprise ultérieure sans perte.
- Progression calculée à partir des critères renseignés/obligatoires.
- Pas de score global obligatoire dans le moteur V1.
- Validation cohérente avec ResponseType.

### Tests clés

Persistance, champs obligatoires, reprise, progression, autorisations.

---

## US-037 — Evidence & Attachments

### Objectif

Joindre des preuves à l'audit global ou à un critère précis.

AuditEvidence : AuditId, AuditItemId optionnel, DocumentId, Caption, AddedAtUtc, AddedByUserId.

### Invariants

- Réutiliser Document / IFileStorage.
- Une preuve doit appartenir au même Workspace et au bon audit.
- Ne jamais créer un stockage de fichiers parallèle.

### Tests clés

Upload, téléchargement, ownership, suppression/détachement selon règles documentaires, isolation Workspace.

---

## US-038 — Finalisation & Versioning

### Objectif

Valider l'audit et créer une version immuable.

AuditReportVersion : Id, AuditId, VersionNumber, FinalizedAtUtc, FinalizedByUserId, Snapshot, Hash, GeneratedDocumentId.

### Invariants

- Vérifier les critères obligatoires avant finalisation.
- Une version finalisée n'est jamais modifiée.
- Une correction produit une nouvelle version.
- Le snapshot d'une ancienne version reste disponible.
- L'empreinte est recalculable et vérifiable.

### Tests clés

Immutabilité, versionnement, hash, concurrence, autorisations et finalisation incomplète.

---

## US-039 — Audit Report

### Objectif

Générer un PDF professionnel à partir d'une AuditReportVersion.

### Contenu minimal

Identité du freelance, référence, client, date/contexte/périmètre, sections, critères, résultats, constats, recommandations, preuves pertinentes, conclusion, date de finalisation et numéro de version.

### Invariant majeur

Le PDF est généré depuis le snapshot de la version, jamais depuis l'état courant de l'audit.

### MVP fonctionnel

À la fin de US-039, un freelance doit pouvoir créer son modèle, réaliser l'audit, le finaliser et remettre un PDF au client.

---

## US-040 — Sharing & Transmission

### Objectif

Télécharger et/ou partager de manière sécurisée une version précise du rapport.

AuditTransmission : Id, AuditId, AuditReportVersionId, Recipient, Method, CreatedAtUtc, CreatedByUserId, ShareId ou SharedDocumentId selon l'infrastructure retenue.

### Invariants

- Toujours savoir quelle version exacte a été transmise.
- Une nouvelle version ne remplace jamais silencieusement une ancienne transmission.
- Réutiliser le partage sécurisé existant autant que possible.

---

## US-041 — Audit History

### Objectif

Afficher une chronologie métier exploitable : création, démarrage, progression significative, finalisation V1/V2, génération du rapport et transmissions.

Cette piste métier est distincte des logs techniques de l'application.

---

## US-042 — Hardening & Audit Security

### Objectif

Rendre le module prêt pour une exposition production.

### Contrôles

- Tests systématiques d'isolation Workspace.
- Contrôles d'accès aux fichiers et rapports.
- Impossible de modifier une version finalisée via endpoints forgés.
- Vérification des hashes.
- Limites de taille et types de pièces jointes.
- Pas de secrets ou données sensibles inutiles dans les snapshots.
- Tests d'autorisation sur toutes les routes.
- Revue des risques de référence directe par ID (IDOR).
- Vérification de la conservation et du stockage persistant des rapports/preuves.

---

## Ordre de développement figé

```text
US-034 Templates
  -> US-035 Foundation + Snapshot
  -> US-036 Réalisation
  -> US-037 Preuves
  -> US-038 Finalisation + Versions + Hash
  -> US-039 Rapport PDF
  -> US-040 Transmission
  -> US-041 Historique
  -> US-042 Hardening
```

## Définition du périmètre V1

Le moteur V1 est générique : aucun type d'audit (sécurité, qualité, énergétique, etc.) n'est codé en dur. Les utilisateurs définissent le métier via leurs modèles.

La V1 ne promet pas une certification réglementaire ou juridique d'inaltérabilité. Les fonctionnalités de signature, modèles Alouva préinstallés, import/export de templates, logique conditionnelle avancée, scoring sophistiqué et statistiques peuvent être traitées ultérieurement.

## Prochaine étape

Avant l'implémentation de US-034, détailler les invariants et contrats du triptyque `AuditTemplate -> Audit snapshot -> AuditReportVersion`, puis implémenter US-034 par branche dédiée avec migration et tests.
