import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { configApi } from '../api/client';
import { AlgorithmType, ALGORITHM_LABELS, DEFAULT_PARAMETERS } from '../types';
import type { AlgorithmConfiguration } from '../types';
import { AlgorithmSelector } from '../components/AlgorithmSelector';
import { ParameterForm } from '../components/ParameterForm';

export function ConfigurationsPage() {
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [algorithmType, setAlgorithmType] = useState<AlgorithmType>(AlgorithmType.SimulatedAnnealing);
  const [parameters, setParameters] = useState<Record<string, number>>(
    DEFAULT_PARAMETERS[AlgorithmType.SimulatedAnnealing],
  );
  const [maxIterations, setMaxIterations] = useState(500);

  const { data: configs, isLoading } = useQuery({
    queryKey: ['configs'],
    queryFn: configApi.getAll,
  });

  const createConfig = useMutation({
    mutationFn: configApi.create,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['configs'] });
      setShowForm(false);
      setName('');
      setDescription('');
    },
  });

  const deleteConfig = useMutation({
    mutationFn: configApi.delete,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['configs'] }),
  });

  const handleAlgorithmChange = (type: AlgorithmType) => {
    setAlgorithmType(type);
    setParameters(DEFAULT_PARAMETERS[type]);
  };

  return (
    <div className="max-w-screen-xl mx-auto p-6">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Algorithm Configurations</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          {showForm ? 'Cancel' : 'New Configuration'}
        </button>
      </div>

      {showForm && (
        <div className="p-4 bg-gray-50 rounded-lg border mb-6 space-y-3">
          <div>
            <label htmlFor="config-name" className="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input
              id="config-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
              placeholder="Configuration name"
            />
          </div>

          <div>
            <label htmlFor="config-description" className="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <input
              id="config-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
              placeholder="Optional description"
            />
          </div>

          <AlgorithmSelector value={algorithmType} onChange={handleAlgorithmChange} />

          <div>
            <label htmlFor="config-max-iterations" className="block text-sm font-medium text-gray-700 mb-1">Max Iterations</label>
            <input
              id="config-max-iterations"
              type="number"
              min={1}
              value={maxIterations}
              onChange={(e) => setMaxIterations(Number(e.target.value))}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
            />
          </div>

          <ParameterForm parameters={parameters} onChange={setParameters} />

          <button
            onClick={() =>
              createConfig.mutate({ name, description, algorithmType, parameters, maxIterations })
            }
            disabled={!name || createConfig.isPending}
            className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
          >
            {createConfig.isPending ? 'Saving...' : 'Save Configuration'}
          </button>
        </div>
      )}

      {isLoading && <p className="text-gray-500">Loading...</p>}

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {configs?.map((config: AlgorithmConfiguration) => (
          <div key={config.id} className="p-4 border rounded-lg bg-white">
            <div className="flex items-start justify-between">
              <div>
                <h3 className="font-semibold">{config.name}</h3>
                <p className="text-sm text-gray-500">{ALGORITHM_LABELS[config.algorithmType]}</p>
              </div>
              <button
                onClick={() => deleteConfig.mutate(config.id)}
                className="text-red-500 hover:text-red-700 text-sm"
              >
                Delete
              </button>
            </div>
            {config.description && <p className="text-sm text-gray-600 mt-1">{config.description}</p>}
            <div className="mt-2 text-xs text-gray-500">
              <p>Max Iterations: {config.maxIterations}</p>
              <p>
                Params:{' '}
                {Object.entries(config.parameters)
                  .map(([k, v]) => `${k}=${v}`)
                  .join(', ')}
              </p>
            </div>
          </div>
        ))}
      </div>

      {configs?.length === 0 && <p className="text-gray-500 mt-4">No configurations saved yet.</p>}
    </div>
  );
}
