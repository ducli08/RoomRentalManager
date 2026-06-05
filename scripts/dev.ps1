# Start backend and frontend in separate terminals for local development.
$root = Split-Path -Parent $PSScriptRoot

Write-Host "Starting backend API..."
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\backend'; dotnet run --project RoomRentalManagerServer.API --launch-profile https"

Start-Sleep -Seconds 2

Write-Host "Starting frontend dev server..."
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\frontend'; npm start"

Write-Host "Backend: https://localhost:7246/swagger"
Write-Host "Frontend: http://localhost:4200"
