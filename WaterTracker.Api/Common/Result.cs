namespace WaterTracker.Api.Common;

public class Result<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }

    public static Result<T> SuccessResult(T data) => new() { Success = true, Data = data };
    public static Result<T> SuccessBool() => new() { Success = true };
    public static Result<T> Failure(string error) => new() { Success = false, Error = error };
}
