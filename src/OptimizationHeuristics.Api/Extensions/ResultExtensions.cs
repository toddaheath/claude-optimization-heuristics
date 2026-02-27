using FluentResults;
using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;
using OptimizationHeuristics.Core.Errors;

namespace OptimizationHeuristics.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value));

        var errors = result.Errors;
        var messages = errors.Select(e => e.Message).ToList();

        if (errors.Any(e => e is UnauthorizedError))
            return new UnauthorizedObjectResult(ApiResponse<T>.Fail(messages));

        if (errors.Any(e => e is NotFoundError))
            return new NotFoundObjectResult(ApiResponse<T>.Fail(messages));

        return new BadRequestObjectResult(ApiResponse<T>.Fail(messages));
    }

    public static ActionResult ToCreatedResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new ObjectResult(ApiResponse<T>.Ok(result.Value)) { StatusCode = 201 };

        return result.ToActionResult();
    }

    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        var errors = result.Errors;
        var messages = errors.Select(e => e.Message).ToList();

        if (errors.Any(e => e is UnauthorizedError))
            return new UnauthorizedObjectResult(ApiResponse<object>.Fail(messages));

        if (errors.Any(e => e is NotFoundError))
            return new NotFoundObjectResult(ApiResponse<object>.Fail(messages));

        return new BadRequestObjectResult(ApiResponse<object>.Fail(messages));
    }
}
