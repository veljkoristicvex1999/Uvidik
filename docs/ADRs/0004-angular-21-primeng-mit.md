# 0004 — Angular 21 + PrimeNG 21.1.9 (plain MIT), deferring Angular 22

## Status

Accepted. Temporary — revisit once PrimeNG ships MIT-licensed support for a
newer Angular major.

## Context

`docs/FRONTEND_STANDARDS.md` specifies Angular 22 as the baseline. At
implementation time (2026-10-17) PrimeTek had changed PrimeNG's license:
PrimeNG versions compatible with Angular 22 ship under a new commercial
"PrimeUI" license. A free Community License tier exists (eligibility: under
$1M annual revenue, fewer than 5 developers, fewer than 10 employees, under
$3M VC funding — Uvidik qualifies today), but it requires registering an
account with PrimeTek, obtaining a license key, embedding it in app
configuration, and re-attesting eligibility annually. Without a key the app
renders with a cosmetic watermark (does not fail builds or tests).

PrimeNG 21.1.x is the last release under the plain MIT license and officially
supports Angular up to v21 only.

## Decision

Scaffold `frontend/` on **Angular 21** (the LTS release immediately prior to
22) with **PrimeNG 21.1.9**, under the plain MIT license. No PrimeUI account,
no license key, no annual re-attestation process.

This is a deliberate, temporary deviation from the "Angular 22" baseline
stated in `docs/FRONTEND_STANDARDS.md` — chosen to avoid taking on any
license-key/account process this early, even a free one, per the project's
cost-and-complexity-first default (`CLAUDE.md` §0, [`0003-cost-first-mvp.md`](0003-cost-first-mvp.md)).

## Consequences

- `docs/FRONTEND_STANDARDS.md`'s stated "Baseline: Angular 22" carries a
  pointer to this ADR so the written baseline doesn't silently mismatch
  `frontend/package.json`.
- No PrimeNG license key or account exists or is required anywhere in this
  codebase, CI, or deployment. Nothing to rotate, nothing to expire.
- Upgrading to Angular 22 later requires re-deciding this question: either
  PrimeNG has shipped MIT-licensed Angular 22+ support by then (preferred,
  no further action needed), or the team explicitly chooses to register for
  PrimeNG's free Community License (or a paid tier) at that time, following
  the standard approval process in `CLAUDE.md` §0.
- This ADR should be revisited (and superseded or updated) the next time a
  frontend dependency upgrade is considered.

## Alternatives considered

- **Angular 22 + PrimeNG 22 Community License**: matches the docs' stated
  baseline exactly and costs €0, but requires a free PrimeTek account, a
  license key committed to app configuration, and annual re-attestation of
  eligibility. Rejected for now to avoid any account/license-key process
  during this very early, unvalidated stage — can be revisited once there's
  a concrete reason to move to Angular 22.
- **A different component library instead of PrimeNG**: rejected — PrimeNG
  is already the library specified across `docs/ARCHITECTURE.md` and
  `docs/FRONTEND_STANDARDS.md`; switching libraries is a much larger decision
  than picking a version, and nothing here demonstrates a need to revisit it.
