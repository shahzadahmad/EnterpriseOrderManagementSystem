
using EnterpriseOrderManagementSystem.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnterpriseOrderManagementSystem.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string Value);

    #region No Validators Tests

    [Fact]
    public async Task Handle_ShouldCallNextWhenNoValidatorsExist()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(
            [],
            NullLogger<ValidationBehavior<TestRequest, string>>.Instance);

        var called = false;

        // Act
        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ =>
            {
                called = true;
                return Task.FromResult("success");
            },
            CancellationToken.None);

        // Assert
        Assert.True(called);
        Assert.Equal("success", result);
    }

    #endregion

    #region Successful Validation Tests

    [Fact]
    public async Task Handle_ShouldCallNextWhenValidationSucceeds()
    {
        // Arrange
        var validator = new InlineValidator<TestRequest>();

        validator.RuleFor(x => x.Value)
            .NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>(
            [validator],
            NullLogger<ValidationBehavior<TestRequest, string>>.Instance);

        var called = false;

        // Act
        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ =>
            {
                called = true;
                return Task.FromResult("success");
            },
            CancellationToken.None);

        // Assert
        Assert.True(called);
        Assert.Equal("success", result);
    }

    #endregion

    #region Failed Validation Tests

    [Fact]
    public async Task Handle_ShouldThrowValidationExceptionAndNotCallNextWhenValidationFails()
    {
        // Arrange
        var validator = new InlineValidator<TestRequest>();

        validator.RuleFor(x => x.Value)
            .NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>(
            [validator],
            NullLogger<ValidationBehavior<TestRequest, string>>.Instance);

        var called = false;

        // Act
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(""),
                _ =>
                {
                    called = true;
                    return Task.FromResult("should-not-run");
                },
                CancellationToken.None));

        // Assert
        Assert.False(called);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_ShouldThrowOperationCanceledExceptionWhenTokenIsCancelled()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var validator = new InlineValidator<TestRequest>();

        validator.RuleFor(x => x.Value)
            .NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>(
            [validator],
            NullLogger<ValidationBehavior<TestRequest, string>>.Instance);

        var called = false;

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            behavior.Handle(
                new TestRequest("ok"),
                _ =>
                {
                    called = true;
                    return Task.FromResult("success");
                },
                cancellationTokenSource.Token));

        Assert.False(called);
    }

    #endregion
}
