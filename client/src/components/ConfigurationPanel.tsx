import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlgorithmType, DEFAULT_PARAMETERS } from '../types';
import type { ProblemDefinition, City } from '../types';
import { problemApi, configApi, runApi } from '../api/client';
import { useStore } from '../store/useStore';
import { AlgorithmSelector } from './AlgorithmSelector';
import { ParameterForm } from './ParameterForm';

export function ConfigurationPanel() {
  const queryClient = useQueryClient();
  const { setCurrentRun, selectedProblemId, setSelectedProblemId } = useStore();

  const [algorithmType, setAlgorithmType] = useState<AlgorithmType>(AlgorithmType.SimulatedAnnealing);
  const [parameters, setParameters] = useState<Record<string, number>>(
    DEFAULT_PARAMETERS[AlgorithmType.SimulatedAnnealing],
  );
  const [maxIterations, setMaxIterations] = useState(500);
  const [cityCount, setCityCount] = useState(20);

  const { data: problems } = useQuery({
    queryKey: ['problems'],
    queryFn: problemApi.getAll,
  });

  const createProblem = useMutation({
    mutationFn: problemApi.create,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['problems'] });
      setSelectedProblemId(data.id);
    },
  });

  const runOptimization = useMutation({
    mutationFn: async () => {
      const config = await configApi.create({
        name: `${algorithmType} - ${new Date().toLocaleTimeString()}`,
        algorithmType,
        parameters,
        maxIterations,
      });
      return runApi.run({
        algorithmConfigurationId: config.id,
        problemDefinitionId: selectedProblemId,
      });
    },
    onSuccess: (run) => {
      setCurrentRun(run);
      queryClient.invalidateQueries({ queryKey: ['runs'] });
    },
  });

  const handleAlgorithmChange = (type: AlgorithmType) => {
    setAlgorithmType(type);
    setParameters(DEFAULT_PARAMETERS[type]);
  };

  // ── City generators ──────────────────────────────────────────────────────

  const generateRandomCities = () => {
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => ({
      id: i,
      x: Math.round(Math.random() * 460 + 20),
      y: Math.round(Math.random() * 360 + 20),
    }));
    createProblem.mutate({
      name: `Random ${cityCount} cities`,
      description: `Randomly generated ${cityCount} cities`,
      cities,
    });
  };

  const generateCircleCities = () => {
    const cx = 250, cy = 200, radius = 180;
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => ({
      id: i,
      x: Math.round(cx + radius * Math.cos((2 * Math.PI * i) / cityCount)),
      y: Math.round(cy + radius * Math.sin((2 * Math.PI * i) / cityCount)),
    }));
    createProblem.mutate({
      name: `Circle ${cityCount} cities`,
      description: `${cityCount} cities evenly arranged on a circle`,
      cities,
    });
  };

  const generateSquareCities = () => {
    // Distribute cities evenly along the 4 sides of a square perimeter
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
    createProblem.mutate({
      name: `Square ${cityCount} cities`,
      description: `${cityCount} cities arranged on a square`,
      cities,
    });
  };

  const generateTriangleCities = () => {
    // Equilateral triangle: top center, bottom-left, bottom-right
    const cx = 250, cy = 200;
    const side = 340;
    const th = (Math.sqrt(3) / 2) * side;
    const vertices = [
      { x: cx,             y: cy - (th * 2) / 3 }, // top
      { x: cx - side / 2,  y: cy + th / 3 },        // bottom-left
      { x: cx + side / 2,  y: cy + th / 3 },        // bottom-right
    ];
    const perimeter = side * 3;
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => {
      const d = (i / cityCount) * perimeter;
      const sideIdx = Math.min(Math.floor(d / side), 2);
      const t = (d - sideIdx * side) / side;
      const from = vertices[sideIdx];
      const to = vertices[(sideIdx + 1) % 3];
      return {
        id: i,
        x: Math.round(from.x + t * (to.x - from.x)),
        y: Math.round(from.y + t * (to.y - from.y)),
      };
    });
    createProblem.mutate({
      name: `Triangle ${cityCount} cities`,
      description: `${cityCount} cities arranged on a triangle`,
      cities,
    });
  };

  const isGenerating = createProblem.isPending;

  return (
    <div className="space-y-4 p-4 bg-gray-50 rounded-lg border">
      <h2 className="font-bold text-lg">Configuration</h2>

      {/* Problem selector */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Problem</label>
        <select
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
            <label className="text-xs text-gray-600 shrink-0">Cities:</label>
            <input
              type="number"
              min={3}
              max={200}
              value={cityCount}
              onChange={(e) => setCityCount(Number(e.target.value))}
              className="w-20 px-2 py-1 border rounded text-sm bg-white"
            />
          </div>
          <div className="grid grid-cols-2 gap-1.5">
            <button
              onClick={generateRandomCities}
              disabled={isGenerating}
              className="px-2 py-1.5 bg-green-600 text-white rounded text-xs font-medium hover:bg-green-700 disabled:opacity-50"
            >
              {isGenerating ? '…' : 'Random'}
            </button>
            <button
              onClick={generateCircleCities}
              disabled={isGenerating}
              className="px-2 py-1.5 bg-purple-600 text-white rounded text-xs font-medium hover:bg-purple-700 disabled:opacity-50"
            >
              {isGenerating ? '…' : 'Circle'}
            </button>
            <button
              onClick={generateSquareCities}
              disabled={isGenerating}
              className="px-2 py-1.5 bg-teal-600 text-white rounded text-xs font-medium hover:bg-teal-700 disabled:opacity-50"
            >
              {isGenerating ? '…' : 'Square'}
            </button>
            <button
              onClick={generateTriangleCities}
              disabled={isGenerating}
              className="px-2 py-1.5 bg-orange-500 text-white rounded text-xs font-medium hover:bg-orange-600 disabled:opacity-50"
            >
              {isGenerating ? '…' : 'Triangle'}
            </button>
          </div>
        </div>
      </div>

      <AlgorithmSelector value={algorithmType} onChange={handleAlgorithmChange} />

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Max Iterations</label>
        <input
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
        onClick={() => runOptimization.mutate()}
        disabled={!selectedProblemId || runOptimization.isPending}
        className="w-full py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50"
      >
        {runOptimization.isPending ? 'Running…' : 'Run Optimization'}
      </button>

      {runOptimization.isError && (
        <p className="text-red-600 text-sm">{(runOptimization.error as Error).message}</p>
      )}
    </div>
  );
}
