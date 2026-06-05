# Architecture

Room Rental Manager is a monorepo containing a .NET 8 backend and an Angular 19 frontend.

## Structure

```
RoomRentalManager/
├── backend/     ASP.NET Core Web API (Clean Architecture)
├── frontend/    Angular SPA (ng-zorro-antd)
├── database/    Database docs and Docker notes
├── docs/        Shared documentation
└── scripts/     Development helper scripts
```

## Backend (Clean Architecture)

```
RoomRentalManagerServer.API          → Controllers, Program.cs, middleware
RoomRentalManagerServer.Application  → Services, DTOs, interfaces
RoomRentalManagerServer.Domain       → Entities, repository interfaces
RoomRentalManagerServer.Infrastructure → EF Core, Redis, Cloudinary, repositories
RoomRentalManagerServer.Tests        → xUnit tests
```

## Frontend

- Angular 19 standalone components
- NSwag-generated HTTP client (`frontend/src/app/shared/service-proxies.ts`)
- API base URL: `frontend/src/app/config/api.config.ts`

## Local development ports

| Service | URL |
|---------|-----|
| Frontend (ng serve) | http://localhost:4200 |
| Backend (HTTPS) | https://localhost:7246 |
| Backend (HTTP) | http://localhost:5233 |
| Swagger | https://localhost:7246/swagger |
| PostgreSQL (Docker) | localhost:5432 |
| Redis (Docker) | localhost:6379 |

## Data flow

```
Browser (Angular :4200)
    → HTTPS API calls
Backend API (:7246)
    → PostgreSQL (EF Core)
    → Redis (cache)
    → Cloudinary (file storage)
```

CORS is configured in the backend via `Base_URL` (default: `http://localhost:4200`).
