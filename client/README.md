# Frontend — Optimization Heuristics Visualizer

React 18 + TypeScript SPA built with Vite 7. Proxies all `/api` requests to the .NET backend at `localhost:5000`.

## Dev setup

```bash
npm install
npm run dev        # http://localhost:5173
npm run build      # Production build → dist/
npm run lint       # ESLint
```

## Structure

```
src/
├── api/
│   └── client.ts          # Axios instance + typed wrappers for all API endpoints
├── components/
│   ├── AlgorithmSelector  # Dropdown for the 6 algorithm types
│   ├── CanvasControls     # Play/pause/scrub/speed controls for post-run replay
│   ├── ConfigurationPanel # City generators, algorithm config, Run button + live polling
│   ├── ConvergenceChart   # Recharts dual-line chart (best + current distance)
│   ├── ParameterForm      # Dynamic parameter inputs per algorithm
│   └── TspCanvas          # HTML5 Canvas route renderer (live orange / replay blue / complete green)
├── hooks/
│   └── useAnimation.ts    # requestAnimationFrame loop driven by Zustand currentIteration
├── pages/
│   ├── ConfigurationsPage # Saved algorithm configurations CRUD
│   ├── DocumentationPage  # In-app algorithm reference with tab-based navigation
│   ├── HistoryPage        # Paginated run list with Details modal and Replay button
│   └── HomePage           # Main visualization: canvas + controls + convergence chart
├── store/
│   └── useStore.ts        # Zustand store — animation state, live run state, initial route
└── types/
    └── index.ts           # Shared interfaces, AlgorithmType/RunStatus const-objects, defaults
```

## Key state flows

### Starting an optimization
1. `ConfigurationPanel` POSTs to `/optimization-runs` → server returns immediately with `Running` status.
2. A `setInterval` (300 ms) polls `GET /optimization-runs/{id}/progress` and writes results into Zustand (`iterationHistory`, `currentIteration`).
3. `TspCanvas` re-renders on each Zustand update, showing the orange live route.
4. When polling detects `Completed` or `Failed`, it fetches the full run from `/optimization-runs/{id}`, sets it in the store, and stops the interval.

### Post-run replay
`useAnimation` drives a `requestAnimationFrame` loop that increments `currentIteration` at the configured speed. `TspCanvas` reads `iterationHistory[currentIteration]` to render the correct frame.

### Initial route underlay
When a problem is created, a Fisher-Yates shuffle of `[0..n-1]` is stored in `useStore.initialRoute`. `TspCanvas` renders this as the grey dashed background tour instead of the sequential 0→1→2→…→N ordering.
