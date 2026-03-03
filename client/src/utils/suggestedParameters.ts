import { AlgorithmType, DEFAULT_PARAMETERS } from '../types';

export function getSuggestedParameters(
  algorithmType: AlgorithmType,
  cityCount: number,
): { parameters: Record<string, number>; maxIterations: number } {
  const defaults = DEFAULT_PARAMETERS[algorithmType];
  const maxIterations = Math.min(100000, Math.max(500, cityCount * 25));

  switch (algorithmType) {
    case AlgorithmType.SimulatedAnnealing: {
      const coolingRate = Math.min(0.999, Math.max(0.9, 1 - 5 / (cityCount * 25)));
      return {
        parameters: {
          initialTemperature: cityCount * 500,
          coolingRate,
          minTemperature: 0.01,
        },
        maxIterations,
      };
    }

    case AlgorithmType.AntColonyOptimization:
      return {
        parameters: {
          ...defaults,
          antCount: Math.max(10, cityCount),
        },
        maxIterations,
      };

    case AlgorithmType.GeneticAlgorithm: {
      const populationSize = Math.max(50, cityCount * 3);
      return {
        parameters: {
          populationSize,
          mutationRate: Math.min(0.1, 2 / cityCount),
          tournamentSize: Math.max(3, Math.round(populationSize * 0.1)),
          eliteCount: Math.max(2, Math.round(populationSize * 0.04)),
        },
        maxIterations,
      };
    }

    case AlgorithmType.ParticleSwarmOptimization:
      return {
        parameters: {
          ...defaults,
          swarmSize: Math.max(20, cityCount * 2),
        },
        maxIterations,
      };

    case AlgorithmType.SlimeMoldOptimization:
      return {
        parameters: {
          populationSize: Math.max(20, cityCount * 2),
          z: 0.03,
        },
        maxIterations,
      };

    case AlgorithmType.TabuSearch:
      return {
        parameters: {
          tabuTenure: Math.max(5, Math.round(Math.sqrt(cityCount))),
          neighborhoodSize: Math.min(500, Math.round((cityCount * (cityCount - 1)) / 4)),
        },
        maxIterations,
      };

    default:
      return { parameters: { ...defaults }, maxIterations };
  }
}
