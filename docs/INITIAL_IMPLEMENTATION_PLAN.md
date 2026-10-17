# Uvidik — Initial implementation plan

This is the near-term plan: a minimal skeleton, then one real end-to-end
source-ingestion vertical slice. It does not replace
[`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §15, which lists the full
milestone sequence (Milestones 0–7) through notifications, optional AI, and pilot
hardening — this plan only expands the first steps into concrete, checkable work and
should stay in sync with that milestone list as it executes.

Do not attempt to generate the whole product in one pass. Each phase below should be a
reviewable, working change with tests.

## Phase 0 — Source feasibility check (precedes any code)

- Manually re-verify the candidate Niš/Novi Sad pages in
  [`03_INITIAL_SOURCE_CATALOG_AND_RISKS.md`](../03_INITIAL_SOURCE_CATALOG_AND_RISKS.md):
  reachable without CAPTCHA/auth, actual DOM shape, attachment URL pattern, date format.
  The two `eservis.ni.rs` pages timed out during research — confirm whether they're
  reachable from the actual dev environment before planning an adapter against them.
- Record findings (reachable/not, terms, date formats, stable id scheme) — this becomes
  the seed of a future `docs/DATA_SOURCES.md`.
- **Acceptance:** for at least one source per pilot city, we know concretely whether a
  real adapter is feasible, and any inaccessible source is documented as deferred, not
  silently skipped.

## Phase 1 — Minimal runnable skeleton

- `backend/Uvidik.sln` with the four projects from
  [`docs/ARCHITECTURE.md`](ARCHITECTURE.md) §3 and matching test projects.
- PostgreSQL via Docker Compose for local dev only.
- `GET /health` (liveness) and `GET /health/ready` (DB reachable) using ASP.NET Core's
  built-in health checks.
- Central exception handling + Problem Details wired per
  [`docs/BACKEND_STANDARDS.md`](BACKEND_STANDARDS.md) §6, with one integration test per
  status code in the mapping table.
- EF Core + initial empty migration applies cleanly against the Compose database.
- Angular app shell (`frontend/`) with routing, the `core`/`shared`/`features` folders,
  and one real route (even if it shows a placeholder) — not a default CLI template left
  untouched.
- CI: restore/build/test for both backend and frontend.
- **Acceptance:** a new developer can clone, run `docker compose up`, run backend and
  frontend, and both build/test commands pass locally and in CI.

## Phase 2 — One real source-ingestion vertical slice

- Implement `MonitoredSource` and `SourceFetchRun` persistence (see
  [`docs/ARCHITECTURE.md`](ARCHITECTURE.md) §5 for the concept list).
- Build exactly one adapter against the source confirmed reachable in Phase 0 — real
  HTTP fetch, real parsing, fixture tests captured from the real page. If neither pilot
  city has a reachable source yet, say so explicitly rather than inventing a fake one.
- Persist the normalized `PlanningNotice`, with content hash and dedup key.
- A second, repeated run against the same fixture produces zero duplicates (test this,
  don't just assert it).
- A deliberately broken fixture (malformed HTML) produces a recorded failure, not a
  silent empty result or a crash.
- **Acceptance:** matches Milestone 2's acceptance criteria in
  [`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §15 — a real test run
  creates persisted records, a repeat run is idempotent, and a parser failure is visible.

## Phase 3 and beyond

Follow Milestones 3–7 in
[`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §15 in order: feed/detail
UI → watchlists & change detection → notifications & verified deadlines → optional AI →
geographic UX & pilot hardening. Do not pull a later milestone's scope forward (e.g. no
AI analysis before the ingestion pipeline is stable; no map before source geometry is
confirmed trustworthy).

## Working rules for every phase

- Keep `docs/ROADMAP.md` (once created) in sync with actual status — a milestone is
  "done" only when its acceptance criteria are met, not when the files exist.
- Add tests in the same change as the behavior (see
  [`docs/TESTING_STRATEGY.md`](TESTING_STRATEGY.md)).
- Any new dependency/service follows the approval process in
  [`CLAUDE.md`](../CLAUDE.md) §0 before it's added, not after.
- Report actual command output, not an assumed result.
