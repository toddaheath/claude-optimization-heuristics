using FluentResults;

namespace OptimizationHeuristics.Core.Errors;

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message) { }
}
