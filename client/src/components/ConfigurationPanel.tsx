import { useEffect, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlgorithmType, DEFAULT_PARAMETERS, RunStatus } from '../types';
import type { ProblemDefinition, City } from '../types';
import { problemApi, configApi, runApi } from '../api/client';
import { useStore } from '../store/useStore';
import { AlgorithmSelector } from './AlgorithmSelector';
import { ParameterForm } from './ParameterForm';

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

  const pollTimerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const { data: problems } = useQuery({
    queryKey: ['problems'],
    queryFn: problemApi.getAll,
  });

  const createProblem = useMutation({
    mutationFn: problemApi.create,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['problems'] });
      setSelectedProblemId(data.id);
      setInitialRoute(randomRoute(data.cityCount));
    },
  });

  const handleAlgorithmChange = (type: AlgorithmType) => {
    setAlgorithmType(type);
    setParameters(DEFAULT_PARAMETERS[type]);
  };

  const stopPolling = () => {
    if (pollTimerRef.current !== null) {
      clearInterval(pollTimerRef.current);
      pollTimerRef.current = null;
    }
  };

  useEffect(() => () => stopPolling(), []);

  const startRun = async () => {
    if (!selectedProblemId) return;
    setIsStartingRun(true);
    stopPolling();

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

      // Start polling for progress
      const runId = run.id;
      pollTimerRef.current = setInterval(async () => {
        try {
          const progress = await runApi.getProgress(runId);
          const history = progress.iterationHistory ?? [];
          setIterationHistory(history);
          if (history.length > 0) {
            setCurrentIteration(history.length - 1);
          }

          if (progress.status !== RunStatus.Running) {
            stopPolling();
            setIsRunning(false);
            // Fetch the final saved run from the DB
            const finalRun = await runApi.getById(runId);
            setCurrentRun(finalRun);
            queryClient.invalidateQueries({ queryKey: ['runs'] });
          }
        } catch {
          stopPolling();
          setIsRunning(false);
          queryClient.invalidateQueries({ queryKey: ['runs'] });
        }
      }, POLL_INTERVAL_MS);
    } catch (err) {
      console.error('Failed to start run:', err);
      setIsRunning(false);
    } finally {
      setIsStartingRun(false);
    }
  };

  // ── City generators ──────────────────────────────────────────────────────

  const makeProblem = (name: string, description: string, cities: City[]) => {
    createProblem.mutate({ name, description, cities });
  };

  const generateRandomCities = () => {
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => ({
      id: i,
      x: Math.round(Math.random() * 460 + 20),
      y: Math.round(Math.random() * 360 + 20),
    }));
    makeProblem(`Random ${cityCount} cities`, `Randomly generated ${cityCount} cities`, cities);
  };

  const generateCircleCities = () => {
    const cx = 250, cy = 200, radius = 180;
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => ({
      id: i,
      x: Math.round(cx + radius * Math.cos((2 * Math.PI * i) / cityCount)),
      y: Math.round(cy + radius * Math.sin((2 * Math.PI * i) / cityCount)),
    }));
    makeProblem(`Circle ${cityCount} cities`, `${cityCount} cities arranged on a circle`, cities);
  };

  const generateSquareCities = () => {
    const margin = 40;
    const w = 420, h = 320;
    const perimeter = 2 * (w + h);
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => {
      const d = (i / cityCount) * perimeter;
      let x: number, y: number;
      if (d < w) {
        x = margin + d;               y = margin;
      } else if (d < w + h) {
        x = margin + w;               y = margin + (d - w);
      } else if (d < 2 * w + h) {
        x = margin + w - (d - w - h); y = margin + h;
      } else {
        x = margin;                   y = margin + h - (d - 2 * w - h);
      }
      return { id: i, x: Math.round(x), y: Math.round(y) };
    });
    makeProblem(`Square ${cityCount} cities`, `${cityCount} cities arranged on a square`, cities);
  };

  const generateTriangleCities = () => {
    const cx = 250, cy = 200, side = 340;
    const th = (Math.sqrt(3) / 2) * side;
    const vertices = [
      { x: cx,             y: cy - (th * 2) / 3 },
      { x: cx - side / 2,  y: cy + th / 3 },
      { x: cx + side / 2,  y: cy + th / 3 },
    ];
    const perimeter = side * 3;
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => {
      const d = (i / cityCount) * perimeter;
      const sideIdx = Math.min(Math.floor(d / side), 2);
      const t = (d - sideIdx * side) / side;
      const from = vertices[sideIdx];
      const to = vertices[(sideIdx + 1) % 3];
      return { id: i, x: Math.round(from.x + t * (to.x - from.x)), y: Math.round(from.y + t * (to.y - from.y)) };
    });
    makeProblem(`Triangle ${cityCount} cities`, `${cityCount} cities arranged on a triangle`, cities);
  };

  const generatePentagonCities = () => {
    const cx = 250, cy = 200, radius = 175;
    const sides = 5;
    const vertices = Array.from({ length: sides }, (_, k) => ({
      x: cx + radius * Math.cos((2 * Math.PI * k) / sides - Math.PI / 2),
      y: cy + radius * Math.sin((2 * Math.PI * k) / sides - Math.PI / 2),
    }));
    const perimeter = sides * 2 * radius * Math.sin(Math.PI / sides);
    const sideLen = perimeter / sides;
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => {
      const d = (i / cityCount) * perimeter;
      const sideIdx = Math.min(Math.floor(d / sideLen), sides - 1);
      const t = (d - sideIdx * sideLen) / sideLen;
      const from = vertices[sideIdx];
      const to = vertices[(sideIdx + 1) % sides];
      return { id: i, x: Math.round(from.x + t * (to.x - from.x)), y: Math.round(from.y + t * (to.y - from.y)) };
    });
    makeProblem(`Pentagon ${cityCount} cities`, `${cityCount} cities arranged on a pentagon`, cities);
  };

  const generateHexagonCities = () => {
    const cx = 250, cy = 200, radius = 170;
    const sides = 6;
    const vertices = Array.from({ length: sides }, (_, k) => ({
      x: cx + radius * Math.cos((2 * Math.PI * k) / sides),
      y: cy + radius * Math.sin((2 * Math.PI * k) / sides),
    }));
    const sideLen = radius; // for regular hexagon, side = radius
    const perimeter = sides * sideLen;
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => {
      const d = (i / cityCount) * perimeter;
      const sideIdx = Math.min(Math.floor(d / sideLen), sides - 1);
      const t = (d - sideIdx * sideLen) / sideLen;
      const from = vertices[sideIdx];
      const to = vertices[(sideIdx + 1) % sides];
      return { id: i, x: Math.round(from.x + t * (to.x - from.x)), y: Math.round(from.y + t * (to.y - from.y)) };
    });
    makeProblem(`Hexagon ${cityCount} cities`, `${cityCount} cities arranged on a hexagon`, cities);
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
            <button onClick={generateRandomCities}   disabled={isGenerating} className="px-2 py-1.5 bg-green-600   text-white rounded text-xs font-medium hover:bg-green-700   disabled:opacity-50">{isGenerating ? '…' : 'Random'}</button>
            <button onClick={generateCircleCities}   disabled={isGenerating} className="px-2 py-1.5 bg-purple-600  text-white rounded text-xs font-medium hover:bg-purple-700  disabled:opacity-50">{isGenerating ? '…' : 'Circle'}</button>
            <button onClick={generateSquareCities}   disabled={isGenerating} className="px-2 py-1.5 bg-teal-600    text-white rounded text-xs font-medium hover:bg-teal-700    disabled:opacity-50">{isGenerating ? '…' : 'Square'}</button>
            <button onClick={generateTriangleCities} disabled={isGenerating} className="px-2 py-1.5 bg-orange-500  text-white rounded text-xs font-medium hover:bg-orange-600  disabled:opacity-50">{isGenerating ? '…' : 'Triangle'}</button>
            <button onClick={generatePentagonCities} disabled={isGenerating} className="px-2 py-1.5 bg-pink-600    text-white rounded text-xs font-medium hover:bg-pink-700    disabled:opacity-50">{isGenerating ? '…' : 'Pentagon'}</button>
            <button onClick={generateHexagonCities}  disabled={isGenerating} className="px-2 py-1.5 bg-indigo-600  text-white rounded text-xs font-medium hover:bg-indigo-700  disabled:opacity-50">{isGenerating ? '…' : 'Hexagon'}</button>
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

      <button
        onClick={startRun}
        disabled={!selectedProblemId || isRunActive}
        className="w-full py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50"
      >
        {isRunActive ? 'Starting…' : 'Run Optimization'}
      </button>
    </div>
  );
}
