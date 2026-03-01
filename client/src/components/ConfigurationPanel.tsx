import { useEffect, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlgorithmType, DEFAULT_PARAMETERS, RunStatus } from '../types';
import type { ProblemDefinition, City } from '../types';
import { problemApi, configApi, runApi } from '../api/client';
import { useStore } from '../store/useStore';
import { AlgorithmSelector } from './AlgorithmSelector';
import { ParameterForm } from './ParameterForm';
import {
  generateRandomCities,
  generateCircleCities,
  generateSquareCities,
  generateTriangleCities,
  generatePentagonCities,
  generateHexagonCities,
} from '../utils/cityGenerators';

/** Fisher-Yates shuffle of indices 0..n-1 */
function randomRoute(n: number): number[] {
  const arr = Array.from({ length: n }, (_, i) => i);
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}

const POLL_INTERVAL_MS = 300;

export function ConfigurationPanel() {
  const queryClient = useQueryClient();
  const {
    setCurrentRun,
    selectedProblemId,
    setSelectedProblemId,
    setInitialRoute,
    isRunning,
    setIsRunning,
    setIterationHistory,
    setCurrentIteration,
  } = useStore();

  const [algorithmType, setAlgorithmType] = useState<AlgorithmType>(AlgorithmType.SimulatedAnnealing);
  const [parameters, setParameters] = useState<Record<string, number>>(
    DEFAULT_PARAMETERS[AlgorithmType.SimulatedAnnealing],
  );
  const [maxIterations, setMaxIterations] = useState(500);
  const [cityCount, setCityCount] = useState(20);
  const [isStartingRun, setIsStartingRun] = useState(false);
  const [runError, setRunError] = useState<string | null>(null);

  const activeRunIdRef = useRef<string | null>(null);

  const { data: problems } = useQuery({
    queryKey: ['problems'],
    queryFn: problemApi.getAll,
  });

  // Poll for progress using React Query's refetchInterval
  const { data: progress } = useQuery({
    queryKey: ['run-progress', activeRunIdRef.current],
    queryFn: () => runApi.getProgress(activeRunIdRef.current!),
    enabled: !!activeRunIdRef.current && isRunning,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (status && status !== 'Running') return false;
      return POLL_INTERVAL_MS;
    },
  });

  // React to progress updates
  useEffect(() => {
    if (!progress) return;

    const history = progress.iterationHistory ?? [];
    setIterationHistory(history);
    if (history.length > 0) {
      setCurrentIteration(history.length - 1);
    }

    if (progress.status !== RunStatus.Running) {
      const runId = activeRunIdRef.current;
      activeRunIdRef.current = null;
      setIsRunning(false);

      if (runId) {
        // Fetch the final saved run from the DB
        void runApi.getById(runId).then((finalRun) => {
          setCurrentRun(finalRun);
          void queryClient.invalidateQueries({ queryKey: ['runs'] });
        }).catch(() => {
          void queryClient.invalidateQueries({ queryKey: ['runs'] });
        });
      }
    }
  }, [progress, setIterationHistory, setCurrentIteration, setIsRunning, setCurrentRun, queryClient]);

  const createProblem = useMutation({
    mutationFn: problemApi.create,
    onSuccess: (data) => {
      void queryClient.invalidateQueries({ queryKey: ['problems'] });
      setSelectedProblemId(data.id);
      setInitialRoute(randomRoute(data.cityCount));
    },
  });

  const handleAlgorithmChange = (type: AlgorithmType) => {
    setAlgorithmType(type);
    setParameters(DEFAULT_PARAMETERS[type]);
  };

  const startRun = async () => {
    if (!selectedProblemId) return;
    setIsStartingRun(true);
    setRunError(null);

    try {
      const config = await configApi.create({
        name: `${algorithmType} - ${new Date().toLocaleTimeString()}`,
        algorithmType,
        parameters,
        maxIterations,
      });

      // POST now returns immediately with Running status
      const run = await runApi.run({
        algorithmConfigurationId: config.id,
        problemDefinitionId: selectedProblemId,
      });

      setCurrentRun(run);
      setIsRunning(true);
      activeRunIdRef.current = run.id;
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to start optimization run';
      setRunError(message);
      setIsRunning(false);
    } finally {
      setIsStartingRun(false);
    }
  };

  // ── City generators ──────────────────────────────────────────────────────

  const makeProblem = (name: string, description: string, cities: City[]) => {
    createProblem.mutate({ name, description, cities });
  };

  const handleRandomCities = () => {
    makeProblem(`Random ${cityCount} cities`, `Randomly generated ${cityCount} cities`, generateRandomCities(cityCount));
  };

  const handleCircleCities = () => {
    makeProblem(`Circle ${cityCount} cities`, `${cityCount} cities arranged on a circle`, generateCircleCities(cityCount));
  };

  const handleSquareCities = () => {
    makeProblem(`Square ${cityCount} cities`, `${cityCount} cities arranged on a square`, generateSquareCities(cityCount));
  };

  const handleTriangleCities = () => {
    makeProblem(`Triangle ${cityCount} cities`, `${cityCount} cities arranged on a triangle`, generateTriangleCities(cityCount));
  };

  const handlePentagonCities = () => {
    makeProblem(`Pentagon ${cityCount} cities`, `${cityCount} cities arranged on a pentagon`, generatePentagonCities(cityCount));
  };

  const handleHexagonCities = () => {
    makeProblem(`Hexagon ${cityCount} cities`, `${cityCount} cities arranged on a hexagon`, generateHexagonCities(cityCount));
  };

  const isGenerating = createProblem.isPending;
  const isRunActive = isStartingRun || isRunning;

  return (
    <div className="space-y-4 p-4 bg-gray-50 rounded-lg border">
      <h2 className="font-bold text-lg">Configuration</h2>

      {/* Problem selector */}
      <div>
        <label htmlFor="problem-select" className="block text-sm font-medium text-gray-700 mb-1">Problem</label>
        <select
          id="problem-select"
          value={selectedProblemId}
          onChange={(e) => setSelectedProblemId(e.target.value)}
          className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
        >
          <option value="">Select a problem…</option>
          {problems?.map((p: ProblemDefinition) => (
            <option key={p.id} value={p.id}>
              {p.name} ({p.cityCount} cities)
            </option>
          ))}
        </select>

        {/* City count + layout generators */}
        <div className="mt-2 space-y-1.5">
          <div className="flex items-center gap-2">
            <label htmlFor="city-count" className="text-xs text-gray-600 shrink-0">Cities:</label>
            <input
              id="city-count"
              type="number"
              min={3}
              max={200}
              value={cityCount}
              onChange={(e) => setCityCount(Number(e.target.value))}
              className="w-20 px-2 py-1 border rounded text-sm bg-white"
            />
          </div>
          <div className="grid grid-cols-3 gap-1.5">
            <button onClick={handleRandomCities}   disabled={isGenerating} className="px-2 py-1.5 bg-green-600   text-white rounded text-xs font-medium hover:bg-green-700   disabled:opacity-50">{isGenerating ? '…' : 'Random'}</button>
            <button onClick={handleCircleCities}   disabled={isGenerating} className="px-2 py-1.5 bg-purple-600  text-white rounded text-xs font-medium hover:bg-purple-700  disabled:opacity-50">{isGenerating ? '…' : 'Circle'}</button>
            <button onClick={handleSquareCities}   disabled={isGenerating} className="px-2 py-1.5 bg-teal-600    text-white rounded text-xs font-medium hover:bg-teal-700    disabled:opacity-50">{isGenerating ? '…' : 'Square'}</button>
            <button onClick={handleTriangleCities} disabled={isGenerating} className="px-2 py-1.5 bg-orange-500  text-white rounded text-xs font-medium hover:bg-orange-600  disabled:opacity-50">{isGenerating ? '…' : 'Triangle'}</button>
            <button onClick={handlePentagonCities} disabled={isGenerating} className="px-2 py-1.5 bg-pink-600    text-white rounded text-xs font-medium hover:bg-pink-700    disabled:opacity-50">{isGenerating ? '…' : 'Pentagon'}</button>
            <button onClick={handleHexagonCities}  disabled={isGenerating} className="px-2 py-1.5 bg-indigo-600  text-white rounded text-xs font-medium hover:bg-indigo-700  disabled:opacity-50">{isGenerating ? '…' : 'Hexagon'}</button>
          </div>
        </div>
      </div>

      <AlgorithmSelector value={algorithmType} onChange={handleAlgorithmChange} />

      <div>
        <label htmlFor="max-iterations" className="block text-sm font-medium text-gray-700 mb-1">Max Iterations</label>
        <input
          id="max-iterations"
          type="number"
          min={1}
          max={100000}
          value={maxIterations}
          onChange={(e) => setMaxIterations(Number(e.target.value))}
          className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
        />
      </div>

      <ParameterForm parameters={parameters} onChange={setParameters} />

      {runError && (
        <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {runError}
        </div>
      )}

      <button
        onClick={() => void startRun()}
        disabled={!selectedProblemId || isRunActive}
        className="w-full py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50"
      >
        {isRunActive ? 'Starting…' : 'Run Optimization'}
      </button>
    </div>
  );
}
