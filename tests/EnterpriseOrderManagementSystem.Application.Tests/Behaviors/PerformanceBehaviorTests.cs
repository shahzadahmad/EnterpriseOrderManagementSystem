using EnterpriseOrderManagementSystem.Application.Behaviors;
using Microsoft.Extensions.Logging;

namespace EnterpriseOrderManagementSystem.Application.Tests.Behaviors;

public sealed class PerformanceBehaviorTests
{
    private sealed record TestRequest(string Value);

    [Fact]
    public async Task Handle_ShouldLogDebugForFastRequest()
    {
        var logger = new RecordingLogger<PerformanceBehavior<TestRequest, string>>();
        var behavior = new PerformanceBehavior<TestRequest, string>(logger);

        var result = await behavior.Handle(
            new TestRequest("fast"),
            _ => Task.FromResult("success"),
            CancellationToken.None);

        Assert.Equal("success", result);
        Assert.Contains(logger.Entries, x => x.Level == LogLevel.Debug);
        Assert.DoesNotContain(logger.Entries, x => x.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_ShouldLogWarningForLongRunningRequest()
    {
        var logger = new RecordingLogger<PerformanceBehavior<TestRequest, string>>();
        var behavior = new PerformanceBehavior<TestRequest, string>(logger);

        var result = await behavior.Handle(
            new TestRequest("slow"),
            async _ =>
            {
                await Task.Delay(650);
                return "success";
            },
            CancellationToken.None);

        Assert.Equal("success", result);
        Assert.Contains(logger.Entries, x => x.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_ShouldLogTimingEvenWhenNextFails()
    {
        var logger = new RecordingLogger<PerformanceBehavior<TestRequest, string>>();
        var behavior = new PerformanceBehavior<TestRequest, string>(logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(
                new TestRequest("fail"),
                _ => Task.FromException<string>(new InvalidOperationException()),
                CancellationToken.None));

        Assert.NotEmpty(logger.Entries);
        Assert.Contains(logger.Entries, x => x.Level == LogLevel.Debug || x.Level == LogLevel.Warning);
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
            => Entries.Add(new Entry(logLevel, formatter(state, exception), exception));

        public sealed record Entry(LogLevel Level, string Message, Exception? Exception);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
