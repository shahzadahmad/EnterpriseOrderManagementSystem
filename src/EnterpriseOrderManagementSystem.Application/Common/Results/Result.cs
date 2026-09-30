namespace EnterpriseOrderManagementSystem.Application.Common.Results;

public class Result
{
    public bool IsSuccess { get; }

    public ResultStatus Status { get; }

    public IReadOnlyCollection<string> Errors { get; }

    protected Result(
        bool isSuccess,
        ResultStatus status,
        IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Status = status;
        Errors = errors;
    }

    public static Result Success()
    {
        return new Result(
            true,
            ResultStatus.Success,
            Array.Empty<string>());
    }

    public static Result Failure(
        params string[] errors)
    {
        return new Result(
            false,
            ResultStatus.Failure,
            errors);
    }
}