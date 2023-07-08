using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter11FunctionalStyle;

[TestClass]
public class Chapter11P6AsyncDisposal
{
    /// <summary>
    /// Implementing <see cref="IDisposable"/> allows to support cancellation of asynchronous processing thanks to
    /// <see cref="CancellationTokenSource"/> which is internal to the class.
    /// </summary>
    [TestMethod]
    public async Task UseSynchronousDisposal()
    {
        var cts = new CancellationTokenSource();
        var hasBeenCancelled = false;
        Task task;

        using (var syncDisposal = new SyncDisposal())
        {
            task = syncDisposal.ExecuteAsync(cts.Token);
        }

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            hasBeenCancelled = true;
        }

        hasBeenCancelled.Should().BeTrue();
    }

    /// <summary>
    /// Implementing <see cref="IAsyncDisposable"/> allows to implements async code in the disposal logic.
    /// </summary>
    [TestMethod]
    public async Task UseAsynchronousDisposal()
    {
        var cts = new CancellationTokenSource();
        var hasBeenCancelled = false;
        Task task;

        await using (var syncDisposal = new AsyncDisposal())
        {
            task = syncDisposal.ExecuteAsync(cts.Token);
        }

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            hasBeenCancelled = true;
        }

        hasBeenCancelled.Should().BeTrue();
    }

    private class SyncDisposal : IDisposable
    {
        private readonly CancellationTokenSource _disposeCts = new();

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
            await Task.Delay(Timeout.Infinite, combinedCts.Token);
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
        }
    }
    
    private class AsyncDisposal : IAsyncDisposable
    {
        private readonly CancellationTokenSource _disposeCts = new();

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
            await Task.Delay(Timeout.Infinite, combinedCts.Token);
        }
        
        public async ValueTask DisposeAsync()
        {
            await Task.Yield();
            _disposeCts.Cancel();
        }
    }
}