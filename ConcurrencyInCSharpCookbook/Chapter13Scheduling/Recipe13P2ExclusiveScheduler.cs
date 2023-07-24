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
        Console.WriteLine($">> {Environment.ProcessorCount}");

        var scheduler = new ConcurrentExclusiveSchedulerPair();
        var concurrent = scheduler.ConcurrentScheduler;
        var exclusive = scheduler.ExclusiveScheduler;

        await MustExecuteConcurrently(new TaskFactory(concurrent));
        await MustExecuteSequentially(new TaskFactory(exclusive));
    }

    private async Task MustExecuteConcurrently(TaskFactory factory)
    {
        var signal = new ManualResetEventSlim();

        var task1 = factory.StartNew(() =>
        {
            var wasSet = signal.Wait(1000);
            wasSet.Should().BeTrue();
        });
        
        var task2 = factory.StartNew(() =>
        {
            signal.Set();
        });

        await Task.WhenAll(task1, task2);
    }
    
    private async Task MustExecuteSequentially(TaskFactory factory)
    {
        var signal = new ManualResetEventSlim();

        var task1 = factory.StartNew(() =>
        {
            var wasSet = signal.Wait(1000);
            wasSet.Should().BeFalse();
        });
        
        var task2 = factory.StartNew(() =>
        {
            signal.Set();
        });

        await Task.WhenAll(task1, task2);
    }
}