using System.Threading.Tasks.Sources;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics;

/// <summary>
/// List some best practices to create <see cref="ValueTask"/>.
/// 
/// Note that we prefer to use <see cref="Task"/> over <see cref="ValueTask"/> generally, unless benchmarked otherwise.
/// <see cref="ValueTask"/> is usually used when most of the asynchronous logic can end up synchronous, or to lower
/// very frequent API to lower the pressure on the heap allocation.
/// </summary>
[TestClass]
public class Recipe2P10CreateValueTasks
{
    [TestMethod]
    public async Task ConsumeSimpleValueTask()
    {
        async ValueTask<int> ReturnSimpleValueTask()
        {
            await Task.Yield();
            return 42;
        }

        (await ReturnSimpleValueTask()).Should().Be(42);
    }

    private static bool _mustBeAsync;

    [TestMethod]
    public async Task ConsumeSynchronousValueTaskMostOfTime()
    {
        async Task<int> ComplexLogic()
        {
            await Task.Yield();
            return 41;
        }

        ValueTask<int> ReturnSyncValueTaskMostOfTime()
        {
            // If the expensive path must be executed, a complex Task may be used (at the expense of heap allocation).
            if (_mustBeAsync)
            {
                _mustBeAsync = false;
                return new ValueTask<int>(ComplexLogic());
            }

            // For the synchronous path, we can directly return a ValueTask, saving an object allocation on the heap.
            return new ValueTask<int>(42);
        }

        _mustBeAsync = true;

        var val1 = await ReturnSyncValueTaskMostOfTime();
        var val2 = await ReturnSyncValueTaskMostOfTime();
        var val3 = await ReturnSyncValueTaskMostOfTime();

        val1.Should().Be(41);
        val2.Should().Be(val3).And.Be(42);
    }

    /// <summary>
    /// Prevents the allocation of the source of <see cref="ValueTask"/> to minimize the heap allocation pressure.
    /// This is demonstrated using a custom class which delays operation in time (<see cref="DelayOperation"/>).
    /// </summary>
    [TestMethod]
    public async Task UsePooledValueTaskSource()
    {
        var delay = new DelayOperation();

        var vt1 = delay.DelayAsync(TimeSpan.FromMilliseconds(1), 42);
        var val1 = await vt1;

        delay.Reset();

        var vt2 = delay.DelayAsync(TimeSpan.FromMilliseconds(2), 43);
        var val2 = await vt2;

        val1.Should().Be(42);
        val2.Should().Be(43);
    }

    /// <summary>
    /// A wrapper around the .NET built-in type <see cref="ManualResetValueTaskSourceCore{T}"/>, used to prevent
    /// the heap allocation of instances of <see cref="Task"/>.
    /// The method <see cref="GetValueTask"/> creates a new struct (stack-allocated) <see cref="ValueTask"/> on each
    /// call which represents the same underlying <see cref="ManualResetValueTaskSource{T}"/>.
    /// This type is a class (not a struct), so it is referenced (not copied) by the <see cref="ValueTask"/> instances.
    /// </summary>
    /// <seealso href="https://github.com/dotnet/runtime/issues/27558#issue-558423566">
    /// The implementation of <see cref="ManualResetValueTaskSource{T}"/> by Microsoft (Stephen Toub).
    /// </seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.sources.manualresetvaluetasksourcecore-1"/>
    private class ManualResetValueTaskSource<T> : IValueTaskSource<T>, IValueTaskSource
    {
        private ManualResetValueTaskSourceCore<T> _logic; // mutable struct; do not make this readonly

        public bool RunContinuationsAsynchronously
        {
            get => _logic.RunContinuationsAsynchronously;
            set => _logic.RunContinuationsAsynchronously = value;
        }

        /// <summary>
        /// The <see cref="ManualResetValueTaskSourceCore{T}.Version"/> is a safety check value to make sure the
        /// <see cref="ValueTask"/> is not awaited many times. Internally, this is an incremented number.
        /// </summary>
        public ValueTask<T> GetValueTask() => new(this, _logic.Version);
        public void Reset() => _logic.Reset();
        public void SetResult(T result) => _logic.SetResult(result);
        public void SetException(Exception error) => _logic.SetException(error);

        void IValueTaskSource.GetResult(short token) => _logic.GetResult(token);
        T IValueTaskSource<T>.GetResult(short token) => _logic.GetResult(token);

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) => _logic.GetStatus(token);
        ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token) => _logic.GetStatus(token);

        void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token,
            ValueTaskSourceOnCompletedFlags flags) => _logic.OnCompleted(continuation, state, token, flags);

        void IValueTaskSource<T>.OnCompleted(Action<object?> continuation, object? state, short token,
            ValueTaskSourceOnCompletedFlags flags) => _logic.OnCompleted(continuation, state, token, flags);
    }

    /// <summary>
    /// An example of using the type <see cref="ManualResetValueTaskSource{T}"/> to provide asynchronous logic
    /// with minimal heap allocation.
    /// </summary>
    public class DelayOperation
    {
        /// <summary>
        /// The only heap-allocated object, which controls the creation and the completion of <see cref="ValueTask"/>s.
        /// </summary>
        private readonly ManualResetValueTaskSource<int> _source = new();
        private Timer? _timer;

        public ValueTask<int> DelayAsync(TimeSpan delay, int result)
        {
            // For the synchronous path, ee can return a new ValueTask without heap or complex allocation.
            if (delay.Equals(TimeSpan.Zero)) return new ValueTask<int>(result);
            
            // Otherwise, we can use the ValueTask source for real allocation-less asynchronous code.
            // When the timer ends, set the value task as completed.
            _timer = new Timer(_ => { _source.SetResult(result); }, null, delay, Timeout.InfiniteTimeSpan);

            return _source.GetValueTask();
        }

        /// <summary>
        /// Offers the possibility to reset the timer, which resets the value task as well.
        /// </summary>
        public void Reset()
        {
            _timer?.Dispose();
            _source.Reset();
        }
    }
}