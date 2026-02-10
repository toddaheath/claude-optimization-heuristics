using FluentResults;
using Microsoft.AspNetCore.Mvc;
using OptimizationHeuristics.Api.DTOs;

namespace OptimizationHeuristics.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value));

        var errors = result.Errors.Select(e => e.Message).ToList();
        if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            return new NotFoundObjectResult(ApiResponse<T>.Fail(errors));

        return new BadRequestObjectResult(ApiResponse<T>.Fail(errors));
    }

    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        var errors = result.Errors.Select(e => e.Message).ToList();
        if (errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            return new NotFoundObjectResult(ApiResponse<object>.Fail(errors));

        return new BadRequestObjectResult(ApiResponse<object>.Fail(errors));
    }
}
