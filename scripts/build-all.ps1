# Build backend and frontend.
$root = Split-Path -Parent $PSScriptRoot
$failed = $false

Write-Host "Building backend..."
Push-Location "$root\backend"
dotnet build RoomRentalManagerServer.sln -c Release
if ($LASTEXITCODE -ne 0) { $failed = $true }
Pop-Location

Write-Host "Building frontend..."
Push-Location "$root\frontend"
npm run build
if ($LASTEXITCODE -ne 0) { $failed = $true }
Pop-Location

if ($failed) {
  Write-Error "One or more builds failed."
  exit 1
}

Write-Host "All builds completed successfully."
