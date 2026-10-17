# Uvidik — Backend standards (.NET)

Companion to [`docs/ARCHITECTURE.md`](ARCHITECTURE.md). These are concrete coding rules,
not aspirational statements — each rule says what to do and why.

## 1. General .NET rules

- .NET 10, nullable reference types enabled solution-wide; treat a new nullable warning
  on code you touch as something to fix, not suppress.
- Use `IHttpClientFactory` for every outbound HTTP call (named/typed clients per source
  adapter), with an explicit `Timeout`, a bounded retry policy (transient failures only,
  with jitter), and a bounded redirect count.
- Inject `TimeProvider` (not `DateTime.Now`/`DateTime.UtcNow` directly) anywhere
  time-dependent business logic exists, so tests can control time deterministically.
- Pass `CancellationToken` through every async call chain that supports one — handler →
  repository/EF Core call → HTTP call. Don't swallow it with `CancellationToken.None`.
- Options classes for configuration, validated at startup (`ValidateOnStart`) rather than
  failing at first use.

## 2. MediatR / CQRS

Use MediatR where a command/query gives a use case a clear, testable boundary — not
mechanically for every CRUD operation. A handler should read as the use case's story:
validate → load → apply domain rule → persist → return a result/DTO. Keep pipeline
behaviors minimal (e.g. validation behavior) — don't build a behavior for something a
five-line `if` would cover just as clearly.

## 3. Validation

FluentValidation (once approved) for command/query input validation, run via a MediatR
pipeline behavior so handlers don't repeat validation boilerplate. Validation failures
map to `400` with a Problem Details body listing the failing fields — never leak the
exception message verbatim if it could contain internal detail.

## 4. Repositories — the rule we repeat everywhere, because it's the one we most want to hold

**A repository is a thin data-access component. It is never where a business decision
gets made.**

Allowed:
- Load/persist an aggregate or entity.
- A named method for a genuinely reusable query (`GetActiveSourcesByCity`, not
  `GetAll`-then-filter-in-memory).
- DB-side filtering, ordering, paging, and projections (`Select` into a DTO) — don't
  pull full entities into memory to filter/page in C#.

Never allowed:
- Deciding a lifecycle transition, matching eligibility, or notification eligibility —
  that's Domain policy, applied by an Application handler.
- Calling an AI provider, email provider, or a source's HTTP endpoint.
- Coordinating a multi-step workflow — that's a MediatR handler's job.
- Becoming a dumping ground for unrelated "service" methods because it's convenient.

Defaults to avoid: no `IGenericRepository<T>`, no repository created automatically for
every `DbSet<T>`/entity. If an Application handler's read is a straightforward EF Core
query (e.g. "get planning notice by id for the API detail screen"), write that query
directly in the handler against `DbContext` — that is the simplest maintainable choice
and does not violate the architecture. Introduce a repository when it genuinely
encapsulates a non-trivial, reused data-access concern (e.g. the dedup/change-detection
query used by multiple ingestion steps).

## 5. EF Core

- One `DbContext` for the monolith unless a real reason to split emerges.
- Configurations via `IEntityTypeConfiguration<T>`, not attributes scattered on
  entities (keeps Domain free of EF Core attributes).
- Migrations are committed to source control and reviewed like code; never auto-apply
  destructive migrations on app startup in a deployed environment. Applying pending
  migrations on local/dev startup is fine.
- Use PostgreSQL-specific indexing appropriate to the actual query pattern (trigram/
  full-text for search) only once a query pattern is real — don't pre-optimize.
- Unique constraints enforce dedup invariants (e.g. source id + canonical URL) at the
  database level, not only in application code.

## 6. Middleware and error mapping

Register in this order (see [`ARCHITECTURE.md`](ARCHITECTURE.md) §4 for the full
rationale): exception handler → HTTPS redirection/HSTS → routing → CORS →
authentication → authorization → endpoints.

Use ASP.NET Core's built-in `IExceptionHandler` + `ProblemDetails` support
(`AddProblemDetails()` / `UseExceptionHandler()`) rather than a hand-rolled middleware,
unless a concrete gap in the built-in behavior is found and documented.

Mapping table (expected — adjust only with a documented reason):

| Exception / situation | Status | Notes |
|---|---|---|
| FluentValidation failure | 400 | field-level Problem Details extension |
| Missing/invalid auth | 401 | no detail beyond "unauthenticated" |
| Authenticated, wrong workspace/role | 403 | never reveal the resource exists if that itself is sensitive |
| Entity not found | 404 | don't distinguish "doesn't exist" vs "exists but not yours" if that distinction would leak information |
| Unique-constraint violation / duplicate op | 409 | catch the specific DB exception, don't guess from message text |
| Anything unhandled | 500 | generic body; log the real exception once at the handler boundary with `traceId` |

Never: convert every exception to 400/404 regardless of meaning; log the same exception
in both a handler's `catch` and the global handler; include SQL text, stack traces, or
provider response bodies in the response body.

## 7. Source adapters

- One adapter class per publisher/source; implement a shared `ISourceAdapter` port from
  Application. A parsing bug in one adapter must not throw an unhandled exception that
  takes down the scheduler loop for other sources — catch at the per-source boundary and
  record a `SourceFetchRun` failure.
- Fixture-based tests per adapter using captured real HTML/PDF samples (see
  [`TESTING_STRATEGY.md`](TESTING_STRATEGY.md)) — never test against the live site.
  Resilient to missing optional fields; a parse failure is recorded, not an empty result.
  Add a circuit breaker/disable flag after N consecutive failures.
- Source hosts are allowlisted in configuration, not accepted as arbitrary input — this
  is the SSRF defense. Enforce response size, content-type, and redirect-count limits
  per fetch.
- Store provenance on every derived fact: source URL, publisher, observed-at (UTC),
  source publication date if present (kept distinct from observed-at), content hash,
  and whether a field is directly extracted vs. AI-derived.

## 8. Security

- ASP.NET Core Identity (or an equally maintained library) for auth; never store tokens
  in browser `localStorage` from the API side (that's a frontend decision too — see
  [`FRONTEND_STANDARDS.md`](FRONTEND_STANDARDS.md)).
- CSRF protection on state-changing cookie-authenticated endpoints.
- Rate limiting on authentication and other public endpoints (`Microsoft.AspNetCore.RateLimiting`,
  built in — no third-party package needed by default).
- Workspace isolation enforced server-side on every workspace-owned resource, not only
  in the UI; cover this with integration tests (cross-workspace access must 403/404).
- Secrets via environment variables / user-secrets / a secrets manager only — never a
  committed `.env` with real values.

## 9. Performance

- Server-side filtering, sorting, and paging for any collection that can grow — never
  page in memory after loading everything.
- No caching layer until a measured need justifies it (see cost-first rule in
  [`CLAUDE.md`](../CLAUDE.md) §0) — in-process `IMemoryCache` is acceptable for a
  genuinely hot, cheap-to-recompute value; no distributed cache (Redis etc.) without
  approval.
