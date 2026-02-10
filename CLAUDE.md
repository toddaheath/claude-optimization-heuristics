# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Full-stack application to visualize and compare optimization heuristics solving the Traveling Salesman Problem (TSP). Users configure algorithms, run simulations server-side, watch animated replays in the browser, and save/load everything to PostgreSQL.

## Tech Stack

- **Backend:** .NET 8, ASP.NET Core Web API, EF Core + Npgsql (PostgreSQL), FluentResults, FluentValidation
- **Frontend:** React 18 + TypeScript, Vite, Zustand, TanStack React Query, HTML5 Canvas, Recharts, Tailwind CSS
- **Testing:** xUnit, NSubstitute, FluentAssertions, Coverlet
- **Database:** PostgreSQL 16 via Docker Compose

## Solution Structure

```
OptimizationHeuristics.sln
├── src/
│   ├── OptimizationHeuristics.Api/          # Controllers, validators, DTOs, middleware
│   ├── OptimizationHeuristics.Core/         # Domain models, algorithms, service interfaces/impls
│   └── OptimizationHeuristics.Infrastructure/  # EF Core context, repositories
├── tests/
│   ├── OptimizationHeuristics.Core.Tests/
│   ├── OptimizationHeuristics.Api.Tests/
│   ├── OptimizationHeuristics.Infrastructure.Tests/
│   └── OptimizationHeuristics.Integration.Tests/
└── client/                                  # React SPA (Vite)
```

## Build & Test Commands

```bash
# Backend
dotnet build                                          # Build the solution
dotnet test                                           # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"    # Run a single test
dotnet run --project src/OptimizationHeuristics.Api   # Start API on localhost:5000

# Frontend
cd client && npm install                              # Install dependencies
cd client && npm run dev                              # Start dev server on localhost:5173
cd client && npm run build                            # Production build

# Database
docker-compose up -d                                  # Start PostgreSQL
```

## Architecture Notes

- **Algorithms:** 5 implementations (SA, ACO, GA, PSO, Slime Mold) inherit from `AlgorithmBase` implementing `IOptimizationAlgorithm`. `AlgorithmFactory` creates instances by `AlgorithmType` enum.
- **Services:** `ProblemDefinitionService`, `AlgorithmConfigurationService`, `OptimizationService` use FluentResults for error handling. Controllers are thin — call service, convert Result to ActionResult via `ResultExtensions`.
- **Database:** 3 tables (`problem_definitions`, `algorithm_configurations`, `optimization_runs`). Cities, parameters, and iteration history stored as JSONB. EF Core with `ApplicationDbContext` handles JSON serialization via value converters.
- **Frontend:** Zustand for animation state, React Query for server state. `TspCanvas` renders routes on HTML5 Canvas. `useAnimation` hook drives playback via `requestAnimationFrame`.
- **API:** All endpoints under `/api/v1`. Vite dev server proxies `/api` to the backend.
