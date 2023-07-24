using System.Collections.Concurrent;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter13Scheduling
{
    [TestClass]
    public class Recipe13P2SynchronizationContext
    {
        /// <summary>
        /// A thread-bound variable to identify where the logic is executed.
        /// The value is either 1 for main thread, 2 for custom single thread, or 0 otherwise (for the thread pool).
        /// </summary>
        [ThreadStatic]
        private static int _localThreadProof;

        /// <summary>
        /// This unit test demonstrates that the `await` keyword captures the current synchronization context (of the thread),
        /// and uses the <see cref="SynchronizationContext.Post"/> method to dispatch the remaining of the method.
        ///
        /// Note that the tasks are created in the current task scheduler (<see cref="TaskScheduler.Default"/>), and by default,
        /// they are started on the thread pool.
        /// </summary>
        [TestMethod]
        public async Task DemonstrateHowSynchronizationContextInteractsWithAwait()
        {
            var cancellation = new CancellationTokenSource();
            // Set up the custom synchronization context of the thread, so when `await` is over, the remaining of the method
            // is resumed in a single same thread.
            SynchronizationContext.SetSynchronizationContext(new SingleThreadSynchronizationContext(cancellation.Token));
            _localThreadProof = 1;

            // Execute in the main thread.
            var dump1 = Dump();
            // The task is executed in the thread pool, due to the default task scheduler.
            var task1 = DumpAsync();
            // Execute in the main thread, as the previous call did not await.
            var dump2 = Dump();
            var dump3 = await task1;

            // Execute in the custom thread, because `await` dispatched the continuation of the method using our custom
            // synchronization context.
            var dump4 = Dump();
            // The task is executed in the thread pool, due to the default task scheduler.
            var task2 = DumpAsync();
            // Execute in the custom thread, as the previous call did not await.
            var dump5 = Dump();
            var dump6 = await task2;

            // Execute in the custom thread, because `await` dispatched the continuation of the method using our custom
            // synchronization context.
            var dump7 = Dump();
            // The task is executed in the thread pool, due to the default task scheduler.
            var task3 = DumpAsync();
            // Execute in the custom thread, as the previous call did not await.
            var dump8 = Dump();
            var dump9 = await task3;
            
            // Execute in the custom thread, because `await` dispatched the continuation of the method using our custom
            // synchronization context.
            var dump10 = Dump();
            
            cancellation.Cancel();

            // Assert
            dump1.Proof.Should().Be(dump2.Proof).And.Be(1, "At this stage, we are in the main thread.");
            dump4.Should().BeEquivalentTo(dump5).And.BeEquivalentTo(dump7).And.BeEquivalentTo(dump8).And.BeEquivalentTo(dump10, 
                "These instructions are executed by the custom synchronization context and shares the same thread.");
            dump3.Proof.Should().Be(dump6.Proof).And.Be(dump9.Proof).And.Be(0, 
                "These tasks are executed by the default task scheduler and therefore in the thread pool.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        // Same tracing methods to prove that our logic works.
        ThreadDumpedInfo Dump() => new (Environment.CurrentManagedThreadId, _localThreadProof);
        Task<ThreadDumpedInfo> DumpAsync()
        {
            return Task.Run(async () =>
            {
                var dump = new ThreadDumpedInfo(Environment.CurrentManagedThreadId, _localThreadProof);
                await Task.Delay(10);
                return dump;
            });
        }

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
