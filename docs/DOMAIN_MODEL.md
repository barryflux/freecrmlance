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
Future: AuditTemplate, Audit, AuditSection, AuditControl, AuditEvidence, Finding, Recommendation. Reuse Customer, Document and Platform.

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
