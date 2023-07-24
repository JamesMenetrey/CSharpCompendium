using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter13Scheduling;

[TestClass]
public class Recipe13P2ExclusiveScheduler
{
    /// <summary>
    /// The task scheduler <see cref="ConcurrentExclusiveSchedulerPair" /> includes two <see cref="TaskScheduler" />s.
    /// The first one, <see cref="ConcurrentExclusiveSchedulerPair.ConcurrentScheduler" />, enables to execute tasks
    /// concurrently, as long as <see cref="ConcurrentExclusiveSchedulerPair.ExclusiveScheduler" /> is not executing
    /// a task. The second one, <see cref="ConcurrentExclusiveSchedulerPair.ExclusiveScheduler" />, only executes
    /// on task at a time.
    /// </summary>
    [TestMethod]
    public async Task UseConcurrentExclusiveScheduler()
    {
        // Only assess the concurrency if more than one core is available.
        if (Environment.ProcessorCount < 2) Assert.Inconclusive();

        var scheduler = new ConcurrentExclusiveSchedulerPair();
        var concurrent = scheduler.ConcurrentScheduler;
        var exclusive = scheduler.ExclusiveScheduler;

        await MustExecuteConcurrently(new TaskFactory(concurrent));
        await MustExecuteSequentially(new TaskFactory(exclusive));
    }

    private async Task MustExecuteConcurrently(TaskFactory factory)
    {
        var array1 = new int[1000];
        var array2 = new int[1000];

        var task1 = factory.StartNew(() =>
        {
            for (var i = 0; i < array1.Length; i++)
            {
                ++array1[i];
                // In the case we are in the last iteration, the tasks2 must have already started.
                if (i == array1.Length - 1)
                {
                    // Only assess the concurrency if more than one core is available.
                    if (Environment.ProcessorCount > 1)
                    {
                        array2[0].Should().BeGreaterThan(0);
                    }
                    
                    Console.WriteLine($">> {Environment.ProcessorCount}");
                }
            }
        });
        
        var task2 = factory.StartNew(() =>
        {
            for (var i = 0; i < array1.Length; i++)
            {
                ++array2[i];
            }
        });

        await Task.WhenAll(task1, task2);
    }
    
    private async Task MustExecuteSequentially(TaskFactory factory)
    {
        var array1 = new int[1000];
        var array2 = new int[1000];

        var task1 = factory.StartNew(() =>
        {
            for (var i = 0; i < array1.Length; i++)
            {
                ++array1[i];
                // In the case we are in the last iteration, the tasks2 must NOT have already started.
                if (i == array1.Length - 1)
                {
                    array2[0].Should().Be(0);
                }
            }
        });
        
        var task2 = factory.StartNew(() =>
        {
            for (var i = 0; i < array1.Length; i++)
            {
                ++array2[i];
            }
        });

        await Task.WhenAll(task1, task2);
    }
}