using EnterpriseOrderManagementSystem.Application.Behaviors;
using EnterpriseOrderManagementSystem.Application.Common.Interfaces;
using EnterpriseOrderManagementSystem.Domain.Repositories;
using Moq;

namespace EnterpriseOrderManagementSystem.Application.Tests.Behaviors;

public sealed class TransactionBehaviorTests
{
    private sealed record TestCommand(string Value) : ICommand<string>;

    [Fact]
    public async Task Handle_ShouldBeginSaveCommitAndReturnResponse()
    {
        var uow = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestCommand, string>(uow.Object);

        var result = await behavior.Handle(
            new TestCommand("ok"),
            _ => Task.FromResult("success"),
            CancellationToken.None);

        Assert.Equal("success", result);
        uow.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRollbackWhenHandlerFails()
    {
        var uow = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestCommand, string>(uow.Object);
        var expected = new InvalidOperationException("handler failed");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestCommand("fail"),
                _ => Task.FromException<string>(expected),
                CancellationToken.None));

        Assert.Same(expected, actual);
        uow.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRollbackWhenSaveChangesFails()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("save failed"));

        var behavior = new TransactionBehavior<TestCommand, string>(uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestCommand("save-fail"),
                _ => Task.FromResult("success"),
                CancellationToken.None));

        uow.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRollbackWhenCommitFails()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("commit failed"));

        var behavior = new TransactionBehavior<TestCommand, string>(uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestCommand("commit-fail"),
                _ => Task.FromResult("success"),
                CancellationToken.None));

        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
