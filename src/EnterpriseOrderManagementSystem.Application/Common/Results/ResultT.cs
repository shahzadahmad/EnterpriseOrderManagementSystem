namespace EnterpriseOrderManagementSystem.Application.Common.Results;

public sealed class Result<T> : Result
{
    public T Value { get; }

    public bool HasValue { get; }

    private Result(
        bool isSuccess,
        ResultStatus status,
        T value,
        bool hasValue,
        IReadOnlyCollection<string> errors)
        : base(
            isSuccess,
            status,
            errors)
    {
        Value = value;
        HasValue = hasValue;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(
            true,
            ResultStatus.Success,
            value,
            true,
            Array.Empty<string>());
    }

    public static Result<T> Failure(
        params string[] errors)
    {
        return new Result<T>(
            false,
            ResultStatus.Failure,
            default!,
            false,
            errors);
    }
}