using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter4ParallelBasics;

[TestClass]
public class Recipe4P1ProcessingOfData
{
    [TestMethod]
    public void ParallelProcessing()
    {
        var values = new int[1000];
        values.Sum().Should().Be(0);

        Parallel.ForEach(values, (value, _, index) =>
        {
            values[index] = value + 1;
        });

        values.Sum().Should().Be(1000);
    }

    /// <summary>
    /// You can interrupt the execution of parallel work using the method <see cref="ParallelLoopState.Break"/> or
    /// <see cref="ParallelLoopState.Stop"/>, depending on the cancellation behavior.
    /// </summary>
    [TestMethod]
    public void StopParallelProcessing()
    {
        var values = new int[1000];
        var numberOfIterations = 0;
        values.Sum().Should().Be(0);

        Parallel.ForEach(values, (value, state, index) =>
        {
            Interlocked.Increment(ref numberOfIterations);

            if (index >= values.Length / 2)
            {
                state.Break();
                return;
            }

            values[index] = value + 1;
        });

        values.Sum().Should().Be(values.Length / 2);

        // We may have already cores processing further number than the exact half of the array.
        numberOfIterations.Should().BeCloseTo(values.Length / 2, (uint)Environment.ProcessorCount);
    }

    /// <summary>
    /// You can request a cancellation using a <see cref="CancellationToken"/>, but the interruption is much more imprecise
    /// compared to an in-loop interruption. Note that the token can be used in the loop for an in-loop cancellation if needed.
    /// </summary>
    [TestMethod]
    public void CancelParallelProcessing()
    {
        var values = new int[1000];
        var numberOfIterations = 0;
        var tokenSource = new CancellationTokenSource();
        var cancelled = false;
        values.Sum().Should().Be(0);

        try
        {
            Parallel.ForEach(values, new ParallelOptions { CancellationToken = tokenSource.Token }, (value, _, index) =>
            {
                Interlocked.Increment(ref numberOfIterations);

                // This can be called from the outside of the loop.
                if (index >= values.Length / 2)
                {
                    tokenSource.Cancel();
                    return;
                }

                values[index] = value + 1;
            });
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }

        cancelled.Should().BeTrue();
        values.Sum().Should().NotBe(values.Length);
    }
}
