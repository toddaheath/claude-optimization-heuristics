export interface City {
  id: number;
  x: number;
  y: number;
  name?: string;
}

export const AlgorithmType = {
  SimulatedAnnealing: 'SimulatedAnnealing',
  AntColonyOptimization: 'AntColonyOptimization',
  GeneticAlgorithm: 'GeneticAlgorithm',
  ParticleSwarmOptimization: 'ParticleSwarmOptimization',
  SlimeMoldOptimization: 'SlimeMoldOptimization',
} as const;
export type AlgorithmType = (typeof AlgorithmType)[keyof typeof AlgorithmType];

export const RunStatus = {
  Pending: 'Pending',
  Running: 'Running',
  Completed: 'Completed',
  Failed: 'Failed',
} as const;
export type RunStatus = (typeof RunStatus)[keyof typeof RunStatus];

export interface IterationResult {
  iteration: number;
  bestDistance: number;
  bestRoute: number[];
}

export interface ProblemDefinition {
  id: string;
  name: string;
  description?: string;
  cities: City[];
  cityCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface AlgorithmConfiguration {
  id: string;
  name: string;
  description?: string;
  algorithmType: AlgorithmType;
  parameters: Record<string, number>;
  maxIterations: number;
  createdAt: string;
  updatedAt: string;
}

export interface OptimizationRun {
  id: string;
  algorithmConfigurationId: string;
  problemDefinitionId: string;
  status: RunStatus;
  bestDistance?: number;
  bestRoute?: number[];
  iterationHistory?: IterationResult[];
  totalIterations: number;
  executionTimeMs: number;
  createdAt: string;
  updatedAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  errors: string[];
}

export const ALGORITHM_LABELS: Record<AlgorithmType, string> = {
  [AlgorithmType.SimulatedAnnealing]: 'Simulated Annealing',
  [AlgorithmType.AntColonyOptimization]: 'Ant Colony Optimization',
  [AlgorithmType.GeneticAlgorithm]: 'Genetic Algorithm',
  [AlgorithmType.ParticleSwarmOptimization]: 'Particle Swarm Optimization',
  [AlgorithmType.SlimeMoldOptimization]: 'Slime Mold Optimization',
};

export const DEFAULT_PARAMETERS: Record<AlgorithmType, Record<string, number>> = {
  [AlgorithmType.SimulatedAnnealing]: {
    initialTemperature: 10000,
    coolingRate: 0.995,
    minTemperature: 0.01,
  },
  [AlgorithmType.AntColonyOptimization]: {
    antCount: 20,
    alpha: 1.0,
    beta: 5.0,
    evaporationRate: 0.5,
    pheromoneDeposit: 100,
  },
  [AlgorithmType.GeneticAlgorithm]: {
    populationSize: 50,
    mutationRate: 0.02,
    tournamentSize: 5,
    eliteCount: 2,
  },
  [AlgorithmType.ParticleSwarmOptimization]: {
    swarmSize: 30,
    cognitiveWeight: 2.0,
    socialWeight: 2.0,
  },
  [AlgorithmType.SlimeMoldOptimization]: {
    populationSize: 30,
    z: 0.03,
  },
};

export const ALGORITHM_TYPE_VALUES = Object.values(AlgorithmType);
