using Nito.AsyncEx;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics
{
    [TestClass]
    public class Recipe2P9HandlingExceptionInAsyncVoid
    {

        [TestMethod]
        public void WorkAroundTwoWithCustomSynchronizationContext()
        {
            try
            {
                // The AsyncEx library provides the type AsyncContext, which has its own SynchronizationContext,
                // TaskScheduler and TaskFactory, which executes synchronously the asynchronous delegate, and
                // bubble up any occurring exception.
                // The scheduled delegate is executed on a thread pool thread.
                AsyncContext.Run(ThrowVoidAsync);
                Assert.Fail();
            }
            catch (ExpectedException _)
            {
                return;
            }

            Assert.Fail();
        }

        private async void ThrowVoidAsync()
        {
            await Task.Yield();
            throw new ExpectedException();
        }

        private class ExpectedException : Exception { }
    }
}
