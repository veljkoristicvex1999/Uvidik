# Uvidik — Frontend standards (Angular)

Companion to [`docs/ARCHITECTURE.md`](ARCHITECTURE.md). Baseline: Angular 22 (verify
current LTS/compatibility at implementation time — see
[`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §17 for the release-policy
link to check), standalone components, PrimeNG as the component library.

> **Temporary deviation:** the actual `frontend/` code currently runs Angular 21 +
> PrimeNG 21.1.9 (plain MIT), not Angular 22 — see
> [`docs/ADRs/0004-angular-21-primeng-mit.md`](ADRs/0004-angular-21-primeng-mit.md) for
> why (PrimeNG's license changed; Angular-22-compatible PrimeNG requires a free but
> account-gated Community License). Revisit when that ADR's conditions change.

## 1. Project organization

```text
frontend/src/app/
  core/
    auth/            # auth state, guards, interceptors
    http/             # API base config, error interceptor, typed HTTP clients
    layout/           # shell, nav, responsive layout
    configuration/    # environment/config access
  shared/
    components/       # genuinely reusable UI (buttons, badges, empty-state, etc.)
    pipes/
    utilities/
  features/
    dashboard/
    planning-documents/
    monitoring/
    notifications/
    settings/
  app.config.ts
  app.routes.ts
```

- `core` is for app-wide infrastructure used everywhere; it must not accumulate
  feature-specific logic.
- `shared` is for components/pipes/utilities actually reused across ≥2 features — a
  component used by one feature belongs in that feature's folder, not in `shared`.
- Every feature route is lazy-loaded (`loadComponent`/`loadChildren`).

## 2. State: Signals vs RxJS

- **Signals** for local, synchronous UI state (form state, selected filter, expanded/
  collapsed, loading flags derived from a request lifecycle).
- **RxJS** for asynchronous flows (HTTP calls, websockets/polling if added, combining
  multiple async sources).
- Don't duplicate the same piece of state in both a signal and an `Observable`/`BehaviorSubject`
  — pick one source of truth per value; bridge with `toSignal`/`toObservable` only at
  the boundary where the other form is actually needed.
- No NgRx or another state-management library without a demonstrated need and explicit
  approval (see [`CLAUDE.md`](../CLAUDE.md) §0) — Signals + well-separated injectable
  services are the default for this app's size.
- Unsubscribe/cleanup: prefer `async` pipe, `toSignal`, or `takeUntilDestroyed()` over
  manual `Subscription` bookkeeping.

## 3. API access and error handling

- One typed HTTP client/service per feature's backend resource; no `any` for API
  response shapes — define an interface matching the backend DTO.
- A single `HttpInterceptor` (in `core/http`) maps backend Problem Details error
  responses into a consistent frontend error shape; feature code does not parse
  Problem Details itself.
- API base URL comes from environment configuration (`core/configuration`), never
  hard-coded in a feature component.
- Every data-driven view provides four states: loading, empty (with an actionable empty
  message, not just "no data"), error (with retry where it makes sense), and success.

## 4. PrimeNG and design

- PrimeNG supplies components; Uvidik owns its visual identity on top via CSS custom
  properties/design tokens (`core`-level stylesheet) — typography, spacing, color
  scale, and a single accent color. No paid PrimeNG theme.
- Visual tone: modern, calm, professional — not a generic unmodified admin template
  and not a government-site look. See
  [`01_UVIDIK_MASTER_PROMPT.md`](../01_UVIDIK_MASTER_PROMPT.md) §7 "UX and visual
  direction" for the specific palette/tone guidance (light neutral background, dark ink
  text, a restrained deep teal/forest-green accent).
- Status/deadline indicators must not rely on color alone (add an icon/label) —
  accessibility requirement, not a nice-to-have.

## 5. Internationalization and text

- Interface language is **Serbian Latin**. Source text (titles, excerpts) is preserved
  in its original script/encoding wherever shown — never transliterate displayed source
  text.
- Centralize UI copy enough to avoid scattering duplicate strings across components, but
  do not add a full i18n framework (`@angular/localize`, ngx-translate, etc.) until
  there's a concrete need for more than one UI language.
- Dates are stored in UTC by the backend and displayed in `Europe/Belgrade` time, using
  Serbian date formatting.

## 6. Accessibility

- All interactive elements keyboard-reachable with a visible focus state.
- Forms have associated labels, validation messages tied to the field via `aria-describedby`,
  and errors announced to assistive tech (not just a red border).
- Sufficient color contrast; verify anything using the accent color against WCAG AA.

## 7. Frontend testing

- Unit tests for non-trivial services (HTTP error mapping, watchlist-matching display
  logic, date formatting) and for component state transitions (loading/empty/error/
  success, form validation).
- Mock the HTTP boundary in unit tests (`HttpTestingController` or a fake service) —
  never hit a real backend in a unit test.
- Full layer breakdown and priorities: [`docs/TESTING_STRATEGY.md`](TESTING_STRATEGY.md).
