using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter14Scenarios;

/// <summary>
/// There is no perfect solution. Calling asynchronous code from synchronous may deadlock, while calling synchronous
/// code from asynchronous is not efficient.
/// One potential solution is called the Boolean Argument Hack, which instructs to have a private core function that
/// contains the logic, and two other methods to expose an synchronous and an asynchronous API, reusing the code function.
/// The private core function has an argument stating whether it must be executed synchronously.
/// </summary>
[TestClass]
public class Recipe14P5IdenticalSyncAndAsyncCode
{
    [TestMethod]
    public async Task CallAsynchronously()
    {
        var demo = new SyncAndAsyncDemo();
        (await demo.DelayAndReturnValueAsync(42)).Should().Be(42);
    }
    
    [TestMethod]
    public void CallSynchronously()
    {
        var demo = new SyncAndAsyncDemo();
        demo.DelayAndReturnValue(42).Should().Be(42);
    }

    private class SyncAndAsyncDemo
    {
        /// <summary>
        /// The synchronous API, which instructs the core function to act synchronously (using the boolean parameter).
        /// </summary>
        public int DelayAndReturnValue(int value)
        {
            var normallyCompletedTask = DelayAndReturnValueCore(value, true);
            Assert.IsTrue(normallyCompletedTask.IsCompleted);

            // We can safely using the GetAwaiter and GetResult without a deadlock occurring, because the task has
            // already completed. Therefore, a deadlock cannot occur.
            return normallyCompletedTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// The asynchronous API, which allows the core function to behave asynchronously (using the boolean parameter).
        /// </summary>
        public Task<int> DelayAndReturnValueAsync(int value)
        {
            return DelayAndReturnValueCore(value, false);
        }

        /// <summary>
        /// The core function, with the code that is shared between the async and sync API.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sync">Indicates whether the function must complete synchronously.</param>
        /// <remarks>
        /// When the boolean parameter is set to <c>true</c>, the function must absolutely execute synchronously, as
        /// the program may deadlock otherwise. The completeness of the task is checked when using the public sync API.
        /// </remarks>
        private async Task<int> DelayAndReturnValueCore(int value, bool sync)
        {
            // Some operations before for asynchronous code...
            var timeToWait = value % 10;

            // The synchronous call if requested.
            if (sync)
                // We use the synchronous API for awaiting.
                Thread.Sleep(timeToWait);
            else
                // We use the asynchronous API for awaiting.
                await Task.Delay(timeToWait);

            return value;
        }
    }
}