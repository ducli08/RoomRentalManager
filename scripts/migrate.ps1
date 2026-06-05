# Apply EF Core database migrations.
$root = Split-Path -Parent $PSScriptRoot
Set-Location "$root\backend"

Write-Host "Applying EF Core migrations..."
dotnet ef database update `
  --project RoomRentalManagerServer.Infrastructure `
  --startup-project RoomRentalManagerServer.API

if ($LASTEXITCODE -ne 0) {
  Write-Error "Migration failed."
  exit $LASTEXITCODE
}

Write-Host "Migrations applied successfully."
