using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter4ParallelBasics;

[TestClass]
public class Recipe4P3ParallelInvocation
{
    /// <summary>
    /// Invoke logic that are independent from each other.
    /// </summary>
    [TestMethod]
    public void InvokeLogicInParallel()
    {
        var array = new int[1000];

        // Determine how much delegates we can run based on the number of cores
        var numberOfDelegates = Environment.ProcessorCount;
        var numberOfItemsHandledByDelegate = array.Length / numberOfDelegates;
        var remainder = array.Length % numberOfDelegates;
        
        // Generate the delegates
        var actions = new Action[numberOfDelegates];
        for (var i = 0; i < numberOfDelegates; i++)
        {
            var startIndex = i * numberOfItemsHandledByDelegate + Math.Min(i, remainder);
            var endIndex = startIndex + numberOfItemsHandledByDelegate + (i < remainder ? 1 : 0);

            actions[i] = () => IncrementEveryCell(new Span<int>(array, startIndex, endIndex - startIndex));
        }

        // Run the delegates in parallel
        Parallel.Invoke(actions);

        array.Sum().Should().Be(1000);
    }

    private void IncrementEveryCell(Span<int> span)
    {
        for (var i = 0; i < span.Length; i++)
        {
            span[i] += 1;
        }
    }
}
