# Room Rental Manager

Monorepo for the Room Rental Manager application — .NET 8 backend and Angular 19 frontend.

## Structure

| Folder | Description |
|--------|-------------|
| [`backend/`](backend/) | ASP.NET Core Web API (.NET 8) |
| [`frontend/`](frontend/) | Angular 19 SPA |
| [`database/`](database/) | Database documentation and EF migration guide |
| [`docs/`](docs/) | Architecture and shared docs |
| [`scripts/`](scripts/) | Development helper scripts |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for PostgreSQL/Redis)

## Quick start (local)

### 1. Backend

```powershell
cd backend
dotnet restore RoomRentalManagerServer.sln
dotnet run --project RoomRentalManagerServer.API --launch-profile https
```

Swagger: https://localhost:7246/swagger

### 2. Frontend

```powershell
cd frontend
npm install
npm start
```

App: http://localhost:4200

### 3. Both at once

```powershell
.\scripts\dev.ps1
```

## Docker

Build images from monorepo root:

```powershell
docker build -f backend/Dockerfile -t room-rental-api backend/
docker build -f frontend/Dockerfile -t room-rental-web frontend/
```

Run full stack (PostgreSQL, Redis, API, Web):

```powershell
docker compose up --build
```

| Service | URL |
|---------|-----|
| Web | http://localhost:4200 |
| API | http://localhost:8080 |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |

## Database migrations

See [`database/README.md`](database/README.md) or run:

```powershell
.\scripts\migrate.ps1
```

## NSwag (regenerate API client)

Backend must be running at https://localhost:7246:

```powershell
cd frontend
npm run nswag:generate
```

## VS Code

Open the multi-root workspace:

```
RoomRentalManager.code-workspace
```

## Legacy repositories

This monorepo replaces the separate repositories:

- [RoomRentalManagerServer](https://github.com/ducli08/RoomRentalManagerServer) (archived)
- [RoomRentalManagerClient](https://github.com/ducli08/RoomRentalManagerClient) (archived)

Backup branches `pre-monorepo-backup` exist on both legacy repos.
