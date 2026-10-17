# Uvidik — Data sources (Phase 0 feasibility findings)

> Seed document per [`docs/INITIAL_IMPLEMENTATION_PLAN.md`](INITIAL_IMPLEMENTATION_PLAN.md)
> Phase 0. Supersedes the reachability assumptions in
> [`03_INITIAL_SOURCE_CATALOG_AND_RISKS.md`](../03_INITIAL_SOURCE_CATALOG_AND_RISKS.md) §2
> with results actually re-checked from the dev environment, as that document requires.
> This covers reachability and rough shape only — it is not an adapter spec. Update it as
> real adapters are built (field-level DOM mapping, pagination handling, polling interval).

**Verification date:** 2026-10-17. **Method:** automated fetch (HTML → text) from this
dev environment, not a full browser session — JavaScript-rendered content would not be
visible to this check. Re-verify in an actual browser before writing adapter code against
a page marked "needs manual recheck" below.

## Niš

| Source | URL | Reachable? | Finding |
|---|---|---|---|
| City public-review notices | https://www.ni.rs/category/javna-obavestenja/javni-uvid/ | Yes (200, no auth/CAPTCHA) | Fetch only returned site navigation/menu structure, no actual notice entries were visible in the extracted content — twice, with different extraction prompts. Inconclusive: either the category is genuinely empty right now, the real listing loads via client-side JS/AJAX past what a markdown-converting fetch sees, or pagination defaults away from a rendered list. **Needs manual recheck in a real browser (view page source vs. rendered DOM) before any adapter work.** |
| Planning-documents e-service | https://www.eservis.ni.rs/urbanistickiprojekti/ | **No** — `ECONNREFUSED` | Confirmed unreachable from this dev environment (not merely a timeout as in the original research). Defer; do not build an adapter against guessed markup. Re-check periodically or ask the city for an alternative access path. |
| Plan-objections e-service | https://www.eservis.ni.rs/primedbeplanovi/ | **No** — `ECONNREFUSED` | Same as above — confirmed unreachable. Deferred. |

**Conclusion for Niš:** no source is confirmed as a real adapter target yet. The two
`eservis.ni.rs` pages are deferred (unreachable). The `ni.rs` public-review category page
is reachable but its actual notice-listing structure is unconfirmed by automated fetch —
resolve with a manual browser check (and check for an RSS/sitemap alternative) before
Niš gets its first real adapter.

## Novi Sad

| Source | URL | Reachable? | Finding |
|---|---|---|---|
| Urbanistic plans index by year | https://skupstina.novisad.rs/regulacioni-planovi/ | Yes | Index page lists years 2013–2026. URL pattern: `urbanisticki-planovi-[YEAR]/` from 2018 onward, `regulacioni-planovi-[YEAR]/` for 2013–2017 (terminology/URL scheme changed around 2018 — do not assume one pattern covers all years). Good discovery/index page for a scheduler to walk. |
| 2026 urbanistic-plan list | https://skupstina.novisad.rs/urbanisticki-planovi-2026/ | Yes | **Best confirmed adapter candidate.** Lists ~11 real planning-document entries, each with a title, a date, and a direct PDF link (sizes observed: 676 KB–33 MB). Date format observed: `d. M. yyyy` (e.g. `7. 9. 2026`) — single-digit day/month, dot-separated, no leading zeros; parse accordingly, don't assume `dd.MM.yyyy`. No separate entry id — the PDF URL/filename is the only stable-ish identifier; dedup key should combine canonicalized PDF URL + title, not id alone. No pagination (single page per year). |
| Early public review | https://skupstina.novisad.rs/rani-javni-uvid/ | Yes | Currently shows "there are no documents at this time" — page mechanism works but has no content right now. Links out to `nsurbanizam.rs/rani_javni_uvid` for the actual documents. Revisit once it has content, or adapt against the `nsurbanizam.rs` page directly. |
| Public review | https://skupstina.novisad.rs/javni-uvid/ | Yes | Same empty-right-now state as early public review; links out to `nsurbanizam.rs/javni_uvid`. |
| Urbanism document host | https://www.nsurbanizam.rs/ | Yes | Structured navigation by plan type/stage (early public review, public review, detail/general regulation plans, master plan, urban projects); a search box; a separate GIS/photo-archive subdomain (`vgis.nsurbanizam.rs`). No obvious RSS/API. Likely the real source of truth behind the two currently-empty `skupstina.novisad.rs` review pages — worth inspecting `nsurbanizam.rs/rani_javni_uvid` and `nsurbanizam.rs/javni_uvid` directly as adapter targets once the city pages above have content, or as a direct source now. |

**Conclusion for Novi Sad:** `https://skupstina.novisad.rs/urbanisticki-planovi-2026/`
(and its yearly siblings via the index page) is a confirmed, reachable, real-content
source — this is the recommended target for Phase 2's first real adapter.

## Phase 2 recommendation

Build the first adapter against the Novi Sad yearly urbanistic-plans page
(`skupstina.novisad.rs/urbanisticki-planovi-<year>/`), since it is the only source with
confirmed, non-empty, structured content reachable from this environment. Niš is deferred
until the public-review category page's real listing structure is confirmed manually and/or
the `eservis.ni.rs` endpoints become reachable. This matches
[`INITIAL_IMPLEMENTATION_PLAN.md`](INITIAL_IMPLEMENTATION_PLAN.md) Phase 2's instruction to
say explicitly when a pilot city has no reachable source yet, rather than inventing one.
