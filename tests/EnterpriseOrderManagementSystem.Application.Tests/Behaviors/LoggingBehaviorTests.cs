using EnterpriseOrderManagementSystem.Application.Behaviors;
using Microsoft.Extensions.Logging;
using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Tests.Behaviors;

public sealed class LoggingBehaviorTests
{
    private sealed record TestRequest(string Value);

    [Fact]
    public async Task Handle_ShouldLogStartAndSuccessAndReturnResponse()
    {
        var logger = new RecordingLogger<LoggingBehavior<TestRequest, string>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);

        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ => Task.FromResult("success"),
            CancellationToken.None);

        Assert.Equal("success", result);
        Assert.Contains(logger.Entries, x => x.Level == LogLevel.Information && x.Message.Contains("Handling request"));
        Assert.Contains(logger.Entries, x => x.Level == LogLevel.Information && x.Message.Contains("Successfully handled request"));
    }

    [Fact]
    public async Task Handle_ShouldLogErrorAndRethrowWhenNextFails()
    {
        var logger = new RecordingLogger<LoggingBehavior<TestRequest, string>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger);
        var expected = new InvalidOperationException("failed");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestRequest("fail"),
                _ => Task.FromException<string>(expected),
                CancellationToken.None));

        Assert.Same(expected, actual);
        Assert.Contains(logger.Entries, x => x.Level == LogLevel.Error && x.Exception is InvalidOperationException);
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<Entry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new Entry(logLevel, formatter(state, exception), exception));
        }

        public sealed record Entry(LogLevel Level, string Message, Exception? Exception);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
