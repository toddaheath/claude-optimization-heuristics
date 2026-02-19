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
  const { setCurrentRun } = useStore();

  const [algorithmType, setAlgorithmType] = useState<AlgorithmType>(AlgorithmType.SimulatedAnnealing);
  const [parameters, setParameters] = useState<Record<string, number>>(
    DEFAULT_PARAMETERS[AlgorithmType.SimulatedAnnealing],
  );
  const [maxIterations, setMaxIterations] = useState(500);
  const [selectedProblemId, setSelectedProblemId] = useState<string>('');
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

      const run = await runApi.run({
        algorithmConfigurationId: config.id,
        problemDefinitionId: selectedProblemId,
      });

      return run;
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

  const generateRandomCities = () => {
    const cities: City[] = Array.from({ length: cityCount }, (_, i) => ({
      id: i,
      x: Math.round(Math.random() * 500),
      y: Math.round(Math.random() * 400),
    }));
    createProblem.mutate({
      name: `Random ${cityCount} cities`,
      description: `Randomly generated ${cityCount} cities`,
      cities,
    });
  };

  const generateCircleCities = () => {
    const cx = 250;
    const cy = 200;
    const radius = 180;
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

  return (
    <div className="space-y-4 p-4 bg-gray-50 rounded-lg border">
      <h2 className="font-bold text-lg">Configuration</h2>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Problem</label>
        <select
          value={selectedProblemId}
          onChange={(e) => setSelectedProblemId(e.target.value)}
          className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
        >
          <option value="">Select a problem...</option>
          {problems?.map((p: ProblemDefinition) => (
            <option key={p.id} value={p.id}>
              {p.name} ({p.cityCount} cities)
            </option>
          ))}
        </select>

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
          <div className="flex gap-2">
            <button
              onClick={generateRandomCities}
              disabled={createProblem.isPending}
              className="flex-1 px-2 py-1.5 bg-green-600 text-white rounded text-xs font-medium hover:bg-green-700 disabled:opacity-50"
            >
              {createProblem.isPending ? 'Generating...' : 'Random'}
            </button>
            <button
              onClick={generateCircleCities}
              disabled={createProblem.isPending}
              className="flex-1 px-2 py-1.5 bg-purple-600 text-white rounded text-xs font-medium hover:bg-purple-700 disabled:opacity-50"
            >
              {createProblem.isPending ? 'Generating...' : 'Circle'}
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
        {runOptimization.isPending ? 'Running...' : 'Run Optimization'}
      </button>

      {runOptimization.isError && (
        <p className="text-red-600 text-sm">{(runOptimization.error as Error).message}</p>
      )}
    </div>
  );
}
