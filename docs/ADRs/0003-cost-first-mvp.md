# 0003 — €0 default recurring cost, explicit approval for anything else

## Status

Accepted.

## Context

Uvidik's market demand is unproven. Potential customers (architecture/urban-planning
bureaus, later possibly investors/developers) have not confirmed they will use or pay
for the product. Spending money — on hosting tiers, paid APIs, commercial UI themes,
managed databases, monitoring services, or AI providers — before that demand is
validated risks funding a product nobody wants.

## Decision

The default target for recurring infrastructure and service cost is **€0**. This
applies to every category of cost, not only "big" ones:

- Hosting/compute, managed database, managed cache/queue, monitoring/observability
  SaaS, paid AI provider usage, commercial UI themes, paid testing infrastructure, and
  any other recurring SaaS/API subscription.
- **Approval is required before adding any new dependency, library, external service,
  or infrastructure component — including free, open-source libraries.** The approval
  requirement is about control over growing complexity and future lock-in, not only
  about money.

Before proposing anything that would break the €0 default, the proposal must state:
the concrete problem it solves, whether an existing built-in platform feature already
solves it, free/cheaper alternatives considered, license/compatibility, operational
complexity it adds, and the actual one-time and recurring cost.

AI usage gets an extra constraint: AI is not the product's core value (data collection/
organization/presentation is) and must never make a paid call without explicit prior
approval. A paid-model proposal must include cost-per-document and expected
monthly-cost estimates, a proposed usage limit, and a description of the non-AI
fallback path.

## Consequences

- Local development uses free tooling: PostgreSQL via Docker Compose, no paid local
  services.
- Architecture choices lean toward "simplest reliable" over "most scalable" —
  see [`0001-modular-monolith.md`](0001-modular-monolith.md) and
  [`0002-clean-architecture.md`](0002-clean-architecture.md) — because operational
  complexity usually correlates with recurring cost (more moving parts to host/monitor).
- A capability that could be postponed without compromising the MVP's core value should
  be postponed, by default, until there's a validated reason to build it.
- When a real business need justifies a cost (e.g. a production hosting tier once there
  are real users, or a paid AI call once its value is demonstrated), it is added
  deliberately, with the proposal above, and the decision is recorded — either as an
  update to this ADR's consequences or a new ADR, so the reasoning isn't lost.
- This rule does not excuse cutting corners on security, data integrity, or
  maintainability — cost-first means picking the cheapest option that is still
  correct and safe, not the cheapest option available.

## Alternatives considered

- **Build for assumed future scale from day one** (paid hosting tier, managed Postgres,
  Redis cache, dedicated worker fleet): rejected — there is no demonstrated need yet,
  and the cost of being wrong about demand is much higher than the cost of migrating
  later if the product succeeds.
- **No explicit approval gate; let implementation choose convenient dependencies as
  needed:** rejected — this is exactly how a low-budget MVP silently accumulates
  recurring cost and dependency risk; an explicit gate keeps every cost/complexity
  addition visible and deliberate.
