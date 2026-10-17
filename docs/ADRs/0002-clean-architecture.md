# 0002 — Clean Architecture with pragmatic DDD-lite

## Status

Accepted.

## Context

The product has genuine business rules worth protecting from framework churn and
provider swaps: planning-document lifecycle transitions, watchlist-matching
eligibility, deadline rules, and notification eligibility. At the same time, this is a
low-budget MVP built largely by one engineer, so any pattern that adds ceremony without
paying for itself (generic repositories, unnecessary aggregates/factories/domain
events, excessive MediatR pipeline behaviors) is a direct cost against shipping speed.

## Decision

Use Clean Architecture with four backend projects and an enforced dependency
direction: `Uvidik.Api -> Uvidik.Application -> Uvidik.Domain`,
`Uvidik.Infrastructure -> Uvidik.Application, Uvidik.Domain`. `Uvidik.Domain` has zero
dependency on ASP.NET Core, EF Core, MediatR, or any external provider — see
[`docs/ARCHITECTURE.md`](../ARCHITECTURE.md) §3 for the full layout and
[`docs/BACKEND_STANDARDS.md`](../BACKEND_STANDARDS.md) for coding rules.

Within that structure, apply DDD *lite*, not full DDD:

- Entities/value objects where they make an invalid state genuinely unrepresentable
  (e.g. a `PlanningNotice` lifecycle stage, a deadline value object with evidence
  status) — not for every record.
- No domain events, specifications, or factories unless a concrete requirement appears.
- MediatR is the default use-case mechanism, but only where a command/query boundary
  adds real clarity — not for every trivial read.

**Repositories are explicitly constrained to data access.** They may load/persist
entities and expose meaningful, named queries with DB-side filtering/sorting/paging.
They may never decide a business transition, matching/notification eligibility, or
coordinate an external provider (AI, email, source HTTP). No `IGenericRepository<T>`
and no repository created automatically per entity/table by default.

**Direct EF Core queries inside an Application handler are explicitly acceptable** for
a straightforward read model (e.g. "fetch this planning notice by id for the detail
screen") when that is the simplest maintainable option and it does not leak EF
Core/PostgreSQL specifics into Domain. This is a deliberate choice to avoid a repository
interface that exists only to wrap a one-line query.

## Consequences

- Domain logic stays testable without a database, HTTP, or AI provider in unit tests.
- Swapping a source adapter's HTTP client, the AI provider, or the email provider
  requires no Domain/Application change — only a new Infrastructure implementation of
  an existing port.
- Fewer files and interfaces than a maximalist Clean Architecture setup, because
  repositories and MediatR are used only where they earn their cost — directly serves
  the cost-first/low-complexity goal.
- The team (today: one person) must actively resist the temptation to put a quick
  business shortcut in a repository or controller "just this once" — code review
  (including this agent's own self-review before committing) must catch that.

## Alternatives considered

- **Anemic model + "service" classes containing all business logic:** rejected — this
  is exactly the repository-as-business-logic anti-pattern the project explicitly wants
  to avoid (see [`CLAUDE.md`](../../CLAUDE.md) §2 and
  [`docs/BACKEND_STANDARDS.md`](../BACKEND_STANDARDS.md) §4).
- **Full DDD (aggregates, domain events, specifications, factories everywhere):**
  rejected for now — no concrete requirement justifies the added ceremony at this
  product stage; revisit per-concept if a real need appears (e.g. domain events once an
  actual integration needs to react to a domain state change asynchronously).
- **Generic repository (`IGenericRepository<T>`) over every `DbSet<T>`:** rejected —
  hides EF Core's actual capabilities (projections, bulk operations, DB-side paging)
  behind a leaky abstraction and does not reduce real complexity here.
