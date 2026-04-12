# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/DomainQueryEngine.SearchUI/DomainQueryEngine.SearchUI.csproj src/DomainQueryEngine.SearchUI/
COPY src/DomainQueryEngine.Application/DomainQueryEngine.Application.csproj src/DomainQueryEngine.Application/
COPY src/DomainQueryEngine.Core/DomainQueryEngine.Core.csproj src/DomainQueryEngine.Core/
COPY src/DomainQueryEngine.Infrastructure/DomainQueryEngine.Infrastructure.csproj src/DomainQueryEngine.Infrastructure/
COPY src/DomainQueryEngine.PostgreSQL/DomainQueryEngine.PostgreSQL.csproj src/DomainQueryEngine.PostgreSQL/

RUN dotnet restore src/DomainQueryEngine.SearchUI/DomainQueryEngine.SearchUI.csproj

# Copy the rest
COPY . .

RUN dotnet publish src/DomainQueryEngine.SearchUI/DomainQueryEngine.SearchUI.csproj \
  -c Release -o /app/publish \
  /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Run as non-root (good practice)
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

# Render/containers: bind to 0.0.0.0 and use injected PORT (default 8080 for local)
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DomainQueryEngine.SearchUI.dll"]
