# Uvidik — Initial Source Catalog and MVP Risk Register

**Research snapshot date:** 2026-10-17  
**Status:** discovery inputs, not a claim of complete source coverage. Re-check all sources from the development environment before implementing or enabling polling.

## 1. Initial cities and customer

- **Cities:** Niš and Novi Sad.
- **Initial customer:** architecture and urban-planning bureaus.
- **Initial subject area:** urban-planning documents, early public review, public review, and relevant planning-stage changes.
- **Excluded at first:** building permits, cadastral ownership, parcel-level legal conclusions, property listings, general construction opportunities, and nationwide coverage.

## 2. Candidate official sources

### Niš

| Source | URL | Observed evidence | Implementation note |
|---|---|---|---|
| City public-review notices | https://www.ni.rs/category/javna-obavestenja/javni-uvid/ | The official site has a dedicated public-review category. Its recent listings include planning-related public-review reports and notices. | First candidate for a real HTML adapter. Verify page pagination, attachment URLs, dates, current terms, and whether early/public reviews are consistently listed. |
| Planning documents portal linked by the city | https://www.eservis.ni.rs/urbanistickiprojekti/ | The official Niš site links to this under spatial and urban-planning documents. | Direct fetch timed out in the research environment. Verify manually; do not assume available or implement against guessed markup. |
| Objections/comments for plans announced at early public review | https://www.eservis.ni.rs/primedbeplanovi/ | The official Niš site links to this service. | Direct fetch timed out in the research environment. Determine accessibility, terms and scope before implementation. |

Official discovery page: https://www.ni.rs/category/javna-obavestenja/javni-uvid/  
Official site navigation references spatial/urban planning documents and plan comments as related e-services.

### Novi Sad

| Source | URL | Observed evidence | Implementation note |
|---|---|---|---|
| Urbanistic plans index by year | https://skupstina.novisad.rs/regulacioni-planovi/ | The official City Assembly page indexes plan pages from 2013 through 2026. | Use as an index/discovery page. Inspect archive navigation and whether future yearly pages use predictable paths. |
| 2026 urbanistic-plan list | https://skupstina.novisad.rs/urbanisticki-planovi-2026/ | The page lists titled planning documents with dates and linked PDF files. | Good parser fixture candidate. Check whether this URL is the current year's page or if links change from the main index. |
| Early public review | https://skupstina.novisad.rs/rani-javni-uvid/ | Official page exists and links to additional documentation hosted by `nsurbanizam.rs`; no documents were displayed in the page snapshot at research time. | Include as a source candidate, but test real-world behavior, attachments and publication cadence. |
| Public review | https://skupstina.novisad.rs/javni-uvid/ | The official page lists public-review draft decisions and links to documentation hosted by `nsurbanizam.rs`. | Good candidate for monitoring notices and attachments. Verify update pattern and page markup. |
| Urbanism-related document hosting | https://www.nsurbanizam.rs/ | The City Assembly pages include links to download documentation from this domain. | Inspect which files are available, source authority, terms, headers, and whether there is a structured index; treat it as a linked official/municipal source only after verifying the context. |

## 3. Competitive and operational context

- An interactive civil-society platform, “Gde je plan?”, tracks urban-planning plans/projects in Belgrade. It is a reason not to build only another generic map/feed. Uvidik should focus on cross-source monitoring, workspace watchlists, change detection, evidence-backed summaries, source health, and deadline notifications for a professional bureau.
- A future centralized Serbian planning register or platform could improve public data access and reduce the value of basic scraping. The product should be able to replace HTML scraping with a public API/registry feed if one becomes available.
- Official source layouts and attachments may change without notice. Per-source adapters, fixture tests, last-checked timestamps and admin-visible failures are required.
- Public availability does not automatically grant permission to republish entire documents or ignore technical terms. Prefer metadata, extracted evidence, short summaries and links to the official source; review terms before caching or redistributing file contents.

## 4. Source-audit checklist

Before enabling a source in any deployed environment:

- [ ] Confirm it is an official source or that its role as an official linked host is explicit.
- [ ] Check terms of use and robots guidance where applicable.
- [ ] Check for RSS/Atom/API/structured lists before choosing HTML scraping.
- [ ] Confirm unauthenticated access and that no CAPTCHA or other access-control circumvention is needed.
- [ ] Record a small set of representative source pages and attachment types.
- [ ] Identify how the page encodes original publication date, public-review deadline, stage, plan title, city/area, attachments and stable identifiers.
- [ ] Note whether dates appear as `dd.MM.yyyy`, Serbian words, ranges, or other formats.
- [ ] Set a conservative polling interval and request timeout.
- [ ] Add conditional requests/caching when supported.
- [ ] Write parser fixtures and verify a repeated fetch is idempotent.
- [ ] Make failures visible; never report an empty feed when the source request or parser failed.
- [ ] Record last actual successful check and parser version.

## 5. Initial product risks

| Risk | Severity | Why it matters | Mitigation / next action |
|---|---|---|---|
| Incomplete source coverage | High | A relevant notice may be posted on a different official page or published late. | Explicit source coverage statement; source registry; source-health monitoring; interview two or three local architects about which pages they currently check. |
| Niš e-service source unavailable or difficult to parse | High | Two linked e-service pages timed out in research. | Test manually in browser/environment; defer/disable if unstable; do not code from guessed HTML. |
| Deadline extraction error | High | Missing or incorrect public-review deadlines can cause loss of trust. | Evidence-backed extraction, low confidence => unverified, deadline tests and manual confirmation labels. |
| False geographic match | High | A plan title mentioning a broad area does not prove the user's parcel is affected. | Start with city/place/keyword matching; no parcel-level claims without trustworthy official geometry. |
| Duplicate notifications | Medium | Noisy alerts make users disable the product. | Stable source IDs/URLs, hashes, material-change rules, idempotency and retry tests. |
| Cosmetic HTML churn | Medium | A markup change can create false “material change” alerts. | Normalize content and compare meaningful fields/attachment metadata rather than whole-page HTML alone. |
| Parser drift | High | Authorities can alter page markup without warning. | Adapter fixture tests, source health panel, failure state, parse anomaly detection and admin re-run. |
| AI hallucination | High | Summaries/dates could be mistaken for authoritative planning advice. | Link to evidence, schema validation, source excerpts, visible uncertainty and no legal conclusions. |
| Document prompt injection | Medium | Text inside downloaded documents may try to manipulate an AI system. | Treat document contents as untrusted data; no tool access or execution; constrained structured output and validation. |
| Access/legal constraints | High | Public pages can still have terms, load limits, copyright or redistribution limits. | Review terms and source policies; respectful polling; link to originals; disable unclear sources until reviewed. |
| Privacy of workspace watchlists | Medium | Monitoring preferences may reveal client projects or business strategy. | Workspace isolation, authenticated endpoints, limited logs and documented retention. |
| Overbuilding map functionality | Medium | Precise boundaries are more complex than title/location extraction. | Start with city and place/area names; map only verified coordinates/boundaries and label approximations. |
| Commercial demand unverified | High | Architects may already use internal assistants or manually track sources. | Ask two architect contacts to map their existing workflow and test a prototype before major investment. |

## 6. Product validation questions for the first architect interviews

Do not ask only “Would you use this?” Ask:

1. Which exact websites, bulletins, mailing lists, or colleagues currently alert you to planning documents and public reviews?
2. How often do you check them, and how many minutes does it take?
3. Which types of plans and geographic areas are useful to your work?
4. Has a relevant public review or deadline ever been missed or discovered too late? What was the consequence?
5. Is a city/area + keyword alert enough, or do you need actual plan boundaries and parcel intersection?
6. Would a daily digest be useful, or are certain updates urgent enough for immediate alerts?
7. What information must be included for the alert to save time: title, stage, deadline, link, PDF summary, plan boundaries, or change history?
8. Who inside the bureau would own this tool, and what budget or approval process is realistic?
9. Would you run a two-week pilot using actual work interests and give feedback on false positives/missed notices?
10. What would make the tool untrustworthy or unusable?

## 7. Source references

- City of Niš — public review: https://www.ni.rs/category/javna-obavestenja/javni-uvid/
- City of Niš — planning-documents e-service link: https://www.eservis.ni.rs/urbanistickiprojekti/
- City of Niš — plan-objection e-service link: https://www.eservis.ni.rs/primedbeplanovi/
- City of Novi Sad Assembly — plans by year: https://skupstina.novisad.rs/regulacioni-planovi/
- City of Novi Sad Assembly — early public review: https://skupstina.novisad.rs/rani-javni-uvid/
- City of Novi Sad Assembly — public review: https://skupstina.novisad.rs/javni-uvid/
- City of Novi Sad Assembly — 2026 plans: https://skupstina.novisad.rs/urbanisticki-planovi-2026/
- “Gde je plan?” (Belgrade planning map): https://gdejeplan.ministarstvoprostora.org/
