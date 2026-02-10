namespace OptimizationHeuristics.Core.Models;

public class Route
{
    public List<int> CityOrder { get; }
    public double TotalDistance { get; }

    public Route(List<int> cityOrder, double totalDistance)
    {
        CityOrder = cityOrder;
        TotalDistance = totalDistance;
    }

    public static double CalculateTotalDistance(List<int> cityOrder, IReadOnlyList<City> cities)
    {
        var total = 0.0;
        for (var i = 0; i < cityOrder.Count - 1; i++)
        {
            total += cities[cityOrder[i]].DistanceTo(cities[cityOrder[i + 1]]);
        }
        total += cities[cityOrder[^1]].DistanceTo(cities[cityOrder[0]]);
        return total;
    }
}
