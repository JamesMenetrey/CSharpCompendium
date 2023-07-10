using System.Diagnostics;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter13Scheduling;

[TestClass]
public class Recipe13P3ThrottlingCpuBoundWithTaskScheduler
{
    /// <summary>
    /// The task scheduler <see cref="ConcurrentExclusiveSchedulerPair" /> can be used to throttle the execution of
    /// async code. Note that this kind of throttling only throttles code whjile it is executing,. It's quite different
    /// than the kind of logical throttling covered in <see cref="Chapter12Synchronization.Chapter12P5Throttling" />.
    /// In particular, async code is not considered to be executing while it is awaiting an operation.
    /// Tis scheduler throttles executing code; other throttling, such as <see cref="SemaphoreSlim" />, throttles at a
    /// higher level (i.e., an entire async method).
    /// </summary>
    [TestMethod]
    public void ThrottleCpuBoundComputationUsingTaskScheduler()
    {
        var throttledScheduler = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, 2);
        var array = new int[1000];
        var options = new ParallelOptions { TaskScheduler = throttledScheduler.ConcurrentScheduler };

        var limitedStopWatch = new Stopwatch();
        limitedStopWatch.Start();
        Parallel.For(0, array.Length, options, index => ++array[index]);
        limitedStopWatch.Stop();

        array.Sum().Should().Be(1000);

        var fullStopWatch = new Stopwatch();
        fullStopWatch.Start();
        Parallel.For(0, array.Length, index => ++array[index]);
        fullStopWatch.Stop();

        array.Sum().Should().Be(2000);

        if (Environment.ProcessorCount > 2) fullStopWatch.Elapsed.Should().BeLessThan(limitedStopWatch.Elapsed);
    }
}