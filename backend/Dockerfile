FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copy file csproj and restore as distinct layers
COPY ["RoomRentalManagerServer.API/RoomRentalManagerServer.API.csproj", "RoomRentalManagerServer.API/"]
COPY ["RoomRentalManagerServer.Application/RoomRentalManagerServer.Application.csproj", "RoomRentalManagerServer.Application/"]
COPY ["RoomRentalManagerServer.Domain/RoomRentalManagerServer.Domain.csproj", "RoomRentalManagerServer.Domain/"]
COPY ["RoomRentalManagerServer.Infrastructure/RoomRentalManagerServer.Infrastructure.csproj", "RoomRentalManagerServer.Infrastructure/"]
# Restore dependencies
RUN dotnet restore "RoomRentalManagerServer.API/RoomRentalManagerServer.API.csproj"
# Copy everything
COPY . ./
# Build and publish a release
WORKDIR /App/RoomRentalManagerServer.API
RUN dotnet publish "RoomRentalManagerServer.API.csproj" -c Release -o /App/out /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /App
COPY --from=build /App/out .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "RoomRentalManagerServer.API.dll"]