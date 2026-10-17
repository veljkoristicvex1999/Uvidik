# Uvidik — Architecture

> **Status note:** This document describes *intended* architecture for an unvalidated,
> low-budget MVP. Nothing described here is implemented yet unless the repository
> contains the corresponding code — check `backend/` and `frontend/` for actual state
> before assuming a component exists. See [`README_FIRST.md`](../README_FIRST.md).

## 1. Product scope (so architecture decisions have a boundary)

Uvidik watches official urban-planning sources in two pilot cities — **Niš** and
**Novi Sad** — and gives architecture/urban-planning bureaus a single feed of planning
documents, public-review stages, and deadlines, with source evidence and
watchlist-based notifications. Full product context, personas, and non-goals:
[`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §2–3.

Explicitly out of scope for the MVP: building-permit records, cadastral/parcel-level
legal conclusions, national coverage beyond the two pilot cities, paid billing, and any
AI-generated conclusion presented as legal advice.

## 2. Cost-first constraint

Default recurring-cost target is **€0**. Every component below should be read with
that constraint attached — do not introduce a managed service, paid tier, or extra
moving part without the approval process in [`CLAUDE.md`](../CLAUDE.md) §0 and
[`ADR-0003`](ADRs/0003-cost-first-mvp.md).

## 3. Solution layout and project boundaries

A pragmatic modular monolith (not microservices) — see
[`ADR-0001`](ADRs/0001-modular-monolith.md).

```text
backend/
  Uvidik.sln
  src/
    Uvidik.Api/             # HTTP endpoints, auth, middleware, DI composition root
    Uvidik.Application/     # use cases, commands/queries, validation, ports
    Uvidik.Domain/          # entities, value objects, domain rules — zero framework deps
    Uvidik.Infrastructure/  # EF Core/PostgreSQL, source adapters, extraction, AI, email
  tests/
    Uvidik.Domain.Tests/
    Uvidik.Application.Tests/
    Uvidik.Infrastructure.IntegrationTests/
    Uvidik.Api.IntegrationTests/

frontend/
  src/app/
    core/            # app-wide infra: auth, http, layout, configuration
    shared/          # genuinely reusable UI/pipes/utilities
    features/        # dashboard, planning-documents, monitoring, notifications, settings
    app.config.ts
    app.routes.ts

docs/
  ARCHITECTURE.md, BACKEND_STANDARDS.md, FRONTEND_STANDARDS.md, TESTING_STRATEGY.md,
  INITIAL_IMPLEMENTATION_PLAN.md, DAILY_DOCUMENTATION_ROUTINE_PROMPT.md, ADRs/
```

Dependency direction: `Api -> Application -> Domain`, `Infrastructure -> Application, Domain`.
`Api` may reference `Infrastructure` only for DI registration at startup. `Domain` never
references `Application`, `Infrastructure`, ASP.NET Core, EF Core, MediatR, or any
external provider.

Do not add a project (or a frontend top-level folder) "to look enterprise." Add one only
when it carries a real, distinct responsibility.

### Open decision: background job execution

[`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) sketches a separate
`Uvidik.Worker` project for scheduled source polling. For the MVP, default to running
scheduled work as a `BackgroundService`/`IHostedService` hosted inside `Uvidik.Api` —
one fewer moving part, no extra deployment target, and it is trivially split out later
if API request latency or deployment needs demand it. Hangfire/Quartz.NET are *not*
approved by default; if in-process `IHostedService` + a small `scheduled_runs` table
proves insufficient (e.g. need for persisted retry state across restarts, admin UI),
propose the specific gap and get approval before adding either — see
[`CLAUDE.md`](../CLAUDE.md) §0 and [`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §4.

### Domain, Application, Infrastructure, Api — what belongs where

**Domain** — business rules and invariants that are genuine domain decisions:
planning-document lifecycle transitions (e.g. draft → public review → adopted),
watchlist-matching eligibility rules, deadline-related business rules, notification
eligibility. Entities/value objects where they make invalid states unrepresentable.
No domain events, specifications, or aggregates without a concrete requirement.

**Application** — use-case handlers (MediatR where it adds real clarity, not
mechanically for every operation), commands/queries, request validation
(FluentValidation once approved), application ports (`ISourceAdapter`,
`IPlanningDocumentAnalyzer`, `INotificationSender`, …) that Infrastructure implements.
No HTTP/PDF-library/AI-SDK/email-provider details here.

**Infrastructure** — EF Core configurations and migrations, PostgreSQL persistence,
per-source adapters (one adapter per publisher, isolated so one source's failure never
blocks another), HTML/PDF/text extraction, the optional AI provider implementation,
email delivery. Infrastructure implements ports; it never decides whether a user
qualifies for a notification or what a document's lifecycle stage means — that's Domain
policy applied by Application.

**Api** — thin controllers/minimal API endpoints (pick one style, stay consistent),
request/response mapping, auth configuration, middleware registration, composition root.

### Repository rules (also in `BACKEND_STANDARDS.md` and `ADR-0002` — repeated
deliberately because it is easy to violate by accident)

Repositories are simple data-access components: load/persist entities, DB-side
filter/sort/page, encapsulate a genuinely useful query. They must never decide business
transitions or eligibility, call AI/email/source providers, or coordinate a workflow.
No `IGenericRepository<T>` and no repository-per-table by default. Direct EF Core
queries in an Application handler are acceptable for a straightforward read model.

## 4. Middleware and error handling

Prefer built-in ASP.NET Core capabilities. Pipeline order (outer to inner):

1. Exception handling (`UseExceptionHandler` / `IExceptionHandler`, built on
   `Microsoft.AspNetCore.Diagnostics` — ASP.NET Core 8+'s Problem-Details-native
   exception handler) — must be first so it wraps everything below.
2. HTTPS redirection / HSTS (deployed environments).
3. Routing.
4. CORS — configured explicitly per allowed origin, never `AllowAnyOrigin` with
   credentials.
5. Authentication.
6. Authorization.
7. Endpoint execution.

**Error contract:** every non-2xx API response is an RFC 9457 Problem Details JSON body
(`application/problem+json`), produced centrally — not re-implemented per controller.

| Status | Meaning | Example |
|---|---|---|
| 400 | Request fails validation | FluentValidation failure, malformed query param |
| 401 | No/invalid authentication | missing or expired auth cookie/token |
| 403 | Authenticated but not permitted | user outside their workspace |
| 404 | Resource does not exist (or isn't visible to this caller) | unknown planning-notice id |
| 409 | Conflicting/duplicate state | duplicate watchlist name in a workspace |
| 500 | Unexpected failure | unhandled exception, provider outage |

Rules: never expose stack traces, SQL, secrets, or raw provider responses in a response
body. Don't collapse every exception into 400/404 — only use those codes where the
semantics genuinely match. Log unexpected exceptions exactly once, at the exception
handler boundary, with a correlation/trace id (`HttpContext.TraceIdentifier`, or
`Activity.Current?.Id` if distributed tracing is added) included in both the log and the
Problem Details `traceId` extension — never log it twice via middleware *and* the
handler. Never log passwords, tokens, or full request/response bodies. Full standards
and test expectations: [`docs/BACKEND_STANDARDS.md`](BACKEND_STANDARDS.md) §Middleware
and error mapping.

## 5. Source ingestion pipeline

One adapter per publisher, isolated behind a small `ISourceAdapter`-style boundary so a
failure in one source never blocks another. Pipeline (see
[`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §9 for full detail):

1. Scheduler enqueues a source check.
2. Adapter fetches via `IHttpClientFactory` with configured timeout, allowlisted host,
   bounded redirects/size.
3. Raw response metadata + content hash recorded (`SourceFetchRun`).
4. Parser extracts source items/attachments into a source-specific DTO.
5. Normalization produces a candidate `PlanningNotice` + candidate facts.
6. Deduplication via stable publisher id / canonicalized URL / content hash — never
   destructive title-similarity-only matching.
7. Change detection compares meaningful fields/attachments against the last snapshot;
   cosmetic HTML churn must not trigger a "changed" event.
8. New/materially-changed items enter processing; deterministic extraction first, AI
   only when it adds demonstrable value.
9. Watchlist matching against workspace preferences.
10. Idempotent notification creation + retryable delivery.
11. Source/job health metrics updated.

Core domain concepts (kept intentionally small — do not expand into a generic schema
before real source content has been inspected): `MonitoredSource`, `SourceFetchRun`,
`SourceItem`/`PlanningNotice`, `NoticeVersion`/`SourceSnapshot`, `AttachmentReference`,
`ExtractedFact`/`Evidence`, `Watchlist`, `Notification`/`DeliveryAttempt`. Full
definitions: [`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §6.

Mandatory safety rules: no arbitrary user-supplied URLs for server-side fetch (allowlist
only — SSRF defense); respect robots/terms; bounded retries for transient failures only;
a source failure is surfaced to admins, never silently reported as "no new items."

## 6. AI cost control

AI is optional and additive, never the source of truth. Deterministic parsing/extraction
is the default path and must work with AI fully disabled (falls back to a
"summary unavailable / needs review" status). When AI is used:

- Only on new or materially-changed content; cache by content hash + prompt/analyzer
  version; never call twice for the same input.
- Behind an `IPlanningDocumentAnalyzer` port so the provider is replaceable.
- Bounded input/output size, retries, and spend; usage/failures recorded without logging
  full payloads.
- Output is a strictly-typed, server-validated structure; a date/fact is only "verified"
  if the model also returned the supporting source excerpt.
- **Never a paid model call without explicit prior approval** — see
  [`CLAUDE.md`](../CLAUDE.md) §0. Before enabling one: estimate cost/document and
  expected monthly cost, propose a usage limit, and confirm the non-AI fallback works.

Full detail: [`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §8.

## 7. Frontend structure

Angular standalone components, lazy-loaded feature routes, Signals for local
synchronous state, RxJS for async streams, PrimeNG components with a project-owned
design-token layer (no paid theme). Interface language is Serbian Latin; original
source text/script is preserved wherever displayed. Full rules:
[`docs/FRONTEND_STANDARDS.md`](FRONTEND_STANDARDS.md).

## 8. Deployment philosophy

No deployment target is chosen yet, and none should be stood up before it's approved —
see [`CLAUDE.md`](../CLAUDE.md) §0. Local development is Docker Compose for PostgreSQL
(and nothing else unless a concrete need appears). Any hosting choice must default to a
free tier, be documented with its actual recurring cost, and get explicit approval
before use.
