using System.Collections.Concurrent;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter13Scheduling
{
    [TestClass]
    public class Recipe13P2TaskSchedulerFromSynchronizationContext
    {
        /// <summary>
        /// A thread-bound variable to identify where the logic is executed.
        /// The value is either 1 for main thread, 2 for custom single thread, or 0 otherwise (for the thread pool).
        /// </summary>
        [ThreadStatic]
        private static int _localThreadProof;

        [TestMethod]
        public async Task DispatchScheduledTasksUsingTheSynchronizationContext()
        {
            var cancellation = new CancellationTokenSource();
            // Set up the custom synchronization context of the thread, so when `await` is over, the remaining of the method
            // is resumed in a single same thread.
            SynchronizationContext.SetSynchronizationContext(new SingleThreadSynchronizationContext(cancellation.Token));
            // Create a custom task scheduler and a factory to schedule tasks.
            // This task scheduler schedules the tasks directly to our custom synchronization context.
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskFactory = new TaskFactory(taskScheduler);
            _localThreadProof = 1;

            // Execute in the main thread.
            var dump1 = Dump();
            // The task is scheduled in the custom task scheduler, which is executed by our custom synchronization context, in a single thread.
            var task1 = taskFactory.StartNew(Dump);
            // Execute in the main thread.
            var dump2 = Dump();
            var dump3 = await task1;

            // Execute in the custom, single thread of the synchronization context, because `await` dispatched the continuation of the method using the
            // custom synchronization context.
            var dump4 = Dump();
            // The task is scheduled in the custom task scheduler, which is executed by our custom synchronization context, in a single thread.
            var task2 = taskFactory.StartNew(Dump);
            // Execute in the custom, single thread of the synchronization context, because `await` dispatched the continuation of the method using the
            // custom synchronization context.
            var dump5 = Dump();
            var dump6 = await task2;

            // Execute in the custom, single thread of the synchronization context, because `await` dispatched the continuation of the method using the
            // custom synchronization context.
            var dump7 = Dump();
            // The task is scheduled in the custom task scheduler, which is executed by our custom synchronization context, in a single thread.
            var task3 = taskFactory.StartNew(Dump);
            // Execute in the custom, single thread of the synchronization context, because `await` dispatched the continuation of the method using the
            // custom synchronization context.
            var dump8 = Dump();
            var dump9 = await task3;

            // Execute in the custom, single thread of the synchronization context, because `await` dispatched the continuation of the method using the
            // custom synchronization context.
            var dump10 = Dump();

            cancellation.Cancel();

            // Assert
            dump1.Proof.Should().Be(dump2.Proof).And.Be(1, "At this stage, we are in the main thread.");
            dump3.Should().BeEquivalentTo(dump4).And.BeEquivalentTo(dump5).And.BeEquivalentTo(dump6).And.BeEquivalentTo(dump7)
                .And.BeEquivalentTo(dump8).And.BeEquivalentTo(dump9);
        }

        [TestCleanup]
        public void Cleanup()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        // Same tracing methods to prove that our logic works.
        ThreadDumpedInfo Dump() => new(Environment.CurrentManagedThreadId, _localThreadProof);

        // ReSharper disable once NotAccessedPositionalProperty.Local
        record ThreadDumpedInfo(int ThreadId, int Proof);

        /// <summary>
        /// A custom synchronization context. It dispatches delegates into a separated thread.
        /// </summary>
        class SingleThreadSynchronizationContext : SynchronizationContext
        {
            private readonly CancellationToken _cancellationToken;
            private readonly ConcurrentQueue<(SendOrPostCallback, object?)> _queue = new();

            public SingleThreadSynchronizationContext(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
                var thread = new Thread(MySingleThread)
                {
                    IsBackground = true
                };
                thread.Start();
            }

            /// <summary>
            /// This method is called once an awaited call is over to resume the remaining
            /// of the method.
            /// </summary>
            public override void Post(SendOrPostCallback d, object? state)
            {
                //Console.WriteLine("=> Post");
                _queue.Enqueue((d, state));
            }

            public override void Send(SendOrPostCallback d, object? state)
            {
                //Console.WriteLine("=> Send");
                _queue.Enqueue((d, state));
            }

            /// <summary>
            /// Our simple and dumb thread that executes the delegates posted by the `await` keyword.
            /// </summary>
            private void MySingleThread()
            {
                // Set the synchronization context for our custom thread, so we still dispatch the continuation
                // of the awaited method in this pump.
                SynchronizationContext.SetSynchronizationContext(this);
                _localThreadProof = 2;
                while (!_cancellationToken.IsCancellationRequested)
                {
                    if (_queue.TryDequeue(out (SendOrPostCallback d, object? state) continuation))
                    {
                        continuation.d(continuation.state);
                    }

                    Thread.Yield();
                }
            }
        }
    }
}