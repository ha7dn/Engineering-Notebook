SECURITY_CHECK.md

# Security Check — Community Events Hub MVP

> Security pass applied to the **planned design** (no running code yet). This document reviews the plan against common vibe-coding security pitfalls and documents what was checked, what risks were found, and how each is mitigated.

---

## 1. Secrets & API Keys

### What secrets does the app need?

| Secret | Needed? | Details |
|---|---|---|
| Database credentials | **No** | SQLite is a local file — no connection string, no username/password. |
| API keys (third-party) | **No** | The MVP has no external integrations (no email service, no calendar API, no analytics). |
| Session secret | **Maybe** | If `express-session` is used for flash messages, it needs a `SESSION_SECRET`. |
| JWT / Auth tokens | **No** | No authentication in the MVP. |

### How the plan keeps secrets out of the codebase

- **No `.env` file is committed.** The `.gitignore` must include `.env`, `.env.*`, and `data/*.db`.
- If a session secret is needed for flash messages, it should be loaded via `process.env.SESSION_SECRET` and the app should fail fast outside development when it is missing:
  ```javascript
  const sessionSecret = process.env.SESSION_SECRET;
  ```
- **Mitigation**: The plan's setup task explicitly ignores `.env` and `.env.*`, while `.env.example` contains names only. Local secrets are supplied through the environment, never committed.

### Risk Assessment: LOW for local MVP use

The MVP is a local-only app with no external API calls and no production deployment. The secret surface area is minimal.

---

## 2. OWASP Top 10 MVP Triage

This is a planning review against the OWASP Top 10 (2021). The MVP is intended for local or trusted internal use only. **It must not be exposed to the public internet until the Phase 2 access-control and deployment controls are implemented.**

| OWASP risk | MVP decision | Required action |
|---|---|---|
| **A01 Broken Access Control** | **Critical MVP limitation** | Keep organizer pages off public deployment. Before shared deployment, add authentication and authorization so only the event organizer can access attendee data and CSV export. UUIDs are not access control. |
| **A02 Cryptographic Failures** | **MVP control** | Do not commit secrets or attendee data files. Use environment variables for secrets. Use HTTPS before any network deployment; local HTTP is limited to development. |
| **A03 Injection** | **MVP control** | Use parameterized SQLite queries, server-side validation, and EJS escaped output. Never concatenate user input into SQL or raw HTML. |
| **A04 Insecure Design** | **MVP control** | Enforce capacity and per-event email uniqueness in the transactional service and database constraint. Keep attendee data out of public responses. |
| **A05 Security Misconfiguration** | **MVP control** | Run with production-safe error responses, avoid debug output in shared environments, and send `Cache-Control: no-store` on attendee/CSV responses. |
| **A06 Vulnerable and Outdated Components** | **MVP control** | Pin compatible dependency ranges, review dependencies before installation, and run `npm audit` before any shared deployment. |
| **A07 Identification and Authentication Failures** | **Phase 2 required for deployment** | The MVP has no login or email verification. Add organizer authentication, authorization, session hardening, and account recovery before treating manage routes as protected. |
| **A08 Software and Data Integrity Failures** | **Phase 2** | Add locked dependency management, CI checks, and verified build/deployment artifacts before production use. |
| **A09 Security Logging and Monitoring Failures** | **Phase 2** | Add structured security logging, audit events for exports and organizer actions, and alerting before shared production use. Do not log attendee email addresses unnecessarily. |
| **A10 Server-Side Request Forgery** | **Not applicable in MVP** | The MVP makes no user-controlled outbound server requests. Reassess if integrations are added. |

### MVP Security Gate

The build may proceed as a local prototype only if A01 is clearly documented as a limitation and A02–A06 controls are implemented. A deployment to a shared or public environment is blocked until organizer authentication/authorization, HTTPS, CSRF protection, rate limiting, and basic logging are added in Phase 2.

---

## 3. Input Validation

### Malformed Email

| Check | Status | Mechanism |
|---|---|---|
| Empty email | ✅ Covered | `body('email').isEmail()` rejects empty strings |
| Missing `@` symbol | ✅ Covered | `isEmail()` validates RFC 5322 basic format |
| Missing domain (`user@`) | ✅ Covered | `isEmail()` requires a valid domain part |
| Missing local part (`@domain.com`) | ✅ Covered | `isEmail()` requires a local part |
| Extra long email (> 254 chars) | ⚠️ **Gap found** | `isEmail()` alone does not enforce max length |
| **Mitigation** | — | Add `.isLength({ max: 254 })` to the email validator chain |

### Negative / Zero Capacity

| Check | Status | Mechanism |
|---|---|---|
| Capacity = 0 | ✅ Covered | `isInt({ min: 1 })` rejects 0 |
| Capacity = -5 | ✅ Covered | `isInt({ min: 1 })` rejects negatives |
| Capacity = "abc" | ✅ Covered | `isInt()` rejects non-integer strings |
| Capacity = 1.5 | ✅ Covered | `isInt()` rejects floats |
| Capacity = 999999999 | ✅ Covered | Plan specifies `isInt({ min: 1, max: 10000 })` |
| DB-level enforcement | ✅ Covered | `CHECK (max_capacity >= 1)` constraint |

### Empty Title

| Check | Status | Mechanism |
|---|---|---|
| Empty string `""` | ✅ Covered | `trim().notEmpty()` catches this |
| Whitespace-only `"   "` | ✅ Covered | `trim()` reduces to empty → `notEmpty()` rejects |
| Title = `null` / missing | ✅ Covered | `notEmpty()` rejects missing fields |
| Extremely long title | ✅ Covered | `isLength({ max: 200 })` enforces limit |

### Summary of Validation Gaps Found & Fixed

| # | Gap | Severity | Mitigation Added to Plan |
|---|---|---|---|
| V1 | Email max length not enforced | Low | Add `.isLength({ max: 254 })` to email validator |
| V2 | Name field in sign-up could be extremely long | Low | Plan specifies `isLength({ max: 100 })` — ✅ already covered |
| V3 | Event ID format not validated | Low | Add `isUUID()` check on `:id` route parameter to reject non-UUID paths early |

---

## 4. Data Exposure Risks

### Risk 1: Attendee Emails Visible in Public Event List

| Aspect | Assessment |
|---|---|
| **Risk** | If the public event list query (`GET /`) returns attendee emails, any visitor could see all registered emails. |
| **Plan status** | ✅ **Mitigated.** The public list query (`index.ejs`) only shows event metadata and an aggregate count (`spots_left`). No attendee data is fetched or rendered. |
| **Verification** | The SQL query in Section 3 (Flow 3) uses `COUNT(s.id)` — it never selects `s.email` or `s.name`. |

### Risk 2: Attendee Emails Visible on Event Detail Page

| Aspect | Assessment |
|---|---|
| **Risk checked** | The event detail page (`GET /events/:id`) could accidentally expose attendee emails to other attendees if it rendered the sign-up list. |
| **Plan status** | ✅ **Not exposed.** This page shows only event details, available spots, and the sign-up form. It does **not** display attendee names or emails. |
| **Attendee-list location** | Attendee names and emails are shown only on the manage page (`GET /events/:id/manage`), which is intended to be accessible only to organizers once authentication/authorization is added. |
| **Verification** | Flow 2 (Section 3) describes only event metadata, available spots, and the sign-up form. The manage flow separately specifies the attendee list. |

### Risk 3: Organizer View Accessible Without Authentication

| Aspect | Assessment |
|---|---|
| **Risk** | **HIGH.** The organizer view (`GET /events/:id/manage`) displays all attendee names and emails. Since there is no authentication, anyone who knows or guesses the URL can view this sensitive data. |
| **Plan status** | ⚠️ **Partially mitigated.** The plan uses UUID v4 for event IDs, making URLs unguessable (1 in 2^122 chance). However, URLs could be shared, leaked in browser history, or logged by proxies. |
| **Residual risk** | Moderate. Acceptable for an internal MVP among trusted users. |
| **Recommended additional mitigations** | (1) Add a `noindex` meta tag to the organizer view to prevent search engine indexing. (2) Document this as a known limitation. (3) In a future iteration, add a simple access code per event or basic auth. |

### Risk 4: CSV Export Contains Sensitive Data

| Aspect | Assessment |
|---|---|
| **Risk** | The CSV export (`GET /events/:id/manage/export-csv`) returns a file with all attendee names and emails. If the URL is accessed by an unauthorized user, data is exposed. |
| **Plan status** | Same mitigation as Risk 3 (UUID-based URL). The CSV endpoint is nested under the organizer route. |
| **Additional mitigation** | The CSV download should set `Cache-Control: no-store` to prevent caching by proxies or browsers. |

### Risk 5: XSS (Cross-Site Scripting)

| Aspect | Assessment |
|---|---|
| **Risk** | A malicious user could inject JavaScript in the title, description, or name fields, which would execute when other users view the page. |
| **Plan status** | ✅ **Mitigated.** EJS auto-escapes output with `<%= %>` syntax. The plan explicitly notes to use `<%= %>` (not `<%- %>`), and user input is never rendered as raw HTML. |
| **Verification** | Double-check during build that no template uses `<%- %>` for user-provided content. |

### Risk 6: SQL Injection

| Aspect | Assessment |
|---|---|
| **Risk** | If user input is concatenated into SQL queries, an attacker could execute arbitrary SQL. |
| **Plan status** | ✅ **Mitigated.** `better-sqlite3` uses parameterized queries with `?` placeholders. The plan's pseudocode (Section 2) demonstrates this pattern. String concatenation is never used for SQL. |

### Risk 7: CSRF (Cross-Site Request Forgery)

| Aspect | Assessment |
|---|---|
| **Risk** | A malicious site could trick a user's browser into submitting the event creation or sign-up form. |
| **Plan status** | ⚠️ **Not addressed.** The plan does not include CSRF tokens. |
| **Severity** | Low for this MVP: no authentication means CSRF doesn't escalate privilege (anyone can already perform these actions directly). |
| **Recommendation** | Add `csurf` middleware or a similar CSRF protection in a post-MVP iteration when auth is added. Document as a known limitation. |

### Risk 8: Denial of Service (DoS)

| Aspect | Assessment |
|---|---|
| **Risk** | Without rate limiting, an attacker could flood the app with event creation or sign-up requests. |
| **Plan status** | ⚠️ **Documented as out of scope.** The plan acknowledges no rate limiting. |
| **Severity** | Low for internal use; moderate if exposed to the internet. |
| **Recommendation** | Add `express-rate-limit` middleware as a post-MVP step. For the MVP, document this limitation clearly. |

---

## 5. Security Checklist Summary

| # | Check | Status | Notes |
|---|---|---|---|
| S1 | No secrets committed to repo | ✅ Pass | No API keys needed; session secret via env var |
| S2 | `.gitignore` covers sensitive files | ✅ Pass | `node_modules/`, `data/*.db`, `.env` |
| S3 | Email validation | ✅ Pass | `isEmail()` + max length added |
| S4 | Capacity validation | ✅ Pass | `isInt({ min: 1, max: 10000 })` + DB CHECK |
| S5 | Title validation | ✅ Pass | `trim().notEmpty().isLength({ max: 200 })` |
| S6 | No email exposure in public views | ✅ Pass | Only aggregate counts shown publicly |
| S7 | Organizer view access control | ⚠️ Partial | UUID-only protection; no auth. Documented trade-off. |
| S8 | XSS prevention | ✅ Pass | EJS auto-escaping + validator sanitizers |
| S9 | SQL injection prevention | ✅ Pass | Parameterized queries throughout |
| S10 | CSRF protection | ⚠️ Deferred to Phase 2 | Add before shared deployment and alongside authentication |
| S11 | Rate limiting | ⚠️ Deferred | Out of scope for MVP; add `express-rate-limit` post-MVP |
| S12 | HTTPS | ⚠️ Deferred to Phase 2 | Required for any network deployment |

### Overall Assessment: **ACCEPTABLE FOR INTERNAL MVP**

The MVP has one critical deployment limitation: the organizer view and CSV export contain attendee data but are not protected by authentication yet. UUIDs reduce accidental discovery but do not authorize access. The prototype is acceptable for local/trusted use only; Phase 2 must add access control, HTTPS, CSRF protection, rate limiting, logging, and deployment hardening before shared or public exposure.

