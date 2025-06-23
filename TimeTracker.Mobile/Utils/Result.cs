
namespace TimeTracker.Mobile.Utils;
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }             // nullable
    public string? Error { get; }        // nullable

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(true, value, null);
    }

    public static Result<T> Fail(string error)
    {
        ArgumentException.ThrowIfNullOrEmpty(error);
        return new(false, default, error);
    }

}
