using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class ParticleSwarmOptimization : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        List<IterationResult> history)
    {
        var swarmSize = GetIntParam(parameters, "swarmSize", 30);
        var cognitiveWeight = GetParam(parameters, "cognitiveWeight", 2.0);
        var socialWeight = GetParam(parameters, "socialWeight", 2.0);

        var n = cities.Count;
        var particles = new List<List<int>>(swarmSize);
        var personalBest = new List<List<int>>(swarmSize);
        var personalBestDistance = new double[swarmSize];
        var velocities = new List<List<(int, int)>>(swarmSize);

        for (var i = 0; i < swarmSize; i++)
        {
            var route = GenerateRandomRoute(n, Rng);
            particles.Add(route);
            personalBest.Add(new List<int>(route));
            personalBestDistance[i] = Route.CalculateTotalDistance(route, cities);
            velocities.Add(new List<(int, int)>());
        }

        var globalBestIdx = Array.IndexOf(personalBestDistance, personalBestDistance.Min());
        var globalBest = new List<int>(personalBest[globalBestIdx]);
        var globalBestDistance = personalBestDistance[globalBestIdx];

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            for (var i = 0; i < swarmSize; i++)
            {
                // Compute swap sequences toward personal and global best
                var personalSwaps = ComputeSwapSequence(particles[i], personalBest[i]);
                var globalSwaps = ComputeSwapSequence(particles[i], globalBest);

                // Probabilistically apply swaps
                var newVelocity = new List<(int, int)>();
                foreach (var swap in personalSwaps)
                    if (Rng.NextDouble() < cognitiveWeight / (cognitiveWeight + socialWeight))
                        newVelocity.Add(swap);
                foreach (var swap in globalSwaps)
                    if (Rng.NextDouble() < socialWeight / (cognitiveWeight + socialWeight))
                        newVelocity.Add(swap);

                velocities[i] = newVelocity;

                // Apply velocity (swap sequence)
                foreach (var (a, b) in velocities[i])
                    (particles[i][a], particles[i][b]) = (particles[i][b], particles[i][a]);

                var distance = Route.CalculateTotalDistance(particles[i], cities);
                if (distance < personalBestDistance[i])
                {
                    personalBest[i] = new List<int>(particles[i]);
                    personalBestDistance[i] = distance;
                }

                if (distance < globalBestDistance)
                {
                    globalBest = new List<int>(particles[i]);
                    globalBestDistance = distance;
                }
            }

            history.Add(new IterationResult(iteration, globalBestDistance, new List<int>(globalBest)));
        }

        return (globalBest, globalBestDistance);
    }

    private static List<(int, int)> ComputeSwapSequence(List<int> current, List<int> target)
    {
        var swaps = new List<(int, int)>();
        var temp = new List<int>(current);
        var posMap = new int[temp.Count];
        for (var i = 0; i < temp.Count; i++)
            posMap[temp[i]] = i;

        for (var i = 0; i < target.Count; i++)
        {
            if (temp[i] != target[i])
            {
                var j = posMap[target[i]];
                swaps.Add((i, j));
                posMap[temp[i]] = j;
                posMap[temp[j]] = i;
                (temp[i], temp[j]) = (temp[j], temp[i]);
            }
        }

        return swaps;
    }
}
