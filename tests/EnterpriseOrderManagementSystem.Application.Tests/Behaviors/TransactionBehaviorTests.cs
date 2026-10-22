using EnterpriseOrderManagementSystem.Application.Behaviors;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Application.Abstractions.Persistence;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Behaviors;

public sealed class TransactionBehaviorTests
{
    private sealed record TestCommand(string Value) : ICommand<string>;

    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDomainEventDispatcher> _domainEventDispatcher = new();

    private TransactionBehavior<TestCommand, string> CreateBehavior()
    {
        return new TransactionBehavior<TestCommand, string>(
            _uow.Object,
            _domainEventDispatcher.Object);
    }

    [Fact]
    public async Task Handle_ShouldBeginSaveCommitAndReturnResponse()
    {
        // Arrange
        var behavior = CreateBehavior();

        // Act
        var result = await behavior.Handle(
            new TestCommand("ok"),
            _ => Task.FromResult("success"),
            CancellationToken.None);

        // Assert
        Assert.Equal("success", result);

        _uow.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRollbackWhenHandlerFails()
    {
        // Arrange
        var behavior = CreateBehavior();
        var expected = new InvalidOperationException("handler failed");

        // Act
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestCommand("fail"),
                _ => Task.FromException<string>(expected),
                CancellationToken.None));

        // Assert
        Assert.Same(expected, actual);

        _uow.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _uow.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _uow.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRollbackWhenSaveChangesFails()
    {
        // Arrange
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("save failed"));

        var behavior = CreateBehavior();

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestCommand("save-fail"),
                _ => Task.FromResult("success"),
                CancellationToken.None));

        // Assert
        _uow.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _uow.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRollbackWhenCommitFails()
    {
        // Arrange
        _uow.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("commit failed"));

        var behavior = CreateBehavior();

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestCommand("commit-fail"),
                _ => Task.FromResult("success"),
                CancellationToken.None));

        // Assert
        _uow.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}