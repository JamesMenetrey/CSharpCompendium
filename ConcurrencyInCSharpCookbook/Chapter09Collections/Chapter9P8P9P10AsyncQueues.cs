using System.Threading.Channels;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter09Collections;

[TestClass]
public class Chapter9P8P9P10AsyncQueues
{
    [TestMethod]
    public async Task UsingAsyncQueueAsUnboundedProducerConsumer()
    {
        var queue = Channel.CreateUnbounded<int>();

        // Start the consumer and producer threads on the thread pool.
        var consumerTask = Task.Run(() => Consumer(queue.Reader));
        var producerTask = Task.Run(() => Producer(queue.Writer));

        await Task.WhenAll(consumerTask, producerTask);
        
        consumerTask.Result.Should().Equal(1, 2, 3, 4);
    }

    /// <summary>
    /// In case the producers produce data faster than consumed, the generation can be throttled using a bounded
    /// channel.
    /// </summary>
    [TestMethod]
    public async Task UsingAsyncQueueAsBoundedProducerConsumer()
    {
        var queue = Channel.CreateBounded<int>(capacity: 2);

        // Start the consumer and producer threads on the thread pool.
        var consumerTask = Task.Run(() => Consumer(queue.Reader));
        var producerTask = Task.Run(() => Producer(queue.Writer));

        await Task.WhenAll(consumerTask, producerTask);
        
        consumerTask.Result.Should().Equal(1, 2, 3, 4);
    }

    /// <summary>
    /// When back pressuring the producers, we can drop the oldest elements.
    /// </summary>
    [TestMethod]
    public async Task UsingAsyncQueueAsBoundedProducerConsumerAndDropOldestWhenFull()
    {
        var queue = Channel.CreateBounded<int>(new BoundedChannelOptions(capacity: 2)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });

        // Start and wait that the producer generates everything to test the DropOldest mode.
        await Task.Run(() => Producer(queue.Writer));
        // Consume the remaining numbers.
        var consumerResult = await Task.Run(() => Consumer(queue.Reader));
        
        consumerResult.Should().Equal(3, 4);
    }
    
    private async Task<List<int>> Consumer(ChannelReader<int> reader)
    {
        var list = new List<int>();

        // Wait for the producer to create some numbers.
        await foreach (var entry in reader.ReadAllAsync())
        {
            list.Add(entry);
        }

        return list;
    }

    private async Task Producer(ChannelWriter<int> writer)
    {
        // Produce some numbers.
        for (var i = 1; i < 5; i++)
        {
            await writer.WriteAsync(i);
        }

        // Mark the generation of numbers as done.
        writer.Complete();
    }
}