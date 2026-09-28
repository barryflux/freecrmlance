# ADR 0001 — PDF generation with PDFsharp/MigraDoc

## Status
Accepted

## Context
Freecrmlance needs server-side PDF generation for quotes. The implementation must remain behind `IPdfGenerator`, work with .NET 9, and avoid coupling Application or Domain to a PDF vendor.

## Decision
Use the stable `PDFsharp-MigraDoc` Core package, version 6.2.4, in Infrastructure.

MigraDoc provides the high-level document/table layout and PDFsharp performs rendering. The Core package is cross-platform and MIT licensed.

The generated quote PDF is created on demand and is not persisted in document storage in US-020.

## Consequences
- Domain and Application remain independent from PDFsharp/MigraDoc.
- Infrastructure owns the concrete PDF dependency.
- PDF rendering is suitable for Windows and Linux deployments.
- The renderer explicitly configures PDFsharp's `FailsafeFontResolver` and uses its Segoe WP fallback so Core rendering does not depend on Windows fonts.
- This is intentionally sufficient for the initial quote template; custom branding/fonts can later replace the fallback resolver with packaged font assets.
- Branding, templates, PDF archival and public sharing remain separate concerns.
