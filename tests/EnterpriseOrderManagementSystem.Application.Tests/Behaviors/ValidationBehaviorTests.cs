using EnterpriseOrderManagementSystem.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string Value);

    [Fact]
    public async Task Handle_ShouldCallNextWhenNoValidatorsExist()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var called = false;

        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ =>
            {
                called = true;
                return Task.FromResult("success");
            },
            CancellationToken.None);

        Assert.True(called);
        Assert.Equal("success", result);
    }

    [Fact]
    public async Task Handle_ShouldCallNextWhenValidationSucceeds()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Value).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var called = false;

        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ =>
            {
                called = true;
                return Task.FromResult("success");
            },
            CancellationToken.None);

        Assert.True(called);
        Assert.Equal("success", result);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationExceptionAndNotCallNextWhenValidationFails()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Value).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>([validator]);
        var called = false;

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(""),
                _ =>
                {
                    called = true;
                    return Task.FromResult("should-not-run");
                },
                CancellationToken.None));

        Assert.False(called);
    }
}
