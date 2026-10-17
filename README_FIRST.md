# Uvidik Claude Code starter pack

This folder contains the initial product and engineering instructions for Claude Code.

1. Open your Uvidik GitHub repository in Claude Code.
2. Paste the content of `01_UVIDIK_MASTER_PROMPT.md` as the initial task.
3. Ask Claude Code to use the source catalog from `03_INITIAL_SOURCE_CATALOG_AND_RISKS.md` and create the repository documentation described in the master prompt.
4. After the repository has a main branch and the daily documentation files exist, create a Claude Code Routine using the content in `02_DAILY_DOCUMENTATION_ROUTINE_PROMPT.md`. Schedule it once per day at a time corresponding to 18:00 in Europe/Belgrade; verify the scheduler timezone rather than assuming it.
5. Keep routine changes on a branch and review the draft PR before merging.

The source catalog is a dated research snapshot, not a guarantee that any live source works or may be scraped. The agent must test it again and document its results.

## Documentation pack (created in `docs/` and root `CLAUDE.md`)

Following the steps above produced a second, durable layer of documentation that
Claude Code reads on every future session — separate from the one-time starter prompts
above:

- [`CLAUDE.md`](CLAUDE.md) — concise, permanent project rules (cost-first MVP,
  dependency approval, architecture/repository boundaries, middleware/testing
  expectations). Read first, every session.
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — system architecture, project
  boundaries, source-ingestion pipeline, AI cost control, deployment philosophy.
- [`docs/BACKEND_STANDARDS.md`](docs/BACKEND_STANDARDS.md) — concrete .NET/EF
  Core/repository/middleware coding rules.
- [`docs/FRONTEND_STANDARDS.md`](docs/FRONTEND_STANDARDS.md) — Angular/Signals/PrimeNG
  rules.
- [`docs/TESTING_STRATEGY.md`](docs/TESTING_STRATEGY.md) — test layers, priority
  matrix, and edge cases to cover.
- [`docs/INITIAL_IMPLEMENTATION_PLAN.md`](docs/INITIAL_IMPLEMENTATION_PLAN.md) — the
  concrete next phases: source feasibility check, minimal runnable skeleton, then one
  real source-ingestion vertical slice.
- [`docs/DAILY_DOCUMENTATION_ROUTINE_PROMPT.md`](docs/DAILY_DOCUMENTATION_ROUTINE_PROMPT.md)
  — the canonical copy of the daily audit routine prompt (originated from
  `02_DAILY_DOCUMENTATION_ROUTINE_PROMPT.md`; point the scheduled Routine at this copy
  going forward).
- [`docs/ADRs/`](docs/ADRs/) — why the modular-monolith, Clean-Architecture, and
  cost-first decisions were made (`0001`–`0003`).

**This documentation defines intended architecture and working rules — it does not
mean any of it is implemented yet.** As of this writing there is no `backend/` or
`frontend/` code in the repository; check those folders directly before assuming any
described component exists. `docs/INITIAL_IMPLEMENTATION_PLAN.md` Phase 0–1 is the
recommended first real implementation slice: verify source feasibility, then stand up
the minimal runnable skeleton (solution + Angular shell + health endpoint + error
contract) before building any source adapter.

Files referenced by the original master prompt but not yet created
(`docs/PRODUCT.md`, `docs/DOMAIN_MODEL.md`, `docs/DATA_SOURCES.md`,
`docs/SOURCE_ADAPTERS.md`, `docs/SECURITY.md`, `docs/UI_DESIGN.md`,
`docs/ROADMAP.md`, `docs/KNOWN_RISKS.md`, `docs/DAILY_LOG.md`, `docs/decisions/`) are
intentionally deferred — they belong to later implementation milestones, not this
documentation pass, and should be created when the work they document actually exists.
