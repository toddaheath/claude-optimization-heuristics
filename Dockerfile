# Stage 1: Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

COPY *.sln ./
COPY src/OptimizationHeuristics.Api/*.csproj src/OptimizationHeuristics.Api/
COPY src/OptimizationHeuristics.Core/*.csproj src/OptimizationHeuristics.Core/
COPY src/OptimizationHeuristics.Infrastructure/*.csproj src/OptimizationHeuristics.Infrastructure/
RUN dotnet restore src/OptimizationHeuristics.Api

COPY src/ src/
RUN dotnet publish src/OptimizationHeuristics.Api \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Stage 2: Build frontend
FROM node:22-slim AS frontend-build
WORKDIR /client

COPY client/package.json client/package-lock.json ./
RUN npm ci

COPY client/ ./
RUN npm run build

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /client/dist ./wwwroot/

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "OptimizationHeuristics.Api.dll"]
