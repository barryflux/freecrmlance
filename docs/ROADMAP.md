# ROADMAP

`PRODUCT -> ROADMAP -> EPICS -> USER STORIES -> GITHUB ISSUES -> DEVELOPMENT`

## Phase 0 — Foundation
- 0.1 Product Definition — DONE
- 0.2 Domain Model — DONE
- 0.3 Architecture — DONE
- 0.4 Roadmap — DONE
- 0.5 AI Development Workflow — DONE
- 0.6 Initial Backlog — NEXT: Epics Platform, CRM, Documents, Billing; first Platform/CRM Stories.

Exit: a Story can be handed to an agent without inventing architecture, business rules, tests, migrations or structure.

## Phase 1 — Platform Foundation
.NET solution/MVC/projects/tests; PostgreSQL/EF/Npgsql/migrations; Identity; Workspace/User/WorkspaceMember; tenant isolation; observability; CI.
Exit: account -> login -> Workspace on tested foundation.

## Phase 2 — CRM
Customers, contacts, internal notes, Customer Dossier, ActivityEvents, tenant/rule/permission/archive tests.
Exit: basic CRM without external tool.

## Phase 3 — Documents
Upload/download/delete/archive, Customer association, IFileStorage -> LocalFileStorage, metadata in PostgreSQL, private authorization.
Exit: secure document centralization.

## Phase 4 — Client Document Sharing
DocumentShare, ClientPortalAccess, explicit share/revoke, secure client view/download.
Exit: controlled document transmission.

## Phase 5 — Quotes
Service catalog; Quote/lines; quantities/prices/taxes/discounts/totals; lifecycle; IPdfGenerator selected via ADR; PDF as Document.
Exit: professional quote + PDF.

## Phase 6 — Billing
Direct/from-quote invoice, numbering, dates, lifecycle, PDF, finalized immutability, Customer Dossier integration.
Exit: customer -> quote -> invoice base flow.

## Phase 7 — Payments
Record date/amount/reference/method; partial/multiple payments; derive UNPAID/PARTIALLY_PAID/PAID.
Exit: know invoiced, paid and outstanding.

## Phase 8 — Client Portal Enrichment
Authorized documents/quotes/invoices/payment status; later quote actions/uploads. Projection, not second domain.

## Phase 9 — Compliance & Electronic Invoicing
Explicit/testable/traceable/versionable rules, validation, formats and integrations. Research exact regulation at implementation time.

## Phase 10 — Audit
AuditTemplate/Audit/sections/controls/evidence/findings/recommendations/reports; reuse CRM/Documents/Platform/PDF.

## Phase 11 — Automation & Integrations
Email, reminders, external storage, accounting, payments, webhooks, jobs only on real need.

## MVP target
Phases 0–7: Account -> Workspace -> Customer -> Contacts -> Documents -> Sharing -> Quote -> Invoice -> Payment Tracking.

Future functionality must not complicate the current phase.
