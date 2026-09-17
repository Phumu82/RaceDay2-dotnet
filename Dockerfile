# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY RaceDay.sln .
COPY src/RaceDay.Domain/RaceDay.Domain.csproj src/RaceDay.Domain/
COPY src/RaceDay.Infrastructure/RaceDay.Infrastructure.csproj src/RaceDay.Infrastructure/
COPY src/RaceDay.Api/RaceDay.Api.csproj src/RaceDay.Api/
COPY src/RaceDay.Web/RaceDay.Web.csproj src/RaceDay.Web/
COPY tests/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj tests/RaceDay.Api.Tests/

RUN dotnet restore RaceDay.sln

COPY . .

RUN dotnet build RaceDay.sln -c Release --no-restore

RUN dotnet publish src/RaceDay.Api/RaceDay.Api.csproj -c Release -o /app/api --no-build
RUN dotnet publish src/RaceDay.Web/RaceDay.Web.csproj -c Release -o /app/web --no-build

# ---------- Runtime stage: API ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api
WORKDIR /app
COPY --from=build /app/api .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RaceDay.Api.dll"]

# ---------- Runtime stage: Web (MVC) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS web
WORKDIR /app
COPY --from=build /app/web .
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
