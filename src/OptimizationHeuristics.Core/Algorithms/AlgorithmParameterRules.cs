using System.Text.Json;
using OptimizationHeuristics.Core.Enums;

namespace OptimizationHeuristics.Core.Algorithms;

/// <summary>
/// Defines per-algorithm parameter constraints and validates parameter dictionaries.
/// </summary>
public static class AlgorithmParameterRules
{
    public record ParamRule(string Key, bool IsInteger, double Min, double Max);

    private static readonly Dictionary<AlgorithmType, ParamRule[]> Rules = new()
    {
        [AlgorithmType.SimulatedAnnealing] = new ParamRule[]
        {
            new("initialTemperature", false, 0.01, 1_000_000),
            new("coolingRate", false, 0.0001, 0.9999),
            new("minTemperature", false, 0.0001, 100_000),
        },
        [AlgorithmType.AntColonyOptimization] = new ParamRule[]
        {
            new("antCount", true, 1, 1000),
            new("alpha", false, 0, 10),
            new("beta", false, 0, 10),
            new("evaporationRate", false, 0.01, 0.99),
            new("pheromoneDeposit", false, 0.01, 100_000),
        },
        [AlgorithmType.GeneticAlgorithm] = new ParamRule[]
        {
            new("populationSize", true, 4, 10_000),
            new("mutationRate", false, 0, 1),
            new("tournamentSize", true, 2, 10_000),
            new("eliteCount", true, 0, 10_000),
        },
        [AlgorithmType.ParticleSwarmOptimization] = new ParamRule[]
        {
            new("swarmSize", true, 2, 10_000),
            new("cognitiveWeight", false, 0, 10),
            new("socialWeight", false, 0, 10),
            new("inertiaMax", false, 0, 1),
            new("inertiaMin", false, 0, 1),
        },
        [AlgorithmType.SlimeMoldOptimization] = new ParamRule[]
        {
            new("populationSize", true, 2, 10_000),
            new("z", false, 0, 1),
        },
        [AlgorithmType.TabuSearch] = new ParamRule[]
        {
            new("tabuTenure", true, 1, 100_000),
            new("neighborhoodSize", true, 1, 100_000),
        },
    };

    /// <summary>
    /// Returns the known parameter rules for the given algorithm type,
    /// or an empty array if the algorithm type is unknown.
    /// </summary>
    public static ParamRule[] GetRules(AlgorithmType algorithmType) =>
        Rules.TryGetValue(algorithmType, out var rules) ? rules : [];

    /// <summary>
    /// Validates parameter values against the rules for the given algorithm type.
    /// Returns a list of human-readable error messages (empty if valid).
    /// Only validates keys that are present — missing keys use algorithm defaults.
    /// </summary>
    public static List<string> Validate(AlgorithmType algorithmType, Dictionary<string, object>? parameters)
    {
        var errors = new List<string>();
        if (parameters is null) return errors;

        var rules = GetRules(algorithmType);
        var knownKeys = new HashSet<string>(rules.Select(r => r.Key));

        foreach (var key in parameters.Keys)
        {
            if (!knownKeys.Contains(key))
            {
                errors.Add($"Unknown parameter '{key}' for {algorithmType}. Known parameters: {string.Join(", ", knownKeys)}.");
                continue;
            }
        }

        foreach (var rule in rules)
        {
            if (!parameters.TryGetValue(rule.Key, out var value)) continue;

            if (!TryGetNumericValue(value, rule.IsInteger, out var numericValue))
            {
                errors.Add($"Parameter '{rule.Key}' must be {(rule.IsInteger ? "an integer" : "a number")}.");
                continue;
            }

            if (rule.IsInteger && numericValue != Math.Floor(numericValue))
            {
                errors.Add($"Parameter '{rule.Key}' must be an integer.");
                continue;
            }

            if (numericValue < rule.Min || numericValue > rule.Max)
            {
                errors.Add($"Parameter '{rule.Key}' must be between {rule.Min} and {rule.Max} (got {numericValue}).");
            }
        }

        // Cross-parameter validation
        ValidateCrossParameterRules(algorithmType, parameters, errors);

        return errors;
    }

    private static void ValidateCrossParameterRules(AlgorithmType algorithmType, Dictionary<string, object> parameters, List<string> errors)
    {
        switch (algorithmType)
        {
            case AlgorithmType.GeneticAlgorithm:
            {
                if (TryGetValue(parameters, "populationSize", out var popSize) &&
                    TryGetValue(parameters, "tournamentSize", out var tourney) &&
                    tourney > popSize)
                {
                    errors.Add("tournamentSize must be less than or equal to populationSize.");
                }
                if (TryGetValue(parameters, "populationSize", out popSize) &&
                    TryGetValue(parameters, "eliteCount", out var elite) &&
                    elite >= popSize)
                {
                    errors.Add("eliteCount must be less than populationSize.");
                }
                break;
            }
            case AlgorithmType.ParticleSwarmOptimization:
            {
                if (TryGetValue(parameters, "inertiaMin", out var min) &&
                    TryGetValue(parameters, "inertiaMax", out var max) &&
                    min >= max)
                {
                    errors.Add("inertiaMin must be less than inertiaMax.");
                }
                break;
            }
        }
    }

    private static bool TryGetNumericValue(object value, bool expectInteger, out double result)
    {
        result = 0;
        try
        {
            if (value is JsonElement je)
            {
                if (expectInteger && je.ValueKind == JsonValueKind.Number)
                {
                    result = je.GetDouble();
                    return true;
                }
                if (je.ValueKind == JsonValueKind.Number)
                {
                    result = je.GetDouble();
                    return true;
                }
                return false;
            }

            result = Convert.ToDouble(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryGetValue(Dictionary<string, object> parameters, string key, out double value)
    {
        value = 0;
        if (!parameters.TryGetValue(key, out var raw)) return false;
        return TryGetNumericValue(raw, false, out value);
    }
}
