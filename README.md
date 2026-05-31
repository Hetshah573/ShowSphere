# ShowSphere — Full-Stack Cinema Booking Platform

A production-ready BookMyShow-inspired ticket booking platform built with **.NET 8 Clean Architecture**, **React 18 + TypeScript**, and **PostgreSQL on Neon**. Features real-time seat locking via SignalR, Razorpay payments, HMAC-signed QR ticket generation, Google OAuth, JWT auth with refresh tokens, strong password validation, lazy booking expiry, and a full admin panel.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, TypeScript, Vite, TailwindCSS, TanStack Query v5, React Hook Form, Zod |
| Backend | .NET 8, ASP.NET Core, Clean Architecture, MediatR (CQRS), FluentValidation |
| Database | PostgreSQL (Neon cloud) + EF Core 8 + Npgsql 8 |
| Auth | JWT (Access + Refresh tokens), Google OAuth, Role-based (Admin / User) |
| Payments | Razorpay |
<<<<<<< HEAD
| Emails | Brevo Email API (HTTP) |
| Real-time | SignalR (live seat push updates) + polling fallback |
| Scheduler | BackgroundService (daily automated show generation + cleanup) |
=======
| Emails | Gmail SMTP with App Password |
| Real-time | SignalR (live seat availability) |
>>>>>>> 4decd47db4d55b1a1d0bc42e8c65ee8dedbbb4e6
| QR Codes | HMAC-signed QR ticket generation + html5-qrcode scanner |
| Logging | Serilog (console + rolling file) |
| Secrets | DotNetEnv (`.env` file, gitignored) |
| Deployment | Docker on Render (backend) + Vercel (frontend) |

---

## Architecture

```
ShowSphere/
├── backend/
│   ├── Dockerfile                          # Multi-stage Docker build for Render
│   ├── ShowSphere.sln
│   └── src/
│       ├── ShowSphere.Domain/              # Entities, Enums, Interfaces
│       ├── ShowSphere.Application/         # CQRS Commands/Queries, Handlers, Validators, DTOs
│       ├── ShowSphere.Infrastructure/      # EF Core, Migrations, JWT, Email, Payment services
│       └── ShowSphere.API/                 # Controllers, Middleware, Hubs, Program.cs
└── frontend/
    └── src/
        ├── api/          # Axios client + API service functions
        ├── components/   # Reusable UI components (Navbar, MovieCard, Layout, etc.)
        ├── pages/        # All page components
        ├── routes/       # Protected route wrapper
        ├── store/        # Auth + Theme context
        └── types/        # TypeScript type definitions
```

## Database Schema

```mermaid
erDiagram
    Users ||--o{ RefreshTokens : has
    Users ||--o{ Bookings : makes
    Users ||--o{ Reviews : writes
    Users }o--|| Roles : has
    
    Movies ||--o{ Shows : has
    Movies ||--o{ Reviews : receives
    Movies }o--o{ Genres : belongs_to
    Movies }o--o{ Cast : features
    
    Theaters ||--o{ Screens : contains
    Screens ||--o{ Seats : has
    Screens ||--o{ Shows : hosts
    
    Shows ||--o{ Bookings : booked_for
    
    Bookings ||--o{ BookingSeats : reserves
    Bookings ||--o| Payments : paid_by
    
    Seats ||--o{ BookingSeats : reserved_in

    Users {
        guid Id PK
        string Email
        string PasswordHash
        string FirstName
        string LastName
        string Phone
        int RoleId FK
        datetime CreatedAt
        boolean IsActive
    }
    
    Roles {
        int Id PK
        string Name
    }
    
    RefreshTokens {
        guid Id PK
        guid UserId FK
        string Token
        datetime ExpiresAt
        datetime CreatedAt
        boolean IsRevoked
    }
    
    Movies {
        guid Id PK
        string Title
        string Description
        string PosterUrl
        string TrailerUrl
        int DurationMinutes
        string Language
        string Certificate
        datetime ReleaseDate
        boolean IsActive
        decimal Rating
    }
    
    Genres {
        int Id PK
        string Name
    }
    
    Cast {
        guid Id PK
        string Name
        string Role
        string PhotoUrl
    }
    
    Theaters {
        guid Id PK
        string Name
        string Address
        string City
        string State
        string PinCode
        decimal Latitude
        decimal Longitude
        boolean IsActive
    }
    
    Screens {
        guid Id PK
        guid TheaterId FK
        string Name
        int TotalSeats
        string ScreenType
    }
    
    Seats {
        guid Id PK
        guid ScreenId FK
        string Row
        int Number
        string Category
        decimal Price
        boolean IsActive
    }
    
    Shows {
        guid Id PK
        guid MovieId FK
        guid ScreenId FK
        datetime StartTime
        datetime EndTime
        decimal BasePrice
        boolean IsActive
    }
    
    Bookings {
        guid Id PK
        guid UserId FK
        guid ShowId FK
        string BookingNumber
        int TotalSeats
        decimal TotalAmount
        string Status
        datetime BookedAt
        datetime ExpiresAt
        string QRCode
    }
    
    BookingSeats {
        guid Id PK
        guid BookingId FK
        guid SeatId FK
        decimal Price
        string Status
    }
    
    Payments {
        guid Id PK
        guid BookingId FK
        decimal Amount
        string Method
        string TransactionId
        string Status
        datetime PaidAt
    }
    
    Reviews {
        guid Id PK
        guid UserId FK
        guid MovieId FK
        int Rating
        string Comment
        datetime CreatedAt
    }
```

## Features

### Auth & Users
- JWT access tokens (15 min) + refresh tokens (7 days) with rotation
- Google OAuth login (graceful conflict handling for email/Google accounts)
- Register, Login, Forgot Password, Reset Password via Brevo Email API
- Strong password validation — 8+ chars, uppercase, lowercase, digit, special character
- Live password checklist UI on Register, Profile, and Reset Password pages
- Role-based access control (Admin / User)

### Movie Browsing
- Movie listing with search and filter by genre, language, city
- Movie detail page with cast, trailer link, reviews, and aggregated rating
- Wishlist (add/remove movies)
- Hero carousel on homepage

### Booking Flow
- Browse shows by movie + city + date
- Real-time seat selection with SignalR (live push updates on lock/book/cancel)
- Polling fallback for resilience (60s interval)
- EF Core serializable transactions to prevent double-booking
- Pending bookings auto-expire (lazy evaluation on GET)
- Booking confirmation page with countdown timer
- QR code ticket generated using HMAC signature (attached to confirmation email)
- QR code scanner for ticket verification

### Payments
- Razorpay integration (test mode by default)
- Payment record linked to booking on success

### Admin Panel
- Movie CRUD (create, edit, delete, poster/trailer URLs)
- Show scheduling with overlap detection per screen
- Theater and screen management
- Admin dashboard with platform-wide stats

### Automated Show Scheduler
- Background service runs daily at 2:00 AM IST
- Auto-generates shows for the next 7 days across all screens for eligible movies
- Idempotent — skips days/slots that already have shows
- Deactivates past shows with bookings; deletes past shows without bookings
- Sends HTML email report to admin on each run (success/failure summary)

### Notifications
- Booking confirmation email with QR code attachment
- Booking cancellation email
- Password reset email with secure link
- Movie release notification for wishlisted movies

### Infrastructure
- Global exception middleware with structured error responses
- Serilog structured logging (console + rolling file sink)
- Rate limiting on auth endpoints
- Health check endpoint (`/health`)
- Automated show scheduler (BackgroundService, daily at 2 AM IST)
- Secrets managed via `.env` with DotNetEnv — never stored in `appsettings.json`

## Prerequisites

Before running this project, install the following on your machine:

| # | Software | Version | Download Link | Verify Command |
|---|----------|---------|---------------|----------------|
| 1 | **Node.js** (includes npm) | 18+ | https://nodejs.org/ | `node -v` / `npm -v` |
| 2 | **.NET 8 SDK** | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 | `dotnet --version` |
| 3 | **Git** | Any | https://git-scm.com/downloads | `git --version` |
| 4 | **VS Code** (recommended) | Latest | https://code.visualstudio.com/ | — |

### Recommended VS Code Extensions

| Extension | Purpose |
|-----------|---------|
| C# Dev Kit | .NET/C# IntelliSense and debugging |
| ESLint | JavaScript/TypeScript linting |
| Tailwind CSS IntelliSense | Tailwind class autocomplete |


## Quick Start

### Step 1 — Clone
```bash
git clone https://github.com/YOUR_USERNAME/showsphere.git
cd showsphere
```

### Step 2 — Configure backend secrets
```bash
cd backend/src/ShowSphere.API
copy .env.example .env
```
Fill in your values in `.env` (DB connection string, JWT secret, Razorpay keys, Brevo API key, etc.).

### Step 3 — Start backend
```bash
dotnet restore
dotnet run
```
API: **http://localhost:5001** | Swagger: **http://localhost:5001/swagger**

### Step 4 — Start frontend
```bash
cd frontend
npm install
npm run dev
```
Frontend: **http://localhost:5173**

## Environment Variables

### Backend — `backend/src/ShowSphere.API/.env`
```env
ConnectionStrings__DefaultConnection=Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require
Jwt__Secret=YOUR_JWT_SECRET_MIN_32_CHARS
Google__ClientId=YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com
Payment__Razorpay__KeyId=rzp_test_XXXX
Payment__Razorpay__KeySecret=YOUR_RAZORPAY_SECRET
Email__BrevoApiKey=xkeysib-xxxxxxxxxxxx
Email__FromEmail=your-verified-email@gmail.com
QrCode__Secret=YOUR_QRCODE_HMAC_SECRET
Cors__AllowedOrigins=http://localhost:5173,https://your-app.vercel.app
```
See `.env.example` for all keys with descriptions.

### Frontend — set on Vercel dashboard (or local `.env`)
```env
VITE_API_URL=https://your-backend.onrender.com/api
```

## Seed Data (Automatic)

On first `dotnet run` the app automatically seeds:
- Admin + sample user accounts
- 10+ movies with genres and cast
- Theaters, screens, and seats
- Sample bookings, payments, and reviews

Shows are generated automatically by the **Show Scheduler** background service (runs daily at 2:00 AM IST, creates shows for the next 7 days).

## Deployment

### Backend → Render (Docker)
1. New Web Service → connect GitHub repo
2. Root Directory: `backend` | Environment: `Docker`
3. Add all `.env` keys as environment variables in the Render dashboard
4. Set `Cors__AllowedOrigins` to include your Vercel frontend URL

### Frontend → Vercel
1. Import GitHub repo
2. Root Directory: `frontend` | Build Command: `npm run build` | Output: `dist`
3. Add env var: `VITE_API_URL=https://your-app.onrender.com/api`



## Troubleshooting

| Issue | Solution |
|-------|----------|
| `dotnet` not recognized | Install .NET 8 SDK and restart terminal |
| `node` / `npm` not recognized | Install Node.js 18+ and restart terminal |
| Database errors on startup | Check `.env` connection string and Neon DB status |
| JWT errors | Ensure `Jwt__Secret` is at least 32 characters |
| CORS errors | Add frontend URL to `Cors__AllowedOrigins` in `.env` |
| Port 5001 in use | `netstat -ano \| findstr :5001` then `taskkill /PID <pid> /F` |
| Port 5173 in use | Change port in `vite.config.ts` |
| Google login not working | Verify `Google__ClientId` in `.env` matches Google Console |
| Emails not sending | Verify sender email in Brevo dashboard (Settings → Senders & IPs) |

## Design Decisions

- **PostgreSQL on Neon** — cloud-hosted, free tier, supports `pgvector` for future AI features
- **Brevo Email API** — HTTP-based (works on Render which blocks SMTP ports), free 300 emails/day, any recipient
- **SignalR + polling hybrid** — real-time seat updates via WebSocket with 60s polling fallback for resilience
- **Automated show scheduler** — idempotent background service; generates shows daily, cleans up past shows
- **Lazy booking expiry** — pending bookings expire on GET instead of requiring a background job
- **HMAC-signed QR tickets** — tamper-proof without a separate lookup on every scan
- **Serializable transactions** — prevents race conditions on seat booking
- **Npgsql UTC enforcement** — all `DateTime` values explicitly set to `DateTimeKind.Utc` before EF Core queries
- **DotNetEnv secrets** — `.env` loaded at startup; `appsettings.json` has no real secrets
- **Refresh token rotation** — old token invalidated on every refresh
- **Rate limiting** — applied on auth endpoints to prevent brute force
- **Razorpay test mode** — swap `KeyId`/`KeySecret` in `.env` to go live
