using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter3AsyncStreams
{
    [TestClass]
    public class Recipe3P3AsyncLinq
    {
        /// <summary>
        /// The usual LINQ methods are compatible with streams, as long as the predicate is synchronous.
        /// For async predicates, check out <see cref="ConsumeAsyncLinqWithAsyncPredicate"/>.
        /// </summary>
        [TestMethod]
        public async Task ConsumeAsyncLinqWithSyncPredicate()
        {
            var values = MultipleOfAsync(3, 10).Where(x => x % 2 == 0);

            (await values.ToListAsync()).Should().Equal(6);
        }

        /// <summary>
        /// We need to use the NuGet package "System.Linq.Async" to use async predicates on streams.
        /// </summary>
        [TestMethod]
        public async Task ConsumeAsyncLinqWithAsyncPredicate()
        {
            var values = MultipleOfAsync(3, 10).WhereAwait(PredicateAsync);

            (await values.ToListAsync()).Should().Equal(6);
        }

        private async IAsyncEnumerable<int> MultipleOfAsync(int n, int cap)
        {
            // May pull some resources on async I/O and buffer some elements to return.
            // The following is a synthetic example.
            int currentValue = n;

            while (currentValue < cap)
            {
                yield return currentValue;
                currentValue += n;
                await Task.Yield();
            }
        }

        private async ValueTask<bool> PredicateAsync(int x)
        {
            await Task.Yield();
            return x % 2 == 0;
        }
    }
}
