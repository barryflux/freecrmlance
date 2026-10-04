# DOMAIN MODEL

This defines business language, ownership, relationships and invariants; it is not a SQL schema.

## Domains
Platform (Identity, Workspace, Permissions, Activity), CRM, Documents, Billing, Compliance, Audit (future).

## Platform
Workspace is tenant/isolation boundary. All business data belongs directly or indirectly to a Workspace. Initially it represents a freelancer activity; later an agency/company/team. It may hold legal, fiscal, contact, logo and billing configuration.

User is an identity and may belong to multiple Workspaces through WorkspaceMember. WorkspaceMember separates identity from membership/role; initial role is OWNER, with MEMBER later. ActivityEvent records meaningful business events, not every click.

## CRM
CRM owns Customer, Contact and CustomerNote. Customer may be a company, professional individual or person and holds legal/admin/tax/contact/status data. Contact belongs to Customer. CustomerNote is internal by default.

## Documents
Documents owns Document and DocumentShare. Future DocumentVersion/Folder only when needed. A Document belongs to a Workspace and may associate with Customer, Quote, Invoice or Audit. Types: USER_DOCUMENT and SYSTEM_DOCUMENT. DocumentShare is explicit authorization; association to Customer does not imply sharing.

## Client Portal
A controlled view over CRM/Documents/Billing, not a data owner. ClientPortalAccess controls external access; authentication mechanism is an architecture concern.

## Billing
Billing owns Service, Quote, QuoteLine, Invoice, InvoiceLine, CreditNote and Payment. Service is a catalog template; lines snapshot commercial data so catalog changes never mutate history.

Indicative Quote lifecycle: DRAFT -> ISSUED -> ACCEPTED, with possible DECLINED/EXPIRED/CANCELLED.

Invoice may be direct or from accepted Quote. A finalized Invoice cannot be freely modified. Payment supports partial/multiple payments; payment state should preferably be derived. Generated PDF is a Document; Invoice/Quote remains source of truth.

## Compliance
Owns regulatory rules, validation, electronic formats, e-invoicing and regulatory traceability.

## Audit
AuditTemplate defines reusable ordered sections and criteria. Audit snapshots a template for one Customer so later template changes never alter existing work. AuditItemResponse stores the progressive response, observation and recommendation for one snapshotted criterion, with update timestamp and user identity. AuditEvidence links a snapshotted audit criterion to a customer Document so evidence reuses the existing file storage while remaining scoped to the audit and Workspace. Audit starts in Draft and moves to InProgress when work is first saved. Future report versions will freeze finalized audit content. Evidence reuses Documents/Platform.

## Ownership
| Concept | Owner |
|---|---|
| User, Workspace, WorkspaceMember, ActivityEvent | Platform |
| ClientPortalAccess | Platform / Portal |
| Customer, Contact, CustomerNote | CRM |
| Document, DocumentShare | Documents |
| Service, Quote, QuoteLine, Invoice, InvoiceLine, CreditNote, Payment | Billing |
| ComplianceRule | Compliance |
| Audit, AuditTemplate | Audit |

## Invariants
1. Workspace isolation is mandatory.
2. Every concept has one owning module.
3. Customer association does not grant document sharing.
4. Commercial lines snapshot historical information.
5. Finalized invoices are not freely mutable.
6. Generated PDFs are not billing source of truth.
7. Audit reuses existing concepts.

Before a new entity, agents check existing concepts, owner module, Workspace boundary and dependencies. If a Story changes the model, update this document in the same PR.


### Audit report versions

Finalizing an audit creates an immutable `AuditReportVersion`. Each version stores a canonical JSON snapshot of the audit stored as text so its exact hashed representation is preserved, its sequential version number, finalization timestamp/user and a SHA-256 integrity hash. Existing versions are never updated. A finalized audit can be explicitly reopened for correction; finalizing it again creates the next version while preserving all previous snapshots. The integrity hash is a technical consistency check, not a legal certification. US-039 must generate reports from a report-version snapshot rather than mutable current audit data.


### Audit PDF reports

A generated audit PDF belongs to one immutable `AuditReportVersion` and is rendered exclusively from that version's snapshot. The generated binary is stored through `IFileStorage`, represented by the existing `Document` entity and referenced by `GeneratedDocumentId`. Regeneration may replace the generated document, but never changes the report-version snapshot or SHA-256 hash. Transmission remains the responsibility of US-040.
