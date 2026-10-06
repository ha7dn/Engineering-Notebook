VERIFICATION_NOTE.md

# Verification Note — Community Events Hub MVP

> This documents **how verification would happen** once the app is built — inputs, expected results, and confirmation methods. No running code exists yet; this is a test plan, not test results.

---

## Test Plan: Three Core Flows

### Flow A: Successful Sign-Up

**Objective**: Verify that a valid attendee can sign up for an event with available capacity and receives confirmation.

| Aspect | Detail |
|---|---|
| **Preconditions** | An event exists with `id = <UUID>`, `maxCapacity = 5`, and `currentSignups = 2` (3 spots left). |
| **Inputs** | `POST /events/<UUID>/signup` with body: `{ email: "alice@wizeline.com", name: "Alice García" }` |
| **Expected Result** | (1) HTTP 302 redirect back to `/events/<UUID>` with a flash success message: "You're signed up! 🎉". (2) The sign-up appears in the `signups` table with `email = "alice@wizeline.com"` and `event_id = <UUID>`. (3) After the POST, `currentSignups` increases from 2 to 3 and the public list shows 2 of 5 spots left, decreased from 3. (4) The organizer view at `/events/<UUID>/manage` includes "Alice García — alice@wizeline.com" in the attendee table. |
| **Confirmation Method** | **Unit test** (`signupService.test.js`): call `signup(eventId, 'alice@wizeline.com', 'Alice García')`, assert it does not throw, then query the DB to confirm the row exists. **Integration test** (`supertest`): `POST` to the endpoint, follow redirect, assert response body contains "You're signed up". **Manual test**: submit the form in the browser, visually confirm the success message and updated spot count. |

---

### Flow B: Rejection When Event Is Full

**Objective**: Verify that a sign-up attempt to a full event is rejected with a clear message.

| Aspect | Detail |
|---|---|
| **Preconditions** | An event exists with `id = <UUID>`, `maxCapacity = 2`. Two sign-ups already exist (`user1@test.com`, `user2@test.com`), so `currentSignups = 2 = maxCapacity`. |
| **Inputs** | `POST /events/<UUID>/signup` with body: `{ email: "user3@test.com", name: "User Three" }` |
| **Expected Result** | (1) The sign-up is **not** created — `signups` table still has exactly 2 rows for this event. (2) The response re-renders the event detail page with error message: "Sorry, this event is full". (3) The sign-up form is visually disabled/hidden. (4) The public list still shows "0 of 2 spots left". |
| **Confirmation Method** | **Unit test**: create an event with `maxCapacity = 2`, insert 2 sign-ups, attempt a third via `signupService.signup()`. Assert it throws `{ status: 409, message: 'Event is full' }`. Query DB to confirm no third row was inserted. **Integration test**: `POST` to the endpoint, assert HTTP 409 (or 302 redirect with error flash), assert response body contains "event is full". **Manual test**: create an event with capacity 2, sign up twice with different emails, then attempt a third — verify the error message appears. |

---

### Flow C: Rejection of Duplicate Email

**Objective**: Verify that signing up with the same email for the same event is rejected.

| Aspect | Detail |
|---|---|
| **Preconditions** | An event exists with `id = <UUID>`, `maxCapacity = 10`. One sign-up exists with `email = "bob@wizeline.com"`. |
| **Inputs** | `POST /events/<UUID>/signup` with body: `{ email: "bob@wizeline.com", name: "Bob Again" }` |
| **Expected Result** | (1) The sign-up is **not** created — still exactly 1 sign-up for `bob@wizeline.com` on this event. (2) The response re-renders the event detail page with error: "This email is already registered for this event". (3) The spot count does **not** change. |
| **Confirmation Method** | **Unit test**: insert a sign-up for `bob@wizeline.com`, call `signup()` again with the same email and event ID. Assert it throws `{ status: 409, message: 'Already registered for this event' }`. Query DB to confirm only 1 row for that email/event pair. **Integration test**: sign up once, then `POST` again with the same email — assert error in response. **Manual test**: submit the form twice with the same email, verify the error message on the second submission. |

---

## Additional Test Scenarios

Beyond the three required flows, the following should also be tested during build:

| # | Scenario | Input | Expected Result | Method |
|---|---|---|---|---|
| T4 | Malformed email | `email: "not-an-email"` | 400 — "Please enter a valid email address" | Unit + manual |
| T5 | Empty title on event creation | `title: ""` | 400 — "Title is required" | Unit + manual |
| T6 | Zero capacity | `maxCapacity: 0` | 400 — "Capacity must be at least 1" | Unit + manual |
| T7 | Past date | `date: "2020-01-01"` | 400 — "Event date must be in the future" | Unit + manual |
| T8 | CSV export | GET `/events/<UUID>/manage/export-csv` | Downloads CSV with correct headers and data | Manual + integration |
| T9 | Non-existent event sign-up | `POST /events/nonexistent-uuid/signup` | 404 — "Event not found" | Integration |
| T10 | Email case normalization | `email: "Alice@Wizeline.COM"` then `email: "alice@wizeline.com"` | Second attempt is rejected as duplicate | Unit |

---

## Planning Mistake Caught & Corrected

### Mistake: Initial plan missed email normalization, allowing case-sensitive duplicate bypass

**What happened**: In the first draft of the plan, the sign-up flow described validating the email with `isEmail()` and checking for duplicates via the `UNIQUE(event_id, email)` constraint. However, the plan did **not** specify that emails should be normalized to lowercase before storage.

**Why this is a problem**: Email addresses are case-insensitive in practice (`Alice@Wizeline.com` and `alice@wizeline.com` should be treated as the same person). Without normalization:
- A user could sign up as `Alice@Wizeline.com` and again as `alice@wizeline.com` — the `UNIQUE` constraint would **not** catch this because SQLite text comparison is case-sensitive by default.
- This would allow the same person to take two spots, undermining the capacity enforcement and duplicating attendee entries.

**How I caught it**: While writing the edge case table, I tested the scenario mentally: "What if the same person signs up with different casing?" I traced the data flow — `isEmail()` validates format but does not normalize case. The `UNIQUE` constraint operates on stored text, which would be case-sensitive. This revealed the gap.

**How it was corrected**: 
1. Added `.normalizeEmail()` to the `express-validator` chain (this lowercases the local part and domain).
2. Updated the `signupService` pseudocode to explicitly call `email.toLowerCase()` before the INSERT as a defense-in-depth measure.
3. Added test scenario T10 (email case normalization) to the verification plan.

**Evidence**: Compare the validator chain in PLAN.md Section 3, Flow 2: `body('email').isEmail().normalizeEmail()` — the `.normalizeEmail()` was added after this catch. The `signupService` pseudocode also shows `email.toLowerCase()` on the INSERT line.

### Additional correction: Missing max-length on email field

While reviewing the SECURITY_CHECK.md validation section, I realized the plan specified `isEmail()` but no maximum length for the email field. An attacker could submit an extremely long technically-valid email to consume storage or cause display issues. This was corrected by adding `.isLength({ max: 254 })` to the email validator chain (254 is the RFC 5321 maximum email length).

