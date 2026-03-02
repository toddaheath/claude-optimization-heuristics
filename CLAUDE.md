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
dotnet run --project src/OptimizationHeuristics.Api   # Start API on localhost:5211

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
3 tables (`problem_definitions`, `algorithm_configurations`, `optimization_runs`) plus auth tables (`users`, `refresh_tokens`). Cities, parameters, and iteration history stored as JSONB. EF Core with `ApplicationDbContext` handles JSON serialization via value converters. `optimization_runs` has FK constraints to `algorithm_configurations` and `problem_definitions` with `DeleteBehavior.Restrict` — delete operations on configs/problems check for referencing runs first.

### Frontend
- **Zustand store** (`useStore`) tracks: `currentRun`, `iterationHistory`, `currentIteration`, `isPlaying`, `playbackSpeed`, `initialRoute` (random shuffle shown as grey underlay), `isRunning` (true while background optimization is active).
- **`TspCanvas`** renders routes on HTML5 Canvas. Shows an orange route + "Running…" badge during live execution; blue during replay; green on completion.
- **`useAnimation`** hook drives post-run playback via `requestAnimationFrame`.
- **`ConfigurationPanel`** starts a run, then polls `GET /api/v1/optimization-runs/{id}/progress` every 300 ms to update the canvas and convergence chart live. Shows a red "Cancel Run" button during execution. Animation controls are hidden during live execution.
- **`DocumentationPage`** (`/docs`) provides tab-based reference for all 6 algorithms: how-it-works, when-to-use, and per-parameter definitions with rationale for defaults.
- **`HistoryPage`** — Details button fetches the full run and shows a modal with initial/optimized distances, improvement %, execution time, parameters, and error info. Uses server-side pagination with `totalCount`.
- **`ComparisonPage`** — Side-by-side route canvases, metrics table, and convergence overlay for up to 4 selected runs.

### API
All endpoints under `/api/v1`. Vite dev server proxies `/api` to the backend. All list endpoints return paginated responses: `{ items: T[], totalCount: number }`.

| Method | Path | Description |
|--------|------|-------------|
| POST | `/optimization-runs` | Start a run (returns immediately with `Running` status) |
| GET | `/optimization-runs/{id}/progress` | Live iteration history from `RunProgressStore` (falls back to DB when store is cleaned) |
| GET | `/optimization-runs/{id}` | Full run from DB (use after polling detects completion) |
| GET | `/optimization-runs?page=1&pageSize=20` | Paginated run list (returns `OptimizationRunSummaryDto` without iterationHistory/bestRoute) |
| POST | `/optimization-runs/{id}/cancel` | Cancel a running optimization (triggers CancellationToken) |
| DELETE | `/optimization-runs/{id}` | Delete a run (also cancels if running) |
| GET | `/problem-definitions?page=1&pageSize=50` | Paginated problem list |
| POST | `/problem-definitions` | Create a problem |
| DELETE | `/problem-definitions/{id}` | Delete (fails if referenced by runs) |
| GET | `/algorithm-configurations?page=1&pageSize=50` | Paginated config list |
| POST | `/algorithm-configurations` | Create a config |
| PUT | `/algorithm-configurations/{id}` | Update a config |
| DELETE | `/algorithm-configurations/{id}` | Delete (fails if referenced by runs) |

### Authentication
JWT-based auth with access/refresh token flow. Tokens stored in Zustand (persisted to localStorage). Axios interceptor handles 401 → token refresh automatically. Auth endpoints: `/api/v1/auth/register`, `/api/v1/auth/login`, `/api/v1/auth/refresh`, `/api/v1/auth/revoke`.

### Validation
FluentValidation validators on all request DTOs with user-friendly `WithMessage()` text. Key rules: Name required + max 200 chars, Description max 1000 chars, MaxIterations 1–100,000, AlgorithmType must be valid enum, Parameters dict required, Cities count >= 2 with coordinate bounds.

### Error Handling
- Backend uses `FluentResults` → `ResultExtensions` maps to appropriate HTTP status codes with `ApiResponse<T>` envelope: `{ success: bool, data?: T, errors: string[] }`
- Failed optimization runs persist the exception type and message (first 500 chars) to `ErrorMessage` column. Cancelled runs get a dedicated message.
- Frontend Axios interceptor extracts structured error messages from API responses, with descriptive fallbacks per HTTP status code
- React Query configured with 2 retries + exponential backoff
- Query/mutation errors displayed inline on all pages
- Route-level `ErrorBoundary` wraps page content (nav stays visible on crash)

### City Layout Generators
6 shapes available in the UI — Random, Circle, Square, Triangle, Pentagon, Hexagon. All use vertex-based perimeter distribution. When a problem is created the frontend also stores a Fisher-Yates shuffle of city indices as `initialRoute`, shown as the grey dashed underlay before/during optimization.
