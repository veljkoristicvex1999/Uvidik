# Uvidik — Testing strategy

Broad, meaningful coverage — not an arbitrary percentage. Every test must be
deterministic, affordable (no paid API calls, no live government sites, no real email),
and runnable in CI without secrets.

## 1. Backend unit tests (xUnit)

Target: domain invariants and application orchestration, isolated from EF Core/HTTP.

- Planning-document lifecycle transitions (valid/invalid state changes).
- Watchlist matching and eligibility — including the "why did this match" explanation.
- Deadline-related business rules, using an injected `TimeProvider`, including ambiguous/
  missing dates and timezone edge cases (UTC storage vs. `Europe/Belgrade` display, DST
  transitions, end-of-day semantics for a deadline date with no time component).
- Source parser behavior per adapter, against captured HTML/PDF fixtures — not live
  sites. Cover missing optional fields and unexpected markup, not just the happy path.
- URL canonicalization and stable deduplication logic.
- Content-hash / change-detection rules — including a case proving cosmetic HTML churn
  does *not* trigger a "materially changed" event.
- Notification idempotency: the same source event must not produce two notification
  records.
- AI-output validation: malformed JSON, missing evidence excerpt for a claimed date,
  timeout, and the no-API-key fallback path.

## 2. Backend integration tests

Use a real PostgreSQL test database (never EF Core's in-memory provider for anything
that depends on PostgreSQL semantics — constraints, case-sensitivity, etc.).

- EF Core mappings and migrations apply cleanly.
- Unique constraints actually prevent the duplicate they're meant to prevent.
- Pagination, filtering, and sorting at the API layer return correct, bounded results.
- API error contract: actual HTTP status, `application/problem+json` content type, and
  a body that contains no stack trace/SQL/secret.
- Authorization: a user from workspace A cannot read/write workspace B's data (403/404
  as appropriate).
- Source-adapter failure isolation: one adapter throwing does not prevent others from
  completing in the same scheduled run.

## 3. Frontend tests

- Services: HTTP error-interceptor mapping, date/locale formatting, watchlist-match
  explanation rendering.
- Components: loading/empty/error/success states for the feed and detail views, form
  validation for the watchlist form, notification read/unread toggling.
- Mock the HTTP boundary; no test depends on a running backend.

## 4. End-to-end tests (Playwright)

Add once there's enough real functionality to justify them — not before. Seed data via
a controlled test fixture/local service, never a live government site.

1. Open the planning-document feed and see seeded items.
2. Filter/search and open an item's detail, following the link to its original source.
3. Create or update a watchlist and see it match a seeded notice, with the match reason
   shown.
4. Poll a fixture source twice: the first run creates one notification, the repeat run
   creates zero duplicates.

## 5. Test-priority matrix

| Area | Priority | Why |
|---|---|---|
| Source parsing (per adapter, fixture-based) | High | Source markup drift is the most likely silent failure mode |
| Duplicate detection | High | Directly causes notification spam, the #1 trust-killer |
| Content-change detection (material vs. cosmetic) | High | False "changed" alerts erode trust fast |
| Deadline extraction edge cases | High | A wrong/missed deadline is the core promise of the product |
| Source failure isolation & visibility | High | A silent failure looks identical to "no new plans" |
| Notification idempotency | High | Repeat polling must not double-notify |
| API error responses (status/contract) | Medium | Needed once real clients depend on the contract |
| Database constraints | Medium | Backstop for the dedup/uniqueness invariants above |
| Workspace authorization | High | Cross-tenant leakage is a hard security failure |
| Critical frontend flows (feed, filter, detail, watchlist) | Medium | Core usable path, but UI bugs are cheaper to recover from than data-integrity bugs |
| AI output validation | Medium | Only matters once AI is enabled; must stay mocked/deterministic in CI |

## 6. Rules

- Use fixtures and an injectable clock; never depend on "now" or a live network call in
  a unit/integration test.
- A test suite must not require a paid AI key, real email credentials, or network
  access to a government site to run in CI.
- Do not weaken an assertion or skip a test to make a build green — fix the underlying
  behavior or report the failure honestly.
- Add tests in the same change as the behavior they cover, not as a follow-up.
