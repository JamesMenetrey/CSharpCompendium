using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter08Interop;

[TestClass]
public class Recipe8P4AsyncWrapperForParallel
{
    /// <summary>
    /// By using <see cref="Task.Run(System.Action)"/>, all the parallel processing is pushed off to the thread pool.
    /// The returning <see cref="Task"/> can then be awaited by the calling thread, which can be the UI.
    /// </summary>
    [TestMethod]
    public async Task WrapParallelCode()
    {
        var array = new int[1000];

        await Task.Run(() => ParallelCodIncrement(array));

        array.Sum().Should().Be(1000);
    }

    private void ParallelCodIncrement(int[] array)
    {
        Parallel.For(0, array.Length, index => array[index]++);
    }
}