using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics
{
    [TestClass]
    public class Recipe2P8HandleExceptionInAsyncTask
    {
        /// <summary>
        /// A try-catch clause handles seamlessly the asynchronous exception.
        /// </summary>
        [TestMethod]
        public async Task HandleExceptionForOneAwait()
        {
            // The exception is thrown and stored in the task, waiting to be observed when awaited.
            var task = ThrowAsync();

            try
            {
                // The exception is bubbled out here, because of the await keyword.
                await task;
                Assert.Fail("Must not continue after an exception is thrown.");
            }
            catch (ExpectedException _)
            {
                return;
            }

            Assert.Fail("The exception has not been caught.");
        }

        private Task ThrowAsync() => Task.Run(() => throw new ExpectedException());

        private class ExpectedException : Exception {}
    }
}
