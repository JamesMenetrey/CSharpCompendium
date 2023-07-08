using FluentAssertions;
using Nito.AsyncEx;

namespace ConcurrencyInCSharpCookbook.Chapter11FunctionalStyle;

[TestClass]
public class Chapter11P4LazyProperties
{
    /// <summary>
    /// The type <see cref="AsyncLazy{T}"/> allows to initialize values lazily. Such values can be set for properties.
    /// </summary>
    [TestMethod]
    public async Task UsingAsyncProperty()
    {
        var foo = new Foo();
        var value1 = await foo.DataAsync;
        var value2 = await foo.DataAsync;

        value1.Should().Be(42);
        value2.Should().Be(42);
        foo.CounterForCallingDataAsync.Should().Be(1);
    }

    private class Foo
    {
        private int _counterForCallingDataAsync = 0;

        public Foo()
        {
            DataAsync = new AsyncLazy<int>(async () =>
            {
                await Task.Yield();
                Interlocked.Increment(ref _counterForCallingDataAsync);
                return 42;
            });
        }

        public int CounterForCallingDataAsync => _counterForCallingDataAsync;
        public AsyncLazy<int> DataAsync { get; init; }
    }
}