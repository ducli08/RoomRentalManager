# Database

PostgreSQL is used by the backend via Entity Framework Core.

## Migrations location

EF Core migrations live in:

```
backend/RoomRentalManagerServer.Infrastructure/Migrations/
```

Do not duplicate migrations in this folder.

## Local development (monorepo)

From the monorepo root, start PostgreSQL and Redis with Docker:

```powershell
docker compose up postgres redis -d
```

Or use your existing remote PostgreSQL/Redis configured in `backend/RoomRentalManagerServer.API/appsettings.json`.

## EF Core commands

Run from the `backend/` directory:

```powershell
cd backend

# Apply all pending migrations
dotnet ef database update --project RoomRentalManagerServer.Infrastructure --startup-project RoomRentalManagerServer.API

# Add a new migration
dotnet ef migrations add <MigrationName> --project RoomRentalManagerServer.Infrastructure --startup-project RoomRentalManagerServer.API
```

## Docker Compose defaults

When using `docker compose up`, the API service receives:

| Variable | Value |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | `Host=postgres;Port=5432;Database=roomrental;Username=postgres;Password=postgres` |
| `Redis__ConnectionString` | `redis:6379` |

These override `appsettings.json` at runtime inside the container.
