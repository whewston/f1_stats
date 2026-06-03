# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the csproj first so `restore` is cached unless dependencies change.
COPY src/F1Stats.Api/F1Stats.Api.csproj src/F1Stats.Api/
RUN dotnet restore src/F1Stats.Api/F1Stats.Api.csproj

COPY . .
RUN dotnet publish src/F1Stats.Api/F1Stats.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# The aspnet image ships a non-root user; run as it.
USER $APP_UID

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "F1Stats.Api.dll"]