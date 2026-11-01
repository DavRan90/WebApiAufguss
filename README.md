# WebApiAufguss  
  
A booking platform for **Aufguss** (sauna infusion) events at Freja Bad & Gym in  
Gnesta kommun. Users register, browse events, book slots, join a reserve  
(waitlist) when full, and manage bookings — including booking for a linked  
"Aufgusskompis" (friend). Admins manage events, news, FAQs, users, settings,  
and audit logs.  
  
## Projects  
  
| Project   | Description |  
|-----------|-------------|  
| `WebApi`  | ASP.NET Core Web API — REST endpoints, Identity + JWT auth, EF Core |  
| `Aufguss` | Blazor WebAssembly client |  
  
## Tech Stack  
  
- **Backend:** ASP.NET Core, Entity Framework Core, ASP.NET Core Identity,  
  JWT Bearer auth  
- **Database:** PostgreSQL (`Npgsql`), connection string `PostgresLocal`  
- **Frontend:** Blazor WebAssembly  
- **Email:** Resend (`ResendEmailService`) plus a background email queue  
  (`IBackgroundEmailQueue` / `EmailBackgroundService`)  
  
## Key Domain Concepts  
  
- **Event** — an Aufguss session with `MaxSlots` capacity; can be `Hidden`  
- **Booking** — a user's slot; `Reserve = true` when waitlisted, `Unbooked`  
  when cancelled  
- **SiteSettings** — singleton config: `MaxBookingsAufguss`,  
  `AufgussHiddenDaysInAdvance`, `MaxBookingsVattenfys`, `MaxNews` (seeded in  
  `ApplicationDbContext`)  
- **AufgussFriend** — a linked companion user auto-booked via  
  `POST /api/bookings/Multi`  
- **BookingManager** (`IBookingManager`) — enforces rules: duplicate check,  
  max active bookings, reserve-list assignment, and promotion on cancellation  
  
## Main API Endpoints (`api/bookings`)  
  
| Route | Auth | Purpose |  
|-------|------|---------|  
| `GET /public-stats` | anonymous | Event capacity vs booked count |  
| `GET /me` | user | Current user's bookings |  
| `GET /validate-token` | anonymous | Validate cancellation token |  
| `POST /cancel-booking` | anonymous | Cancel via email token |  
| `POST /` | — | Create booking |  
| `POST /Multi` | user | Book self + AufgussFriend in one transaction |  
| `POST /{id}/unbook` | user/admin | Unbook |  
  
Also: `api/events`, `api/faqs` (admin CRUD + ordering), `api/users` (admin  
list, `/me` profile), `api/entries` (news), `api/invites`, `api/surveys`.  
