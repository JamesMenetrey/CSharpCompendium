using FluentAssertions;
using Nito.AsyncEx;

namespace ConcurrencyInCSharpCookbook.Chapter12Synchronization;

[TestClass]
public class Chapter12P4AsyncSignals
{
    /// <summary>
    /// <see cref="TaskCompletionSource"/> can be used just once for signaling.
    /// </summary>
    [TestMethod]
    public async Task UsingTaskCompletionSourceForOneShotSignal()
    {
        var demo = new DemonstrateTaskCompletionSourceForOneShotSignal();

        var initializationTask = Task.Run(() => demo.WaitForInitialization());
        await Task.Run(() => demo.InitializeFromAnotherThread(42));

        var value = await initializationTask;

        value.Should().Be(42);
    }
    
    /// <summary>
    /// <see cref="AsyncManualResetEvent"/> is an async equivalent of <see cref="ManualResetEvent"/> provided by
    /// the library <see cref="Nito.AsyncEx"/>.
    /// </summary>
    [TestMethod]
    public async Task UsingAsyncManualResetEvent()
    {
        var demo = new DemonstrateAsyncManualResetEvent();

        var initializationTask = Task.Run(() => demo.WaitForInitialization());
        await Task.Run(() => demo.InitializeFromAnotherThread(42));

        var value = await initializationTask;

        value.Should().Be(42);
    }

    private class DemonstrateTaskCompletionSourceForOneShotSignal
    {
        private readonly TaskCompletionSource _signal = new();
        private int _value;

        public async Task<int> WaitForInitialization()
        {
            await _signal.Task;
            return _value;
        }

        public void InitializeFromAnotherThread(int value)
        {
            _value = value;
            _signal.SetResult();
        }
    }

    private class DemonstrateAsyncManualResetEvent
    {
        private readonly AsyncManualResetEvent _signal = new();
        private int _value;

        public async Task<int> WaitForInitialization()
        {
            await _signal.WaitAsync();
            return _value;
        }

        public void InitializeFromAnotherThread(int value)
        {
            _value = value;
            _signal.Set();
        }
    }
}