interface Props {
  parameters: Record<string, number>;
  onChange: (params: Record<string, number>) => void;
}

const PARAM_LABELS: Record<string, string> = {
  initialTemperature: 'Initial Temperature',
  coolingRate: 'Cooling Rate',
  minTemperature: 'Min Temperature',
  antCount: 'Ant Count',
  alpha: 'Alpha (pheromone weight)',
  beta: 'Beta (distance weight)',
  evaporationRate: 'Evaporation Rate',
  pheromoneDeposit: 'Pheromone Deposit',
  populationSize: 'Population Size',
  mutationRate: 'Mutation Rate',
  tournamentSize: 'Tournament Size',
  eliteCount: 'Elite Count',
  swarmSize: 'Swarm Size',
  cognitiveWeight: 'Cognitive Weight',
  socialWeight: 'Social Weight',
  z: 'Exploration Factor (z)',
};

export function ParameterForm({ parameters, onChange }: Props) {
  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">Parameters</label>
      {Object.entries(parameters).map(([key, value]) => (
        <div key={key} className="flex items-center gap-2">
          <label className="text-xs text-gray-600 w-40 shrink-0">{PARAM_LABELS[key] ?? key}</label>
          <input
            type="number"
            step="any"
            value={value}
            onChange={(e) => onChange({ ...parameters, [key]: parseFloat(e.target.value) || 0 })}
            className="flex-1 px-2 py-1 border rounded text-sm"
          />
        </div>
      ))}
    </div>
  );
}
