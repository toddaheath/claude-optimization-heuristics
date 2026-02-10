namespace OptimizationHeuristics.Api.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = new();

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(List<string> errors) => new() { Success = false, Errors = errors };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Errors = new List<string> { error } };
}
