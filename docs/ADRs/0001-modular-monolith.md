# 0001 — Modular monolith, not microservices

## Status

Accepted.

## Context

Uvidik is an unvalidated MVP with a solo backend-focused founder and a default
recurring-cost target of €0 (see [`0003-cost-first-mvp.md`](0003-cost-first-mvp.md)).
We don't yet know whether the product has real customers, let alone whether it will
need to scale past a handful of bureau workspaces. The system does have several
distinct concerns — source ingestion, document extraction, optional AI analysis,
watchlist matching, notifications — which could tempt a service-per-concern split.

## Decision

Build a single deployable backend solution (a "modular monolith"): one ASP.NET Core
Web API process, structured internally into `Api`, `Application`, `Domain`, and
`Infrastructure` projects with enforced dependency direction (see
[`0002-clean-architecture.md`](0002-clean-architecture.md)), plus a separate Angular
frontend. No microservices, no message broker, no service mesh, no per-concern
deployable unit.

Scheduled background work (source polling, notification delivery) runs as an
`IHostedService`/`BackgroundService` inside the same API host rather than a separate
Worker service/deployment, unless a concrete operational problem (e.g. request latency
contention, need to scale ingestion independently) proves otherwise.

## Consequences

- One thing to build, test, deploy, and pay to host — directly serves the €0/low-cost
  goal and a solo developer's limited time.
- Internal module boundaries (the four projects + per-source adapter isolation) still
  give us most of the testability and separation-of-concerns benefit of services,
  without the operational cost of distribution (network calls, partial failure
  handling, service discovery, distributed tracing).
- A source adapter's failure must still not take down the whole process — that
  isolation is enforced at the code level (try/catch per adapter + recorded
  `SourceFetchRun` failure), not by process boundaries.
- If a specific concern later proves it needs independent scaling or deployment (e.g.
  ingestion volume genuinely outgrows the API host), split it out then, informed by
  real measurement — not preemptively. Revisit this ADR if that happens.

## Alternatives considered

- **Microservices per concern (ingestion, AI, notifications, API):** rejected —
  multiplies deployment/ops cost and complexity with no current justification; directly
  conflicts with the cost-first MVP constraint.
- **Separate Worker deployment for scheduled jobs** (as sketched in an earlier draft of
  [`01_UVIDIK_MASTER_PROMPT.md`](../../01_UVIDIK_MASTER_PROMPT.md) §4/§15): deferred —
  an in-process `BackgroundService` is simpler to run and deploy for the MVP's expected
  load; revisit if persistence/scaling needs outgrow it.
