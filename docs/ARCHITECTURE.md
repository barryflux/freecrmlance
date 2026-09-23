# ARCHITECTURE

## Principle
Freecrmlance is a **modular monolith**. A correctly structured monolith is preferable to premature distributed architecture.

## Stack
C#, .NET 9, ASP.NET Core MVC (Controllers + Views; no Razor Pages/Blazor), EF Core, PostgreSQL, Npgsql, ASP.NET Core Identity, NUnit, Testcontainers, GitHub Actions, OpenTelemetry, Prometheus, Loki, Tempo, Grafana. Runtime version may evolve with its support cycle.

## Solution
```text
Freecrmlance.sln
src/
  Freecrmlance.Web/
  Freecrmlance.Application/
  Freecrmlance.Domain/
  Freecrmlance.Infrastructure/
tests/
  Freecrmlance.UnitTests/
  Freecrmlance.IntegrationTests/
  Freecrmlance.ArchitectureTests/
```
Organize by business domain inside projects.

## Dependencies
`Web -> Application -> Domain`
`Infrastructure -> Application / Domain`
Forbidden: Domain -> Web, Domain -> Infrastructure, Application -> Web. MVC is an adapter; Domain/Application remain reusable by future API/worker/frontend. Controllers stay thin.

## Persistence
Single PostgreSQL initially; optional schemas platform.*, identity.*, crm.*, documents.*, billing.*, compliance.*. Workspace is tenant boundary. Identity answers who; Workspace/permissions answer what is allowed.

## Migrations
EF Core Migrations mandatory for persistent changes. Repo is schema source of truth. No manual production changes. Same PR contains model change + migration. CI validates against temporary PostgreSQL. Destructive migrations require explicit risk/strategy.

## Files
Domain/Application never call filesystem directly. Use `IFileStorage` (StoreAsync/OpenReadAsync/DeleteAsync). Initial `LocalFileStorage`, replaceable later. Files private; authorize before access.

## PDF
Use `IPdfGenerator`. Library undecided; select by ADR considering license, .NET/Linux/container compatibility, rendering, pagination, headers/footers/logo, performance and testing. Generated PDFs become Documents; billing entity stays source of truth.

## Testing
TDD where appropriate: Red -> Green -> Refactor. Unit: domain/application. Integration: EF/PostgreSQL/migrations/repositories/Identity/storage using Testcontainers. Architecture tests guard dependencies. E2E only critical journeys.

## Money
Use deterministic decimal representation and explicit rounding; no binary floating point for business money.

## Observability
ActivityEvent is separate from technical logs. OpenTelemetry decouples app from backends. Prometheus metrics, Loki logs, Tempo traces, Grafana dashboards. Measure before Redis/microservices/queues/Kubernetes.

## Security
No secrets in Git. Workspace checks, authorization, input validation, private files, safe logs and appropriate MVC CSRF protection.

## ADRs
Significant decisions live in `docs/adr/`: modular monolith, Postgres/EF, Identity, local storage, observability, future PDF engine.

**Optimiser pour la simplicité aujourd'hui, protéger le métier pour demain.**

**Simple à développer. Facile à tester. Facile à observer. Difficile à casser. Possible à faire évoluer.**
