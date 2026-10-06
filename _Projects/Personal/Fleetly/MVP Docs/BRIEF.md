## What Is Being Built

An internal web application called Fleetly that allows companies to manage drivers and vehicles licensed to work with the different private transport companies (Uber, Cabify, Freenow, Bolt, Lyft) from the onboarding phase with HR to traffic and fleet management. The app must support multi-location, send notifications to the mobile app, manage schedules, car dispatch and allocation.

This is an **MVP** — the minimal viable product needed to validate the idea and start collecting real usage data. No external integrations, login/authentication system, email ownership verification, or export/import functionality.

## For Whom

- **Fleet managers** (any internal team member): need a frictionless way to publish an event and see who signed up.
- **Drivers** (any internal team member): need a quick way to browse upcoming events and RSVP with just their email.
- **Private transport companies** (stakeholder): needs visibility into which events are popular and how many people are engaging.
## Functional Requirements

### FR-1: Create Event
An organizer can create an event by providing:

| Field         | Type     | Constraints                              |
| ------------- | -------- | ---------------------------------------- |
| `title`       | string   | Required, non-empty, max 200 characters  |
| `date`        | datetime | Required, must be in the future          |
| `description` | string   | Required, non-empty, max 2000 characters |
| `maxCapacity` | integer  | Required, must be ≥ 1                    |
The system assigns a unique identifier upon creation.

### FR-2: Sign-Up (RSVP) with Validation
An attendee can sign up for an event by providing their **email address** and, optionally, their **name**. This is an unauthenticated RSVP: the email is not verified and is not a login credential. The system must validate:
1. **Email format** — reject malformed emails (RFC 5322 basic compliance) and normalize valid emails consistently before storage.
2. **No duplicate sign-ups** — reject if the same normalized email is already registered for the same event.
3. **Capacity enforcement** — reject sign-ups once `currentSignups >= maxCapacity`, with a clear user-facing message.

### FR-3: Public Event List
A publicly accessible page/endpoint that shows **upcoming events** (date ≥ today) sorted ascending by date. Each event card/row displays:
- Title, date, description (or truncated preview)
- Available spots: `maxCapacity - currentSignups`

Past events are excluded from this view.

### FR-4: Organizer View
Per-event detail view showing:
- All event metadata
- Full attendee list (name + email)
- Current sign-up count vs. capacity
- Export: download as CSV **or** one-click copy to clipboard

### FR-5: Organizer View
Per-event detail view showing:
- All event metadata
- Full attendee list (name + email)
- Current sign-up count vs. capacity
- Export: download as CSV **or** one-click copy to clipboard

## Technical Constraints

- **Stack**: free choice — the plan must state and justify it.
- **Persistence**: in-memory is acceptable for the MVP **if documented as a limitation**. File-based or SQLite is preferred.
- **No authentication or email verification**: the MVP does not include login, identity verification, or access control. An email is used only for RSVP validation, duplicate prevention, and organizer export; this is a documented trade-off.
- **Deployment**: not required for the MVP — local `npm run dev` (or equivalent) is sufficient.
- **Time box**: the build session is expected to complete within 2–3 hours.
- **No external services**: no third-party APIs, no email sending, no calendar integration.

## Expected Edge Cases

| #   | Edge Case                                                | Expected Behavior                                                                                  |
| --- | -------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| E1  | Malformed email (e.g., `foo@`, `@bar.com`, `no-at-sign`) | Reject with 400 + clear validation message                                                         |
| E2  | Duplicate sign-up (same email + same event)              | Reject with 409 Conflict + message "Already registered"                                            |
| E3  | Event at full capacity                                   | Reject with 409 + message "Event is full"                                                          |
| E4  | Negative or zero `maxCapacity`                           | Reject event creation with 400 + validation message                                                |
| E5  | Empty or whitespace-only `title`                         | Reject event creation with 400 + validation message                                                |
| E6  | `date` in the past                                       | Reject event creation with 400 + message "Date must be in the future"                              |
| E7  | Non-existent event ID on sign-up                         | Reject with 404 + message "Event not found"                                                        |
| E8  | Description exceeding 2000 characters                    | Reject with 400 + message indicating max length                                                    |
| E9  | Concurrent sign-ups racing for the last spot             | Only one succeeds; the other gets a "full" rejection (documented if not handled atomically in MVP) |
| E10 | XSS in title/description fields                          | All user input is sanitized/escaped before rendering                                               |

## Acceptance Criteria

The MVP is **done** when all of the following are true:

- [ ] **AC-1**: An organizer can create a new event with title, date, description, and max capacity; the event appears in the public list.
- [ ] **AC-2**: An attendee can sign up for an event with a valid email and sees a confirmation message.
- [ ] **AC-3**: A sign-up with a malformed email is rejected with a clear error message.
- [ ] **AC-4**: A duplicate sign-up (same email, same event) is rejected with a "Already registered" message.
- [ ] **AC-5**: A sign-up to a full event is rejected with an "Event is full" message.
- [ ] **AC-6**: The public list shows only upcoming events, sorted by date ascending, displaying available spots.
- [ ] **AC-7**: The organizer view shows the full attendee list for an event with name and email.
- [ ] **AC-8**: The attendee list can be exported as CSV or copied to clipboard.
- [ ] **AC-9**: Event creation with invalid data (empty title, zero/negative capacity, past date) is rejected with appropriate error messages.
- [ ] **AC-10**: No email addresses are exposed in the public event list or to non-organizer views.

