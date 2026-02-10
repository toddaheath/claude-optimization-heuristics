using FluentAssertions;
using OptimizationHeuristics.Core.Models;

namespace OptimizationHeuristics.Core.Tests.Models;

public class CityTests
{
    [Fact]
    public void DistanceTo_SameCity_ReturnsZero()
    {
        var city = new City(0, 5.0, 5.0);
        city.DistanceTo(city).Should().Be(0.0);
    }

    [Fact]
    public void DistanceTo_KnownDistance_ReturnsCorrectValue()
    {
        var a = new City(0, 0.0, 0.0);
        var b = new City(1, 3.0, 4.0);
        a.DistanceTo(b).Should().BeApproximately(5.0, 0.0001);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var a = new City(0, 1.0, 2.0);
        var b = new City(1, 4.0, 6.0);
        a.DistanceTo(b).Should().BeApproximately(b.DistanceTo(a), 0.0001);
    }
}
