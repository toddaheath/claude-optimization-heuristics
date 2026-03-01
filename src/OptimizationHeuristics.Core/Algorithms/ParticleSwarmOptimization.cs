using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Algorithms;

public class ParticleSwarmOptimization : AlgorithmBase
{
    protected override (List<int> BestRoute, double BestDistance) RunAlgorithm(
        IReadOnlyList<City> cities, int maxIterations, Dictionary<string, object> parameters,
        IList<IterationResult> history, CancellationToken cancellationToken = default)
    {
        var swarmSize = GetIntParam(parameters, "swarmSize", 30);
        var cognitiveWeight = GetParam(parameters, "cognitiveWeight", 2.0);
        var socialWeight = GetParam(parameters, "socialWeight", 2.0);
        var inertiaMax = GetParam(parameters, "inertiaMax", 0.9);
        var inertiaMin = GetParam(parameters, "inertiaMin", 0.4);

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

        for (var iteration = 0; iteration < maxIterations && !cancellationToken.IsCancellationRequested; iteration++)
        {
            // Linearly decay inertia weight from inertiaMax to inertiaMin over iterations
            double w = inertiaMax - (inertiaMax - inertiaMin) * iteration / maxIterations;

            var iterationBestDistance = double.MaxValue;
            for (var i = 0; i < swarmSize; i++)
            {
                // Compute swap sequences toward personal and global best
                var personalSwaps = ComputeSwapSequence(particles[i], personalBest[i]);
                var globalSwaps = ComputeSwapSequence(particles[i], globalBest);

                // Apply inertia: probabilistically retain swaps from old velocity
                var newVelocity = new List<(int, int)>();
                foreach (var swap in velocities[i])
                    if (Rng.NextDouble() < w)
                        newVelocity.Add(swap);

                // Add cognitive component (toward personal best)
                foreach (var swap in personalSwaps)
                    if (Rng.NextDouble() < cognitiveWeight / (cognitiveWeight + socialWeight))
                        newVelocity.Add(swap);

                // Add social component (toward global best)
                foreach (var swap in globalSwaps)
                    if (Rng.NextDouble() < socialWeight / (cognitiveWeight + socialWeight))
                        newVelocity.Add(swap);

                var maxSwaps = n / 2;
                if (newVelocity.Count > maxSwaps)
                    newVelocity = newVelocity.Take(maxSwaps).ToList();

                velocities[i] = newVelocity;

                // Apply velocity (swap sequence)
                foreach (var (a, b) in velocities[i])
                    (particles[i][a], particles[i][b]) = (particles[i][b], particles[i][a]);

                var distance = Route.CalculateTotalDistance(particles[i], cities);
                if (distance < iterationBestDistance)
                    iterationBestDistance = distance;

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

            // iterationBestDistance = best particle distance found this iteration (noisy)
            history.Add(new IterationResult(iteration, globalBestDistance, new List<int>(globalBest), iterationBestDistance));
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
