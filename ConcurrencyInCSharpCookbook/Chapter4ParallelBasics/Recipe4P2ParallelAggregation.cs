using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter4ParallelBasics;

[TestClass]
public class Recipe4P2ParallelAggregation
{
    [TestMethod]
    public void AggregateUsingParallel()
    {
        var values = Enumerable.Repeat(1, 1000);
        var result = 0;

        // The localInit initializes a local value to the executing thread.
        // Once every dedicated threads finished to compute their local value, they all
        // execute the localFinally logic, which needs to support concurrent execution.
        Parallel.ForEach(
            source: values,
            localInit: () => 0,
            body: (item, _, localValue) => item + localValue,
            localFinally: localValue => Interlocked.Add(ref result, localValue));

        result.Should().Be(1000);
    }

    /// <summary>
    /// Using the PLINQ extension method <see cref="ParallelEnumerable.AsParallel"/> transforms
    /// the regular LINQ expressions into parallel ones.
    /// Note that PLINQ is more friendly to other processes on the system than PLINQ.
    /// </summary>
    [TestMethod]
    public void AggregateUsingPLinq()
    {
        var result = Enumerable.Repeat(1, 1000).AsParallel().Sum();
        result.Should().Be(1000);
    }

    [TestMethod]
    public void AggregateUsingPLinqFullDefinition()
    {
        var values = Enumerable.Repeat(1, 1000);
        var result = values.AsParallel().Aggregate(
            seed: 0,
            func: (accumulator, item) => accumulator + item);

        result.Should().Be(1000);
    }
}
