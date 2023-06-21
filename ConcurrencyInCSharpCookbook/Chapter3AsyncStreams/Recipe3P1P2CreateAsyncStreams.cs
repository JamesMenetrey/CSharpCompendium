using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter3AsyncStreams
{
    [TestClass]
    public class Recipe3P1P2CreateAsyncStreams
    {
        /// <summary>
        /// An asynchronous stream function may use <c>async</c>, <c>await</c>, and <c>yield</c> instructions.
        /// The caller consumes these values using <c>await foreach</c> instruction.
        /// </summary>
        [TestMethod]
        public async Task ReadAsyncStream()
        {
            var multiples = new List<int>();

            await foreach (var n in MultipleOfAsync(3, 10))
            {
                multiples.Add(n);
            }

            multiples.Should().Equal(3, 6, 9);
        }

        [TestMethod]
        public async Task ReadAsyncStreamWithoutContextCapture()
        {
            var multiples = new List<int>();

            // The returned task after the "in" keyword can be configured as a regular task.
            await foreach (var n in MultipleOfAsync(3, 10).ConfigureAwait(false))
            {
                multiples.Add(n);
                // The body of the loop may be async as well.
                await Task.Delay(0).ConfigureAwait(false);
            }

            multiples.Should().Equal(3, 6, 9);
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
    }
}
