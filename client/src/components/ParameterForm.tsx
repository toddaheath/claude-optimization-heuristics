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
  tabuTenure: 'Tabu Tenure',
  neighborhoodSize: 'Neighborhood Size',
};

export function ParameterForm({ parameters, onChange }: Props) {
  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">Parameters</label>
      {Object.entries(parameters).map(([key, value]) => (
        <div key={key}>
          <label className="block text-xs text-gray-600 mb-0.5">{PARAM_LABELS[key] ?? key}</label>
          <input
            type="number"
            step="any"
            value={value}
            onChange={(e) => onChange({ ...parameters, [key]: parseFloat(e.target.value) || 0 })}
            className="w-full px-2 py-1 border rounded text-sm bg-white"
          />
        </div>
      ))}
    </div>
  );
}
