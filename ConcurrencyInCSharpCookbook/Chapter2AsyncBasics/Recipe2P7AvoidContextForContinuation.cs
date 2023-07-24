using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics
{
    [TestClass]
    public class Recipe2P7AvoidContextForContinuation
    {
        [TestMethod]
        public async Task AvoidContextForContinuation()
        {
            var context = new MonitoredSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);

            // The current context is NOT captured by the `await` keyword and therefore,
            // the continuation is run in the thread pool.
            await Task.Run(() => Task.CompletedTask).ConfigureAwait(continueOnCapturedContext: false);

            context.HasBeenCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task KeepContextForContinuationAsDefaultBehavior()
        {
            var context = new MonitoredSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);

            // The current context is captured by the `await` keyword.
            await Task.Run(async () => await Task.Delay(10));

            context.HasBeenCalled.Should().BeTrue();
        }

        [TestCleanup]
        public void Cleanup()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        private class MonitoredSynchronizationContext : SynchronizationContext
        {
            public bool HasBeenCalled { get; private set; }

            public override void Post(SendOrPostCallback d, object? state)
            {
                HasBeenCalled = true;
                base.Post(d, state);
            }

            public override void Send(SendOrPostCallback d, object? state)
            {
                HasBeenCalled = true;
                base.Send(d, state);
            }
        }
    }
}
