# syntax=docker/dockerfile:1

# SDC-CRM API container image.
# Build context: the Backend directory, e.g. from the repository root:
#   docker build -f Backend/Dockerfile -t sdc-crm-api Backend
#
# No configuration or secrets are baked into the image - provide them at runtime, e.g.:
#   ConnectionStrings__Crm, Oidc__Authority, Oidc__AllowedCorsOrigins__0, OTEL_EXPORTER_OTLP_ENDPOINT
# (the API fails fast when ConnectionStrings:Crm or Oidc:Authority is missing).
# Database migrations are not applied by the container; run them explicitly (see Backend/README.md).

# --- Build stage: restore and publish the API (tests are not part of the image) -------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore in a separate, cacheable layer: only project files and shared build properties.
COPY Directory.Build.props ./
COPY src/SDC.CRM.Domain/SDC.CRM.Domain.csproj src/SDC.CRM.Domain/
COPY src/SDC.CRM.Application/SDC.CRM.Application.csproj src/SDC.CRM.Application/
COPY src/SDC.CRM.Infrastructure/SDC.CRM.Infrastructure.csproj src/SDC.CRM.Infrastructure/
COPY src/SDC.CRM.Api/SDC.CRM.Api.csproj src/SDC.CRM.Api/
RUN dotnet restore src/SDC.CRM.Api/SDC.CRM.Api.csproj

COPY src/ src/
RUN dotnet publish src/SDC.CRM.Api/SDC.CRM.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

# --- Runtime stage: ASP.NET Core runtime only, non-root user -------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Built-in non-root user of the official .NET images.
USER $APP_UID

ENTRYPOINT ["dotnet", "SDC.CRM.Api.dll"]

