import { memo } from 'react';

interface Props {
  parameters: Record<string, number>;
  onChange: (params: Record<string, number>) => void;
}

interface ParamMeta {
  label: string;
  min: number;
  max: number;
  step: number;
  integer?: boolean;
}

const PARAM_META: Record<string, ParamMeta> = {
  initialTemperature: { label: 'Initial Temperature', min: 0.01, max: 1_000_000, step: 100 },
  coolingRate:         { label: 'Cooling Rate',        min: 0.0001, max: 0.9999, step: 0.005 },
  minTemperature:      { label: 'Min Temperature',     min: 0.0001, max: 100_000, step: 0.01 },
  antCount:            { label: 'Ant Count',           min: 1, max: 1000, step: 1, integer: true },
  alpha:               { label: 'Alpha (pheromone weight)', min: 0, max: 10, step: 0.1 },
  beta:                { label: 'Beta (distance weight)',   min: 0, max: 10, step: 0.1 },
  evaporationRate:     { label: 'Evaporation Rate',    min: 0.01, max: 0.99, step: 0.05 },
  pheromoneDeposit:    { label: 'Pheromone Deposit',   min: 0.01, max: 100_000, step: 10 },
  populationSize:      { label: 'Population Size',     min: 4, max: 10_000, step: 1, integer: true },
  mutationRate:        { label: 'Mutation Rate',        min: 0, max: 1, step: 0.01 },
  tournamentSize:      { label: 'Tournament Size',     min: 2, max: 10_000, step: 1, integer: true },
  eliteCount:          { label: 'Elite Count',          min: 0, max: 10_000, step: 1, integer: true },
  swarmSize:           { label: 'Swarm Size',           min: 2, max: 10_000, step: 1, integer: true },
  cognitiveWeight:     { label: 'Cognitive Weight',     min: 0, max: 10, step: 0.1 },
  socialWeight:        { label: 'Social Weight',        min: 0, max: 10, step: 0.1 },
  z:                   { label: 'Exploration Factor (z)', min: 0, max: 1, step: 0.01 },
  tabuTenure:          { label: 'Tabu Tenure',          min: 1, max: 100_000, step: 1, integer: true },
  neighborhoodSize:    { label: 'Neighborhood Size',    min: 1, max: 100_000, step: 1, integer: true },
};

function isOutOfRange(key: string, value: number): boolean {
  const meta = PARAM_META[key];
  if (!meta) return false;
  return value < meta.min || value > meta.max;
}

export const ParameterForm = memo(function ParameterForm({ parameters, onChange }: Props) {
  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">Parameters</label>
      {Object.entries(parameters).map(([key, value]) => {
        const meta = PARAM_META[key];
        const outOfRange = isOutOfRange(key, value);
        return (
          <div key={key}>
            <label htmlFor={`param-${key}`} className="block text-xs text-gray-600 mb-0.5">
              {meta?.label ?? key}
            </label>
            <input
              id={`param-${key}`}
              type="number"
              min={meta?.min}
              max={meta?.max}
              step={meta?.step ?? 'any'}
              value={value}
              onChange={(e) => {
                const raw = parseFloat(e.target.value);
                const parsed = meta?.integer ? Math.round(raw) : raw;
                onChange({ ...parameters, [key]: parsed || 0 });
              }}
              className={`w-full px-2 py-1 border rounded text-sm bg-white ${outOfRange ? 'border-red-400' : ''}`}
            />
            {outOfRange && meta && (
              <p className="text-xs text-red-500 mt-0.5">
                Must be between {meta.min} and {meta.max}
              </p>
            )}
          </div>
        );
      })}
    </div>
  );
});
