# ── Stage 1: Build ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies first (efficient layering)
COPY ["GLMS.csproj", "GLMS/"]
RUN dotnet restore "GLMS/GLMS.csproj"

# Copy everything else and build
COPY . GLMS/
WORKDIR /src/GLMS
RUN dotnet build "GLMS.csproj" -c Release -o /app/build

# ── Stage 2: Publish ──────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "GLMS.csproj" -c Release -o /app/publish \
    --no-restore

# ── Stage 3: Runtime ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create uploads folder for PDF files
RUN mkdir -p /app/wwwroot/uploads/contracts

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=publish /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "GLMS.dll"]