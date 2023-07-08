using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter12Synchronization;

/// <summary>
/// Throttling may be necessary when you find your code is using too many resources (e.g., CPU, memory), or stressing
/// a dependency too hard.
/// </summary>
[TestClass]
public class Chapter12P5Throttling
{
    [TestMethod]
    public void ThrottlingPLinq()
    {
        var result = Enumerable.Repeat(0, 1000)
            .AsParallel()
            .WithDegreeOfParallelism(4)
            .Select(value => ++value);

        result.Sum().Should().Be(1000);
    }

    [TestMethod]
    public void ThrottlingParallel()
    {
        var array = new int[1000];
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4
        };

        Parallel.For(0, array.Length, options, (index, _) => array[index]++);

        array.Sum().Should().Be(1000);
    }

    [TestMethod]
    public async Task ThrottlingConcurrentAsyncCode()
    {
        var throttler = new SemaphoreSlim(4);
        var tasks = Enumerable.Repeat(0, 1000).Select(async value =>
        {
            await throttler.WaitAsync();

            try
            {
                return ++value;
            }
            finally
            {
                throttler.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        tasks.Select(t => t.Result).Sum().Should().Be(1000);
    }
}