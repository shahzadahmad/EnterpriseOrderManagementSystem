using EnterpriseOrderManagementSystem.Application.Common.Results;

namespace EnterpriseOrderManagementSystem.Application.Tests.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithErrors()
    {
        var result = Result.Failure("Error 1", "Error 2");

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal(["Error 1", "Error 2"], result.Errors);
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void GenericFailure_ShouldHaveNoValueAndContainErrors()
    {
        var result = Result<int>.Failure("Invalid value");

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Failure, result.Status);

        Assert.False(result.HasValue);

        Assert.Single(result.Errors);
        Assert.Equal("Invalid value", result.Errors.Single());
    }
}
