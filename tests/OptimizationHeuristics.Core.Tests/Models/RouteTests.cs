using FluentAssertions;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Tests.Models;

public class RouteTests
{
    private readonly List<City> _cities = new()
    {
        new City(0, 0.0, 0.0),
        new City(1, 1.0, 0.0),
        new City(2, 1.0, 1.0),
        new City(3, 0.0, 1.0)
    };

    [Fact]
    public void CalculateTotalDistance_SquareRoute_ReturnsPerimeter()
    {
        var order = new List<int> { 0, 1, 2, 3 };
        var distance = Route.CalculateTotalDistance(order, _cities);
        distance.Should().BeApproximately(4.0, 0.0001);
    }

    [Fact]
    public void CalculateTotalDistance_ReversedRoute_ReturnsSameDistance()
    {
        var forward = new List<int> { 0, 1, 2, 3 };
        var backward = new List<int> { 3, 2, 1, 0 };
        var d1 = Route.CalculateTotalDistance(forward, _cities);
        var d2 = Route.CalculateTotalDistance(backward, _cities);
        d1.Should().BeApproximately(d2, 0.0001);
    }

    [Fact]
    public void CalculateTotalDistance_SingleCity_ReturnsZero()
    {
        var cities = new List<City> { new City(0, 0.0, 0.0) };
        var distance = Route.CalculateTotalDistance(new List<int> { 0 }, cities);
        distance.Should().Be(0.0);
    }
}
