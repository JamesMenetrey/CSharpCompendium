using FluentAssertions;
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

        /// <summary>
        /// While asynchronous functions returning a <see cref="Task"/> stores the exception in that type, asynchronous
        /// <c>void</c> functions throws the exception in the instance of the <see cref="SynchronizationContext"/> which
        /// initially called the asynchronous function. This use case demonstrates how it can be done.
        /// </summary>
        /// /// <seealso href="https://devblogs.microsoft.com/dotnet/how-async-await-really-works/#synchronizationcontext-and-configureawait">
        /// The explanations of asynchronous exception handling for both async <see cref="Task"/> and <c>void</c>.
        /// </seealso>
        [TestMethod]
        public async Task SynchronizationContextMayTrackAsyncVoid()
        {
            var context = new VoidAsyncTrackingSynchronizationContext();
            var previousContext = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                context.NumberOfStartedOperations.Should().Be(0);
                context.NumberOfCompletedOperations.Should().Be(0);

                ThrowVoidAsync();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }

            await Task.Delay(10);

            context.NumberOfStartedOperations.Should().Be(1);
            context.NumberOfCompletedOperations.Should().Be(1);
            context.ThrownException.Should().NotBeNull().And.BeAssignableTo<ExpectedException>();
        }

        private class VoidAsyncTrackingSynchronizationContext : SynchronizationContext
        {
            public int NumberOfStartedOperations { get; private set; } = 0;
            public int NumberOfCompletedOperations { get; private set; } = 0;
            public Exception? ThrownException { get; private set; }

            public override void OperationStarted()
            {
                NumberOfStartedOperations++;

                base.OperationStarted();
            }

            public override void OperationCompleted()
            {
                NumberOfCompletedOperations++;

                base.OperationCompleted();
            }

            public override void Post(SendOrPostCallback d, object? state)
            {
                base.Post(WrapExceptionHandling(d), state);
            }

            private SendOrPostCallback WrapExceptionHandling(SendOrPostCallback d)
            {
                return state =>
                {
                    try
                    {
                        d(state);
                    }
                    catch (Exception e)
                    {
                        ThrownException = e;
                    }
                };
            }
        }
    }
}
