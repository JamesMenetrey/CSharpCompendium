using FluentAssertions;
using System.Collections.Concurrent;

namespace ConcurrencyInCSharpCookbook.Chapter13Scheduling
{
    [TestClass]
    public class Recipe13P2TaskScheduler
    {
        [ThreadStatic]
        private static int _localThreadProof;

        /// <summary>
        /// This unit test demonstrates how we can use a custom task scheduler to execute tasks in a single
        /// separated thread.
        /// </summary>
        [TestMethod]
        public async Task DemonstrateHowCustomTaskSchedulerWorks()
        {
            var cancellation = new CancellationTokenSource();
            var taskScheduler = new MyTaskScheduler(cancellation.Token);
            var taskFactory = new TaskFactory(taskScheduler);
            _localThreadProof = 1;

            // Execute in the main thread.
            var dump1 = Dump();
            // The task is scheduled in the custom task scheduler, in a single thread.
            var task1 = taskFactory.StartNew(Dump);
            // Execute in the main thread.
            var dump2 = Dump();
            var dump3 = await task1;

            // Execute in the thread pool, because `await` dispatched the continuation of the method using the
            // default synchronization context.
            var dump4 = Dump();
            // The task is scheduled in the custom task scheduler, in a single thread.
            var task2 = taskFactory.StartNew(Dump);
            // Execute in the thread pool, because `await` dispatched the continuation of the method using the
            // default synchronization context.
            var dump5 = Dump();
            var dump6 = await task2;

            // Execute in the thread pool, because `await` dispatched the continuation of the method using the
            // default synchronization context.
            var dump7 = Dump();
            // The task is scheduled in the custom task scheduler, in a single thread.
            var task3 = taskFactory.StartNew(Dump);
            // Execute in the thread pool, because `await` dispatched the continuation of the method using the
            // default synchronization context.
            var dump8 = Dump();
            var dump9 = await task3;

            // Execute in the thread pool, because `await` dispatched the continuation of the method using the
            // default synchronization context.
            var dump10 = Dump();

            cancellation.Cancel();

            // Assert
            dump1.Proof.Should().Be(dump2.Proof).And.Be(1, "At this stage, we are in the main thread.");
            dump3.Should().BeEquivalentTo(dump6).And.BeEquivalentTo(dump9,
                "These tasks are executed by custom task scheduler and therefore in the single thread.");
            dump4.Proof.Should().Be(dump5.Proof).And.Be(dump7.Proof).And.Be(dump8.Proof).And.Be(dump10.Proof).And.Be(0,
                "These instructions are executed by the synchronization context and therefore in the thread pool.");
        }

        // Same tracing methods to prove that our logic works.
        ThreadDumpedInfo Dump() => new(Environment.CurrentManagedThreadId, _localThreadProof);

        // ReSharper disable once NotAccessedPositionalProperty.Local
        record ThreadDumpedInfo(int ThreadId, int Proof);

        internal class MyTaskScheduler : TaskScheduler
        {
            private readonly CancellationToken _cancellationToken;
            private readonly ConcurrentQueue<Task> _queue = new();

            public MyTaskScheduler(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
                var thread = new Thread(MySingleThread)
                {
                    IsBackground = true
                };
                thread.Start();
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return _queue.AsEnumerable();
            }

            protected override void QueueTask(Task task)
            {
                _queue.Enqueue(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (taskWasPreviouslyQueued) return false;

                return TryExecuteTask(task);
            }

            private void MySingleThread()
            {
                _localThreadProof = 2;
                while (!_cancellationToken.IsCancellationRequested)
                {
                    if (_queue.TryDequeue(out var task))
                    {
                        TryExecuteTask(task);
                    }

                    Thread.Yield();
                }
            }
        }
    }
}
