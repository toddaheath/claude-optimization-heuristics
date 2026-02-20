# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Full-stack application to visualize and compare optimization heuristics solving the Traveling Salesman Problem (TSP). Users configure algorithms, run simulations server-side, watch live and animated-replay routes in the browser, and save/load everything to PostgreSQL.

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

### Algorithms
6 implementations (SA, ACO, GA, PSO, Slime Mold, Tabu Search) all inherit from `AlgorithmBase` which implements `IOptimizationAlgorithm`. `AlgorithmFactory` creates instances by `AlgorithmType` enum.

`AlgorithmBase.Solve()` accepts an optional `Action<IterationResult>? onIteration` callback. Internally it uses an `ObservableList<IterationResult>` — a private subclass that fires the callback on every `Add` — so individual algorithm implementations do not need to be modified to support streaming.

### Services
- `ProblemDefinitionService`, `AlgorithmConfigurationService`, `OptimizationService` use FluentResults for error handling.
- Controllers are thin — call service, convert Result to ActionResult via `ResultExtensions`.
- `OptimizationService.RunAsync` creates the DB record with `Running` status, registers the run in `RunProgressStore`, then fires off a background `Task.Run` (with its own DI scope via `IServiceScopeFactory`) and **returns immediately**. The background task calls the algorithm with the progress callback and updates the DB when done.
- `RunProgressStore` is a singleton `ConcurrentDictionary`-backed service that accumulates `IterationResult` entries from the background task so the frontend can poll for live updates.

### Database
3 tables (`problem_definitions`, `algorithm_configurations`, `optimization_runs`). Cities, parameters, and iteration history stored as JSONB. EF Core with `ApplicationDbContext` handles JSON serialization via value converters.

### Frontend
- **Zustand store** (`useStore`) tracks: `currentRun`, `iterationHistory`, `currentIteration`, `isPlaying`, `playbackSpeed`, `initialRoute` (random shuffle shown as grey underlay), `isRunning` (true while background optimization is active).
- **`TspCanvas`** renders routes on HTML5 Canvas. Shows an orange route + "Running…" badge during live execution; blue during replay; green on completion.
- **`useAnimation`** hook drives post-run playback via `requestAnimationFrame`.
- **`ConfigurationPanel`** starts a run, then polls `GET /api/v1/optimization-runs/{id}/progress` every 300 ms to update the canvas and convergence chart live. Animation controls are hidden during live execution.
- **`DocumentationPage`** (`/docs`) provides tab-based reference for all 6 algorithms: how-it-works, when-to-use, and per-parameter definitions with rationale for defaults.
- **`HistoryPage`** — Details button fetches the full run and shows a modal with initial/optimized distances, improvement %, execution time, parameters, and error info.

### API
All endpoints under `/api/v1`. Vite dev server proxies `/api` to the backend.

| Method | Path | Description |
|--------|------|-------------|
| POST | `/optimization-runs` | Start a run (returns immediately with `Running` status) |
| GET | `/optimization-runs/{id}/progress` | Live iteration history from `RunProgressStore` |
| GET | `/optimization-runs/{id}` | Full run from DB (use after polling detects completion) |
| GET | `/optimization-runs` | Paginated run list |
| DELETE | `/optimization-runs/{id}` | Delete a run |

### City Layout Generators
6 shapes available in the UI — Random, Circle, Square, Triangle, Pentagon, Hexagon. All use vertex-based perimeter distribution. When a problem is created the frontend also stores a Fisher-Yates shuffle of city indices as `initialRoute`, shown as the grey dashed underlay before/during optimization.
