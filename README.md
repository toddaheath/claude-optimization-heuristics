# Optimization Heuristics Visualizer

A full-stack application for visualizing and comparing metaheuristic algorithms solving the **Traveling Salesman Problem (TSP)**. Configure an algorithm, watch the route improve in real time as it runs server-side, then replay the full optimization history frame by frame.

## Features

- **Live optimization display** — the canvas updates every 300 ms while the server is computing, showing the route improving and the convergence chart streaming data in real time.
- **6 optimization algorithms** — Simulated Annealing, Ant Colony Optimization, Genetic Algorithm, Particle Swarm Optimization, Slime Mold Optimization, and Tabu Search.
- **6 city layout generators** — Random, Circle, Square, Triangle, Pentagon, and Hexagon. The initial tour displayed is a random permutation, not a sequential one.
- **Animated replay** — scrub, step, or play back any completed run frame by frame with configurable speed (0.5× – 10×).
- **Convergence chart** — dual-line Recharts visualization showing best-distance and current-iteration distance over time.
- **Run history** — paginated table with a Details modal showing initial/optimized distances, improvement %, execution time, parameters, and error info.
- **Documentation page** — in-app reference for every algorithm: how it works, when to use it, and per-parameter rationale for defaults.
- **PostgreSQL persistence** — problem definitions, algorithm configurations, and full iteration histories saved as JSONB.

## Tech Stack

| Layer | Technologies |
|-------|-------------|
| Backend | .NET 8, ASP.NET Core Web API, EF Core + Npgsql, FluentResults, FluentValidation |
| Frontend | React 18 + TypeScript, Vite 7, Zustand, TanStack React Query, Recharts, Tailwind CSS |
| Database | PostgreSQL 16 (Docker Compose) |
| Testing | xUnit, NSubstitute, FluentAssertions, Coverlet |

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)

### 1. Start the database

```bash
docker-compose up -d
```

### 2. Start the API

```bash
dotnet run --project src/OptimizationHeuristics.Api
# Listens on http://localhost:5000
```

### 3. Start the frontend dev server

```bash
cd client
npm install
npm run dev
# Opens http://localhost:5173
```

The Vite dev server proxies all `/api` requests to the backend, so no CORS configuration is needed locally.

## Usage

1. **Generate cities** — choose a count and a layout (Random, Circle, Square, Triangle, Pentagon, or Hexagon). The grey dashed initial tour is randomly shuffled.
2. **Configure the algorithm** — pick an algorithm, adjust parameters, and set max iterations.
3. **Run optimization** — click "Run Optimization". The canvas updates live with an orange route while the server computes.
4. **Replay** — use the playback controls to scrub through the iteration history after the run completes.
5. **History** — visit the History page to view past runs, click Details for full metadata, or Replay to re-animate any run.
6. **Docs** — visit the Docs page for in-depth explanations of every algorithm and parameter.

## Project Structure

```
OptimizationHeuristics.sln
├── src/
│   ├── OptimizationHeuristics.Api/
│   │   ├── Controllers/        # Thin controllers — validate, call service, map result
│   │   ├── DTOs/               # Request/response records
│   │   ├── Validators/         # FluentValidation validators
│   │   └── Middleware/         # Global exception handling
│   ├── OptimizationHeuristics.Core/
│   │   ├── Algorithms/         # AlgorithmBase + 6 implementations + factory
│   │   ├── Entities/           # EF Core entity classes
│   │   ├── Models/             # City, Route, IterationResult, OptimizationResult
│   │   ├── Enums/              # AlgorithmType, RunStatus
│   │   └── Services/           # Service interfaces + implementations (incl. RunProgressStore)
│   └── OptimizationHeuristics.Infrastructure/
│       ├── Data/               # ApplicationDbContext (JSONB value converters)
│       └── Repositories/       # Generic Repository<T>, UnitOfWork
├── tests/
│   ├── OptimizationHeuristics.Core.Tests/
│   ├── OptimizationHeuristics.Api.Tests/
│   ├── OptimizationHeuristics.Infrastructure.Tests/
│   └── OptimizationHeuristics.Integration.Tests/
└── client/                     # React SPA
    └── src/
        ├── api/                # Axios client + typed API wrappers
        ├── components/         # TspCanvas, ConfigurationPanel, ParameterForm, …
        ├── hooks/              # useAnimation (requestAnimationFrame loop)
        ├── pages/              # HomePage, HistoryPage, ConfigurationsPage, DocumentationPage
        ├── store/              # Zustand store (animation + live run state)
        └── types/              # Shared TypeScript interfaces and constants
```

## Algorithms

| Algorithm | Key idea | Best for |
|-----------|----------|----------|
| **Simulated Annealing** | Accepts bad moves with decreasing probability (temperature schedule) | General purpose; reliable on any size |
| **Ant Colony Optimization** | Pheromone trails reinforce good edges over many ant iterations | Graph problems with meaningful edge costs |
| **Genetic Algorithm** | Population of tours evolved via crossover, mutation, and elitism | Large search spaces; diverse solution pool |
| **Particle Swarm Optimization** | Particles balance personal-best and swarm-best pull | Fast early convergence; few parameters |
| **Slime Mold Optimization** | Vein weights updated by sigmoid fitness; guided + random phases | Quick convergence on structured problems |
| **Tabu Search** | Deterministic local search with forbidden-move memory to avoid cycling | Reproducible results; combinatorial fine-tuning |

See the in-app **Docs** page (`/docs`) for full parameter documentation and default rationale.

## API Reference

All endpoints are under `/api/v1`.

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/problem-definitions` | List all problems |
| `POST` | `/problem-definitions` | Create a problem (cities as JSON array) |
| `DELETE` | `/problem-definitions/{id}` | Delete a problem |
| `GET` | `/algorithm-configurations` | List saved configurations |
| `POST` | `/algorithm-configurations` | Save a configuration |
| `PUT` | `/algorithm-configurations/{id}` | Update a configuration |
| `DELETE` | `/algorithm-configurations/{id}` | Delete a configuration |
| `POST` | `/optimization-runs` | Start a run — returns immediately with `Running` status |
| `GET` | `/optimization-runs/{id}/progress` | Live iteration data (poll while running) |
| `GET` | `/optimization-runs/{id}` | Full run record from DB |
| `GET` | `/optimization-runs` | Paginated run list |
| `DELETE` | `/optimization-runs/{id}` | Delete a run |

## Running Tests

```bash
dotnet test                                        # All tests
dotnet test --filter "FullyQualifiedName!~Integration"  # Unit tests only (no DB needed)
```
