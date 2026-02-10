namespace OptimizationHeuristics.Core.Models;

public record City(int Id, double X, double Y, string? Name = null)
{
    public double DistanceTo(City other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
