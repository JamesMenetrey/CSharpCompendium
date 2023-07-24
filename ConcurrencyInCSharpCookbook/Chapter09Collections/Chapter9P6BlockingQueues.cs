using System.Collections.Concurrent;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter09Collections;

[TestClass]
public class Chapter9P6BlockingQueues
{
    /// <summary>
    /// You can also have a different collection implementation (e.g., <see cref="ConcurrentBag{T}"/> or
    /// <see cref="ConcurrentStack{T}"/> by passing such instance in the constructor of
    /// <see cref="BlockingCollection{T}"/>.
    /// </summary>
    [TestMethod]
    public async Task UsingBlockingQueueAsProducerConsumerScenario()
    {
        var queue = new BlockingCollection<int>();

        // Start the consumer and producer threads on the thread pool.
        var consumerTask = Task.Run(() => Consumer(queue));
        var producerTask = Task.Run(() => Producer(queue));

        await Task.WhenAll(consumerTask, producerTask);
    }

    private void Consumer(BlockingCollection<int> queue)
    {
        var list = new List<int>();

        // Wait for the producer to create some numbers.
        foreach (var entry in queue.GetConsumingEnumerable())
        {
            list.Add(entry);
        }

        list.Should().Equal(1, 2, 3, 4);
    }

    private void Producer(BlockingCollection<int> queue)
    {
        // Produce some numbers.
        for (var i = 1; i < 5; i++)
        {
            queue.Add(i);
        }

        // Mark the generation of numbers as done.
        queue.CompleteAdding();
    }
}