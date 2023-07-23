using System.Collections.Immutable;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter14Scenarios;

[TestClass]
public class Recipe14P4ImplicitState
{
    /// <summary>
    /// The type <see cref="AsyncLocal{T}" /> enables an object to have a state which lives on a logical context.
    /// Beware that <see cref="AsyncLocal{T}" /> must store immutable objects only. When updated, the existing value
    /// must be overwritten. One can typically create an helper class to ensure the stored data is immutable
    /// and updated correctly.
    /// <see cref="AsyncLocal{T}" /> supersedes the values bound to a particular thread (notably done using the attribute
    /// <see cref="STAThreadAttribute" />).
    /// </summary>
    [TestMethod]
    public async Task UseImplicitStatesWithAsyncContext()
    {
        var counter = 0;
        var controlOfExecution = 0;
        var state = new ImplicitStateDemo<int>();
        state.Current.Should().BeEmpty();

        // The multiple tasks demonstrate that the state is locale for each task (and not shared).
        var tasks = Enumerable.Repeat(0, 10).Select(_ => Task.Run(() =>
        {
            var value1 = Interlocked.Increment(ref counter);
            var value2 = Interlocked.Increment(ref counter);

            state.Enqueue(value1);
            state.Current.Should().HaveCount(1);
            state.Current.Peek().Should().Be(value1);

            state.Enqueue(value2);
            state.Current.Should().HaveCount(2);
            state.Current.Peek().Should().Be(value1);

            state.Dequeue().Should().Be(value1);
            state.Current.Should().HaveCount(1);

            state.Dequeue().Should().Be(value2);
            state.Current.Should().BeEmpty();

            Interlocked.Increment(ref controlOfExecution);
        }));

        await Task.WhenAll(tasks);

        controlOfExecution.Should().Be(10);
    }

    /// <summary>
    /// Helper class which ensures that the instance of <see cref="AsyncLocal{T}"/> is properly assigned and updated.
    /// </summary>
    private class ImplicitStateDemo<T>
    {
        private readonly AsyncLocal<ImmutableQueue<T>> _state = new();

        public ImmutableQueue<T> Current => _state.Value ?? ImmutableQueue<T>.Empty;

        public void Enqueue(T value)
        {
            _state.Value = Current.Enqueue(value);
        }

        public T Dequeue()
        {
            _state.Value = Current.Dequeue(out var value);
            return value;
        }
    }
}