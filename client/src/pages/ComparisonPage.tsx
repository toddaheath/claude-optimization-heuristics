import { useState, useMemo } from 'react';
import { useQuery, useQueries } from '@tanstack/react-query';
import { runApi, configApi, problemApi } from '../api/client';
import { RunStatus, ALGORITHM_LABELS } from '../types';
import type { OptimizationRun, AlgorithmConfiguration } from '../types';
import { TspCanvas } from '../components/TspCanvas';
import { MetricsTable } from '../components/MetricsTable';
import { ConvergenceOverlay } from '../components/ConvergenceOverlay';

const MAX_SELECTIONS = 4;

export function ComparisonPage() {
  const [selectedProblemId, setSelectedProblemId] = useState<string>('');
  const [selectedRunIds, setSelectedRunIds] = useState<Set<string>>(new Set());

  const { data: allRuns } = useQuery({
    queryKey: ['runs', 1, 100],
    queryFn: () => runApi.getAll(1, 100),
  });

  const { data: configs } = useQuery({
    queryKey: ['configs'],
    queryFn: () => configApi.getAll(),
  });

  const { data: problems } = useQuery({
    queryKey: ['problems'],
    queryFn: () => problemApi.getAll(),
  });

  const configMap = useMemo(
    () => new Map(configs?.map((c: AlgorithmConfiguration) => [c.id, c]) ?? []),
    [configs],
  );

  // Completed runs grouped by problem
  const completedRuns = useMemo(
    () => (allRuns ?? []).filter((r: OptimizationRun) => r.status === RunStatus.Completed),
    [allRuns],
  );

  // Problems that have at least one completed run
  const problemIds = useMemo(
    () => [...new Set(completedRuns.map((r: OptimizationRun) => r.problemDefinitionId))],
    [completedRuns],
  );

  const filteredRuns = useMemo(
    () =>
      selectedProblemId
        ? completedRuns.filter((r: OptimizationRun) => r.problemDefinitionId === selectedProblemId)
        : [],
    [completedRuns, selectedProblemId],
  );

  // Fetch full run data for selected runs
  const selectedIds = useMemo(() => [...selectedRunIds], [selectedRunIds]);
  const runQueries = useQueries({
    queries: selectedIds.map((id) => ({
      queryKey: ['run', id],
      queryFn: () => runApi.getById(id),
      staleTime: 60_000,
    })),
  });

  const loadedRuns = runQueries
    .filter((q) => q.isSuccess && q.data)
    .map((q) => q.data!);

  // Fetch the shared problem definition
  const { data: problem } = useQuery({
    queryKey: ['problem', selectedProblemId],
    queryFn: () => problemApi.getById(selectedProblemId),
    enabled: !!selectedProblemId,
  });

  function toggleRun(id: string) {
    setSelectedRunIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else if (next.size < MAX_SELECTIONS) {
        next.add(id);
      }
      return next;
    });
  }

  function handleProblemChange(id: string) {
    setSelectedProblemId(id);
    setSelectedRunIds(new Set());
  }

  const isLoading = runQueries.some((q) => q.isLoading);

  return (
    <div className="flex gap-6 p-6 max-w-screen-xl mx-auto">
      {/* Left panel — Run Selection */}
      <div className="w-80 shrink-0 space-y-4">
        <h1 className="text-2xl font-bold">Compare Runs</h1>

        {/* Problem filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Problem</label>
          <select
            value={selectedProblemId}
            onChange={(e) => handleProblemChange(e.target.value)}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm"
          >
            <option value="">Select a problem...</option>
            {problemIds.map((pid) => {
              const prob = problems?.find((p) => p.id === pid);
              return (
                <option key={pid} value={pid}>
                  {prob?.name ?? pid}
                </option>
              );
            })}
          </select>
        </div>

        {/* Run list */}
        {selectedProblemId && (
          <div>
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm font-medium text-gray-700">
                Runs ({selectedRunIds.size}/{MAX_SELECTIONS})
              </span>
              {selectedRunIds.size > 0 && (
                <button
                  onClick={() => setSelectedRunIds(new Set())}
                  className="text-xs text-blue-600 hover:text-blue-800"
                >
                  Clear
                </button>
              )}
            </div>
            <div className="border border-gray-300 rounded-lg max-h-[60vh] overflow-y-auto divide-y">
              {filteredRuns.length === 0 && (
                <p className="p-3 text-sm text-gray-500">No completed runs for this problem.</p>
              )}
              {filteredRuns.map((run: OptimizationRun) => {
                const cfg = configMap.get(run.algorithmConfigurationId);
                const checked = selectedRunIds.has(run.id);
                const disabled = !checked && selectedRunIds.size >= MAX_SELECTIONS;
                return (
                  <label
                    key={run.id}
                    className={`flex items-start gap-2 p-3 cursor-pointer hover:bg-gray-50 ${
                      disabled ? 'opacity-50 cursor-not-allowed' : ''
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      disabled={disabled}
                      onChange={() => toggleRun(run.id)}
                      className="mt-0.5"
                    />
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">
                        {cfg ? ALGORITHM_LABELS[cfg.algorithmType] : '–'}
                      </p>
                      <p className="text-xs text-gray-500">
                        {run.bestDistance?.toFixed(2) ?? '–'} &middot;{' '}
                        {new Date(run.createdAt).toLocaleDateString()}
                      </p>
                    </div>
                  </label>
                );
              })}
            </div>
          </div>
        )}
      </div>

      {/* Right panel — Comparison Display */}
      <div className="flex-1 min-w-0 space-y-6">
        {selectedRunIds.size < 2 ? (
          <div className="flex items-center justify-center h-64 bg-white rounded-lg border border-gray-300 text-gray-500 text-sm">
            Select at least 2 completed runs to compare.
          </div>
        ) : isLoading ? (
          <div className="flex items-center justify-center h-64 bg-white rounded-lg border border-gray-300 text-gray-500 text-sm">
            Loading run data...
          </div>
        ) : (
          <>
            {/* Canvas grid */}
            <div>
              <h3 className="text-sm font-semibold mb-2 text-gray-700">Final Routes</h3>
              <div
                className={`grid gap-4 ${loadedRuns.length <= 2 ? 'grid-cols-1 md:grid-cols-2' : 'grid-cols-2 xl:grid-cols-4'}`}
              >
                {loadedRuns.map((run, i) => {
                  const cfg = configMap.get(run.algorithmConfigurationId);
                  const lastIter =
                    run.iterationHistory && run.iterationHistory.length > 0
                      ? run.iterationHistory[run.iterationHistory.length - 1]
                      : run.bestRoute
                        ? { iteration: 0, bestDistance: run.bestDistance!, bestRoute: run.bestRoute, currentDistance: run.bestDistance! }
                        : null;
                  return (
                    <div key={run.id} className="space-y-1">
                      <p className="text-xs font-medium text-gray-700 truncate">
                        {cfg ? ALGORITHM_LABELS[cfg.algorithmType] : `Run ${i + 1}`}
                        {' — '}
                        {run.bestDistance?.toFixed(2) ?? '–'}
                      </p>
                      <TspCanvas
                        cities={problem?.cities ?? []}
                        currentFrame={lastIter}
                        isComplete
                        width={400}
                        height={300}
                      />
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Metrics table */}
            <MetricsTable runs={loadedRuns} configs={configs ?? []} />

            {/* Convergence overlay */}
            <ConvergenceOverlay runs={loadedRuns} configs={configs ?? []} />
          </>
        )}
      </div>
    </div>
  );
}
