# AGENTS.md

## Purpose
Humans and AI agents develop this repository. Implement the requested User Story without reinventing product, domain or architecture.

## Sources of truth
Read in order: User Story and acceptance criteria; `docs/PRODUCT.md`; `docs/DOMAIN_MODEL.md`; `docs/ARCHITECTURE.md`; `docs/ROADMAP.md`; relevant ADRs. Surface conflicts instead of inventing rules.

## Architecture baseline
C# / .NET 9; ASP.NET Core MVC (Controllers + Views, no Razor Pages or Blazor); modular monolith; EF Core + PostgreSQL + Npgsql; ASP.NET Core Identity; NUnit; Testcontainers; EF Core Migrations; GitHub Actions; OpenTelemetry with Prometheus, Loki, Tempo and Grafana. Human approval is required before merge.

Production projects: Web, Application, Domain, Infrastructure. Tests: UnitTests, IntegrationTests, ArchitectureTests. Organize by business module inside projects.

## Dependency rules
Allowed: `Web -> Application -> Domain` and `Infrastructure -> Application / Domain`.
Forbidden: Domain -> Web, Domain -> Infrastructure, Application -> Web.
Controllers stay thin. Business rules belong in Domain/Application.

## Module ownership
Platform owns User, Workspace, WorkspaceMember, ClientPortalAccess, ActivityEvent. CRM owns Customer, Contact, CustomerNote. Documents owns Document, DocumentShare. Billing owns Service, Quote, QuoteLine, Invoice, InvoiceLine, CreditNote, Payment. Compliance owns regulatory rules. Audit is future.

Before adding an entity, verify existing concepts, owner module, Workspace boundary and dependencies.

## Workspace isolation
Workspace is the tenant boundary. Every business resource belongs directly or indirectly to one. Access must include Workspace context. Cross-workspace read/write/delete must be prevented and tested. Identity answers who; Workspace membership/permissions answer what is allowed.

## Persistence and migrations
PostgreSQL + EF Core. Repository migrations are schema source of truth. Persistent model changes require an EF migration in the same PR. Generate, review, test against PostgreSQL and commit it. Never modify production directly. Flag destructive migrations and their strategy.

## TDD and testing
Use Red -> Green -> Refactor where appropriate. Unit-test domain rules, calculations, permissions and transitions. Integration-test EF/PostgreSQL/migrations/repositories/Identity/file storage. Architecture tests guard dependencies. E2E only for critical journeys. Never weaken tests to pass CI.

## Billing
Quote/invoice lines snapshot commercial data. Use deterministic decimal money and explicit rounding. Finalized invoices are not freely mutable. Payment status should derive from payments where practical. Generated PDFs are Documents; billing entities remain source of truth.

## Files and PDFs
Use `IFileStorage`; Domain/Application never access filesystem directly. Initial implementation may be `LocalFileStorage`. Files are private and authorized before access.
Use `IPdfGenerator`; PDF library remains undecided and requires an ADR.

## Compliance and Audit
Compliance rules are explicit, testable, traceable and versionable where needed. Do not introduce speculative Audit abstractions; future Audit reuses CRM, Documents, Platform and PDF capabilities.

## Observability
Structured logs; no secrets/sensitive document contents. ActivityEvents are distinct from technical logs. OpenTelemetry is the application abstraction. Measure before Redis, queues, microservices or Kubernetes.

## No premature abstractions
Do not add generic repositories everywhere, generic manager/service layers, event buses, mediator/CQRS frameworks, mapping frameworks or custom DI frameworks without a concrete current need.

## Code quality
Nullable enabled. Async for I/O. CancellationToken for cancellable I/O/application operations. Explicit date/time semantics. Technical IDs and business numbers are distinct. Intentional error handling.

## Story workflow
Identify Actor, Goal, Value, Acceptance criteria, Module, Domain rules, Security, Workspace isolation, Persistence, Migration, Tests and Observability. Keep PRs small.

Flow: read docs -> identify rules -> tests -> failing test where appropriate -> minimum implementation -> refactor -> integration tests -> migration if needed -> all tests -> review diff -> PR.

## Pull requests
Explain Story, changes, domain/database/security/test/architecture/observability impact. Update affected source-of-truth docs in the same PR. Significant technical decisions require ADRs.

## Definition of Done
Acceptance criteria satisfied; tests pass; architecture rules hold; authorization and Workspace isolation verified; migrations included/tested; docs updated; CI passes; ready for human approval.

## Review agent
Check criteria, domain, architecture, tests, migrations, security, tenant isolation, authorization, dependencies, unnecessary complexity, sensitive logs and breaking changes.
Severity: BLOCKER, MAJOR, MINOR, SUGGESTION.

## Final rule
**Follow the User Story. Protect the Domain. Respect the Architecture. Prove the behavior with tests.**
