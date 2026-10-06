PLAN.md

# Delivery Plan — Community Events Hub MVP

> This is the plan/context you'd hand to an AI (or a teammate) to actually build the MVP. A build session should be able to start from this alone, with no further clarification needed.

---

## 1. Architecture & Stack

### Stack

| Layer | Technology | Version |
|---|---|---|
| **Runtime** | Node.js | ≥ 18 LTS |
| **Backend framework** | Express.js | ^4.18 |
| **Persistence** | SQLite via `better-sqlite3` | ^11.0 |
| **Frontend** | Vanilla HTML + CSS + JavaScript | — |
| **Templating** | EJS (Embedded JavaScript) | ^3.1 |
| **Validation** | `express-validator` | ^7.0 |
| **Unique IDs** | `uuid` (v4) | ^9.0 |

### Why This Stack

1. **Express.js** is the most widely understood Node.js framework — minimal boilerplate, excellent for a 2–3 hour time box. No build step required.
2. **SQLite via `better-sqlite3`** gives us real persistence (survives restarts) with zero infrastructure — no DB server to install, single-file storage, synchronous API that avoids callback complexity. It's the lightest step above in-memory that still feels production-adjacent.
3. **EJS templates** let us render server-side HTML without a SPA framework, keeping the build simple and the session focused on logic rather than frontend tooling.
4. **Vanilla CSS** avoids framework lock-in and build-step complexity. For an MVP with ~4 pages, a small hand-written stylesheet is sufficient and faster to iterate.
5. **`express-validator`** gives us declarative, chainable validation middleware — avoiding hand-rolled regex and making the validation rules readable and testable.

### Persistence Approach & Limitations

**Approach**: SQLite database stored as `./data/events.db`. Created automatically on first run. Schema initialized via a `db/init.js` script that runs `CREATE TABLE IF NOT EXISTS` statements.

**Limitations**:
- **Single-process only**: SQLite does not support concurrent writes from multiple processes. Fine for a single-server MVP; would need PostgreSQL/MySQL for horizontal scaling.
- **No migrations framework**: Schema changes require manual SQL. Acceptable for an MVP.
- **No backups**: The `.db` file is not automatically backed up. If deleted, all data is lost.
- **File-system dependent**: The DB file must be writable by the Node process.

### Project Structure

```
community-events-hub/
├── package.json
├── server.js                  # Express app entry point
├── db/
│   └── init.js                # SQLite schema initialization
├── routes/
│   ├── events.js              # Event CRUD routes
│   ├── signups.js             # RSVP routes
│   └── organizer.js           # Organizer view routes
├── middleware/
│   └── validators.js          # express-validator chains
├── services/
│   ├── eventService.js        # Event business logic
│   └── signupService.js       # Sign-up business logic
├── views/
│   ├── layout.ejs             # Base layout (head, nav, footer)
│   ├── index.ejs              # Public event list
│   ├── event-detail.ejs       # Single event + sign-up form
│   ├── create-event.ejs       # Event creation form
│   └── organizer.ejs          # Organizer attendee view
├── public/
│   ├── css/
│   │   └── style.css          # Global styles
│   └── js/
│       └── main.js            # Client-side interactions (copy, export)
├── data/
│   └── events.db              # SQLite DB (auto-created, gitignored)
└── tests/
    └── ...                    # Unit/integration tests
```

---

## 2. Data Model

### SQLite Schema

```sql
CREATE TABLE IF NOT EXISTS events (
    id            TEXT PRIMARY KEY,           -- UUID v4
    title         TEXT NOT NULL,              -- max 200 chars, enforced in app layer
    description   TEXT NOT NULL,              -- max 2000 chars, enforced in app layer
    event_date    TEXT NOT NULL,              -- ISO 8601 string (e.g., "2026-10-15T14:00:00Z")
    max_capacity  INTEGER NOT NULL CHECK (max_capacity >= 1),
    created_at    TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS signups (
    id            TEXT PRIMARY KEY,           -- UUID v4
    event_id      TEXT NOT NULL,
    email         TEXT NOT NULL,              -- normalized to lowercase
    name          TEXT DEFAULT '',            -- optional display name
    created_at    TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (event_id) REFERENCES events(id) ON DELETE CASCADE,
    UNIQUE (event_id, email)                 -- prevents duplicate sign-ups at DB level
);

CREATE INDEX IF NOT EXISTS idx_signups_event_id ON signups(event_id);
CREATE INDEX IF NOT EXISTS idx_events_date ON events(event_date);
```

### How Constraints Are Represented

| Constraint | Enforcement Layer | Mechanism |
|---|---|---|
| **Non-empty title** | App (validator) + DB (`NOT NULL`) | `express-validator`: `body('title').trim().notEmpty()` |
| **Title max length** | App (validator) | `body('title').isLength({ max: 200 })` |
| **Description max length** | App (validator) | `body('description').isLength({ max: 2000 })` |
| **Capacity ≥ 1** | App (validator) + DB (`CHECK`) | `body('maxCapacity').isInt({ min: 1 })` |
| **Future date** | App (validator) | Custom validator: `new Date(value) > new Date()` |
| **Valid email** | App (validator) | `body('email').isEmail().normalizeEmail()` |
| **No duplicate sign-ups** | DB (`UNIQUE` constraint) + App (pre-check) | `UNIQUE(event_id, email)` — app also checks before insert for a friendlier error message |
| **Capacity not exceeded** | App (service layer, atomic check) | Count current sign-ups in a transaction before inserting |

### Capacity Enforcement — Atomic Check

The sign-up service uses a SQLite transaction to prevent race conditions:

```javascript
// signupService.js — pseudocode
function signup(eventId, email, name) {
    const db = getDb();
    const txn = db.transaction(() => {
        const event = db.prepare('SELECT * FROM events WHERE id = ?').get(eventId);
        if (!event) throw { status: 404, message: 'Event not found' };

        const count = db.prepare('SELECT COUNT(*) as c FROM signups WHERE event_id = ?').get(eventId).c;
        if (count >= event.max_capacity) throw { status: 409, message: 'Event is full' };

        const existing = db.prepare('SELECT id FROM signups WHERE event_id = ? AND email = ?').get(eventId, email);
        if (existing) throw { status: 409, message: 'Already registered for this event' };

        db.prepare('INSERT INTO signups (id, event_id, email, name) VALUES (?, ?, ?, ?)')
          .run(uuid(), eventId, email.toLowerCase(), name || '');
    });
    txn(); // executes atomically
}
```

Because `better-sqlite3` is synchronous and single-threaded, the transaction guarantees atomicity within a single Node.js process — no two requests can interleave within the same transaction.

---

## 3. Flows

### Flow 1: Create Event

**Route**: `GET /events/new` → render form | `POST /events` → process creation

| Step | Detail |
|---|---|
| **Screen** | `/events/new` — a form with fields: Title, Date (datetime-local input), Description (textarea), Max Capacity (number input, min=1) |
| **Client-side** | HTML5 `required` attributes + `min="1"` on capacity for immediate feedback |
| **Server-side validation** | `validators.js` chain: title non-empty & ≤200, description non-empty & ≤2000, date is valid ISO & in future, maxCapacity is int ≥1 |
| **On validation failure** | Re-render the form with error messages next to each field and the user's previously entered values preserved |
| **On success** | `eventService.create(title, date, description, maxCapacity)` → inserts into `events` table with a new UUID → redirect to `GET /events/:id` (the event detail page) with a flash success message |

**Endpoints**:
```
GET  /events/new          → renders create-event.ejs
POST /events              → validates + creates → redirects to /events/:id
```

### Flow 2: Sign-Up (RSVP)

**Route**: `POST /events/:id/signup`

| Step | Detail |
|---|---|
| **Screen** | The sign-up form is on the event detail page (`/events/:id`). Fields: Email (required), Name (optional). The form is hidden/disabled if the event is full. |
| **Server-side validation** | `body('email').isEmail().normalizeEmail()`, `body('name').optional().trim().isLength({ max: 100 })` |
| **On malformed email** | Re-render event detail page with error: "Please enter a valid email address" |
| **On duplicate** | Re-render with error: "This email is already registered for this event" |
| **On full event** | Re-render with error: "Sorry, this event is full" |
| **On success** | Insert sign-up → re-render event detail page with success: "You're signed up! 🎉" |

**Endpoints**:
```
GET  /events/:id          → renders event-detail.ejs (includes sign-up form)
POST /events/:id/signup   → validates + registers → redirects back to /events/:id
```

### Flow 3: Public Event List

**Route**: `GET /` (home page)

| Step | Detail |
|---|---|
| **Screen** | A card-based list of upcoming events. Each card shows: title, date (formatted nicely), description preview (first 150 chars), a tooltip containing the full description on hover/focus of the preview, available spots ("X of Y spots left"), and a "View Details" link. The tooltip must be accessible to keyboard users and have its content safely escaped. |
| **Data query** | `SELECT e.*, (e.max_capacity - COUNT(s.id)) AS spots_left FROM events e LEFT JOIN signups s ON e.id = s.event_id WHERE e.event_date >= datetime('now') GROUP BY e.id ORDER BY e.event_date ASC` |
| **Empty state** | If no upcoming events exist, show: "No upcoming events. Check back soon!" |
| **No email exposure** | This view shows **no** attendee data — only event metadata and aggregate counts |

**Endpoints**:
```
GET  /                    → renders index.ejs (public list)
GET  /events              → returns upcoming events sorted by date with available spots
```

### Flow 4: Organizer View

**Route**: `GET /events/:id/manage`

| Step | Detail |
|---|---|
| **Screen** | Full event details at the top + a table of attendees (Name, Email, Sign-up Date). Below the table: "Download CSV" button + "Copy to Clipboard" button. Shows "X / Y registered". |
| **Data query** | Event from `events` table + all sign-ups from `signups` table where `event_id = :id`, ordered by `created_at ASC` |
| **CSV export** | `GET /events/:id/manage/export-csv` — returns a `text/csv` response with headers `Name,Email,Signed Up At`. Content-Disposition header triggers download. |
| **Copy to clipboard** | Client-side JS: formats the table data as tab-separated text (pasteable into spreadsheets) and uses `navigator.clipboard.writeText()`. |
| **Empty state** | If no attendees yet: "No sign-ups yet for this event." |

**Endpoints**:
```
GET  /events/:id/manage             → returns the event manage view with attendee information
GET  /events/:id/manage/export-csv  → returns CSV file download
```

### Route Summary

#### Screen Paths

These `GET` paths render browser-facing screens:

| Path | Screen | Purpose |
|---|---|---|
| `/` | Public event list | Show upcoming events sorted by date with available spots and description previews/tooltips. |
| `/events/new` | Create event form | Collect title, date, description, and maximum capacity. |
| `/events/:id` | Event detail and RSVP form | Show event details, available spots, and the sign-up form. |
| `/events/:id/manage` | Organizer attendee view | Show event metadata, attendee list, registration count, and export controls. |

#### Backend Endpoints

These endpoints process submissions or return exported data:

| Method | Path | Purpose |
|---|---|---|
| `GET` | `/events` | Return upcoming events sorted by date with available spots for clients that consume the event list endpoint. |
| `POST` | `/events` | Validate and create an event, then redirect to its detail screen. |
| `POST` | `/events/:id/signup` | Validate and process an RSVP, rejecting malformed, duplicate, or over-capacity sign-ups. |
| `GET` | `/events/:id/manage` | Return the event manage view with its attendee/sign-up information. |
| `GET` | `/events/:id/manage/export-csv` | Return the event attendee list as a CSV download. |

---

## 4. Edge Cases

| # | Case | Expected Behavior | Implementation |
|---|---|---|---|
| E1 | **Malformed email** (`foo@`, `no-at-sign`, `@bar.com`) | Reject with 400 — "Please enter a valid email address" | `express-validator`'s `isEmail()` — covers RFC 5322 basics |
| E2 | **Negative or zero capacity** (0, -5) | Reject with 400 — "Capacity must be at least 1" | `isInt({ min: 1 })` validator + DB `CHECK (max_capacity >= 1)` |
| E3 | **Empty or whitespace-only title** | Reject with 400 — "Title is required" | `trim().notEmpty()` validator + DB `NOT NULL` |
| E4 | **Event at full capacity** | Reject sign-up with 409 — "Sorry, this event is full" | Transactional count check in `signupService` |
| E5 | **Duplicate sign-up** (same email + same event) | Reject with 409 — "This email is already registered for this event" | App-level pre-check + DB `UNIQUE(event_id, email)` as safety net |
| E6 | **Date in the past** | Reject event creation with 400 — "Event date must be in the future" | Custom validator comparing against `new Date()` |
| E7 | **Non-existent event ID** | Return 404 — "Event not found" | Service layer checks before any operation |
| E8 | **Title > 200 characters** | Reject with 400 — "Title must be 200 characters or less" | `isLength({ max: 200 })` validator |
| E9 | **Description > 2000 characters** | Reject with 400 — "Description must be 2000 characters or less" | `isLength({ max: 2000 })` validator |
| E10 | **XSS in title/description** | User-controlled values are escaped at render time | EJS auto-escapes with `<%= %>` (not `<%- %>`); do not render user input as raw HTML |
| E11 | **SQL injection in any field** | Parameterized queries prevent injection | `better-sqlite3` uses `?` placeholders — never string concatenation |
| E12 | **Concurrent sign-ups for last spot** | Only one succeeds; the other gets "Event is full" | SQLite transaction serializes access (single-writer lock) |
| E13 | **Very large number for capacity** (e.g., 999999999) | Accept but cap at 10000 in validator | `isInt({ min: 1, max: 10000 })` — reasonable for internal events |

---

## 5. Out of Scope / Limitations

### Deliberately Out of Scope for MVP

| Feature | Rationale |
|---|---|
| **Authentication / Authorization** | No login system. Any user can create events or view organizer pages. This is the single biggest trade-off — acceptable for a trusted internal environment, not for production. |
| **Event editing / deletion** | Organizers cannot modify or cancel events after creation. Would add complexity without validating the core idea. |
| **Email notifications** | No confirmation emails or reminders are sent. Would require an email service (SendGrid, SES). |
| **Calendar integration** | No `.ics` download or Google Calendar sync. |
| **User profiles / accounts** | Attendees are identified solely by email. No login, no profile page. |
| **Pagination** | The public list loads all upcoming events. Fine for < 100 events; would need pagination at scale. |
| **Image uploads** | Events don't have banner images or attachments. |
| **Multi-timezone support** | All dates are stored and displayed in a single timezone (server-local or UTC). |
| **RSVP cancellation** | Attendees cannot cancel their sign-up. Would require an auth mechanism or a magic link. |
| **Search / filtering** | No search bar or category filters on the public list. |
| **Rate limiting** | No request throttling. In a trusted internal network this is lower risk, but still a known gap. |
| **HTTPS** | Local dev server runs HTTP only. A deployment would need a reverse proxy with TLS. |

### Known Trade-offs

1. **No auth = anyone can access organizer view**: The `/events/:id/manage` URL is unguessable (UUID) but not protected. Anyone with the link can see attendee emails. This is documented and acceptable for a first internal MVP among trusted users.
2. **SQLite single-process**: Cannot scale horizontally. Sufficient for expected internal load (< 50 concurrent users).
3. **Server-side rendering**: No real-time updates. If two users view the same event, sign-up counts update on page refresh only.

---

## 6. Task Breakdown

An ordered list of build steps an AI session could follow. Each step is designed to be independently testable before moving to the next.

### Phase 1: Project Setup (15 min)
1. **Initialize the project**: `npm init -y`, install dependencies (`express`, `ejs`, `better-sqlite3`, `express-validator`, `uuid`).
2. **Create the folder structure**: `routes/`, `views/`, `public/css/`, `db/`, `services/`, `middleware/`, `data/`.
3. **Set up `server.js`**: minimal Express app with EJS view engine, static file serving, body parser, and a health-check route at `GET /health`.
4. **Add `.gitignore`**: include `node_modules/`, `data/*.db`, and `.env`/`.env.*`; allow a documented `.env.example` with placeholder names only.

### Phase 2: Database & Data Model (15 min)
5. **Create `db/init.js`**: write the `CREATE TABLE IF NOT EXISTS` statements for `events` and `signups`. Export a `getDb()` function that returns the initialized `better-sqlite3` instance (singleton pattern).
6. **Verify DB creation**: start the app, confirm `data/events.db` is created and tables exist.

### Phase 3: Event Creation (30 min)
7. **Create `middleware/validators.js`**: define the `validateCreateEvent` chain (title, description, date, maxCapacity).
8. **Create `services/eventService.js`**: implement `createEvent(title, date, description, maxCapacity)` and `getEventById(id)`.
9. **Create `routes/events.js`**: `GET /events/new` (render form) and `POST /events` (validate + create + redirect).
10. **Create `views/create-event.ejs`**: form with validation error display.
11. **Test**: manually create an event and verify it's in the database.

### Phase 4: Public Event List (20 min)
12. **Add `getAllUpcomingEvents()` to `eventService.js`**: query with LEFT JOIN to get spot counts, filter by future date, order by date.
13. **Create `views/index.ejs`**: card-based event listing.
14. **Create `views/layout.ejs`**: shared HTML structure (head, nav, footer).
15. **Route `GET /`** in `server.js` or a dedicated router.
16. **Test**: verify events appear sorted, with correct available spots.

### Phase 5: Sign-Up (RSVP) (30 min)
17. **Create `services/signupService.js`**: implement `signup(eventId, email, name)` with transactional capacity + duplicate check.
18. **Add `validateSignup` chain to `validators.js`**: email validation + normalization.
19. **Create `routes/signups.js`**: `POST /events/:id/signup`.
20. **Create `views/event-detail.ejs`**: event info + sign-up form + available spots display. Disable form if full.
21. **Route `GET /events/:id`**: render event detail with sign-up count.
22. **Test**: sign up successfully, try duplicate, try when full.

### Phase 6: Organizer View (25 min)
23. **Add `getSignupsByEventId(eventId)` to `signupService.js`**: return list of attendees with name, email, signed-up date.
24. **Create `routes/organizer.js`**: `GET /events/:id/manage` and `GET /events/:id/manage/export-csv`.
25. **Create `views/organizer.ejs`**: attendee table + export buttons.
26. **Implement CSV export**: set `Content-Type: text/csv` header, format rows, set `Content-Disposition: attachment`.
27. **Implement copy-to-clipboard**: client-side JS in `public/js/main.js`.
28. **Test**: verify attendee list renders, CSV downloads correctly, clipboard copy works.

### Phase 7: Styling & Polish (20 min)
29. **Create `public/css/style.css`**: modern, clean design with responsive layout, card components, form styling, flash message styling, and status indicators (spots available, full).
30. **Add flash messages**: implement a simple flash message system for success/error feedback after redirects (can use `express-session` + a simple middleware, or query params for the MVP).
31. **Responsive check**: verify layout works on mobile widths.

### Phase 8: Testing & Verification (25 min)
32. **Write unit tests** for `eventService` and `signupService` (at minimum: create event, sign up, duplicate rejection, capacity enforcement).
33. **Write integration tests** for the main API routes (using `supertest`).
34. **Manual smoke test**: walk through all 4 flows end-to-end.
35. **Edge case sweep**: test all cases from the edge case table.

### Phase 9: Documentation (10 min)
36. **Write `README.md`**: setup instructions, how to run, API routes, known limitations.
37. **Final review**: check all acceptance criteria from `BRIEF.md`.

