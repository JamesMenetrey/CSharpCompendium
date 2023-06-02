using System.Threading.Tasks.Sources;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics;

/// <summary>
/// A <see cref="ValueTask"/> can be awaited or its result read once, because its underlying
/// <see cref="IValueTaskSource"/> can be reused for further usages.
/// </summary>
[TestClass]
public class Recipe2P11ConsumingValueTasks
{

    /// <summary>
    /// Getting the result of an <see cref="ValueTask{TResult}"/> when the operation hasn't yet completed is unsafe,
    /// because the <see cref="IValueTaskSource"/> / <see cref="IValueTaskSource{TResult}"/> implementation need not
    /// support blocking until the operation completes, and likely doesn't, so such an operation is inherently a race
    /// condition and is unlikely to behave the way the caller intends.
    /// In contrast, <see cref="Task"/> / <see cref="Task{Result}"/> do enable this, blocking the caller until
    /// the task completes.
    /// </summary>
    [TestMethod]
    public async Task ValueTaskDoesNotBlockOnResult()
    {
        var delay = new Recipe2P10CreateValueTasks.DelayOperation();
        
        ValueTask<int> GetValueAfterMilliseconds(int value)
        {
            return delay.DelayAsync(TimeSpan.FromMilliseconds(50), value);
        }
        
        const int expectedValue = 42;
        var task = GetValueAfterMilliseconds(expectedValue);
        task.Invoking(t => t.GetAwaiter().GetResult()).Should().Throw<InvalidOperationException>();

        var result = await task;
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// This is not safe to await a <see cref="ValueTask"/> more than once, because the underlying object may have been
    /// recycled already and be in use by another operation. In contrast, a <see cref="Task"/> / <see cref="Task{T}"/>
    /// will never transition from a complete to incomplete state, so you can await it as many times as you need to,
    /// and will always get the same answer every time.
    /// </summary>
    [TestMethod]
    public async Task ValueTaskMustNotBeAwaitedMoreThanOnce()
    {
        var delay = new Recipe2P10CreateValueTasks.DelayOperation();
        
        ValueTask<int> GetValueAlmostInstantly(int value)
        {
            return delay.DelayAsync(TimeSpan.FromMilliseconds(1), value);
        }

        var task1 = GetValueAlmostInstantly(1);
        var normalAwaitOfValueTask1 = await task1;
        delay.Reset();
        var task2 = GetValueAlmostInstantly(2);
        var normalAwaitOfValueTask2 = await task2;

        normalAwaitOfValueTask1.Should().Be(1);
        normalAwaitOfValueTask2.Should().Be(2);
        // The source of the ValueTask has been reused to serve task2, so this is not safe to await more than once.
        await this.Invoking(async _ => await task1).Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task ValueTaskCanBeConvertedIntoTask()
    {
        var delay = new Recipe2P10CreateValueTasks.DelayOperation();
        
        ValueTask<int> GetValueAlmostInstantly(int value)
        {
            return delay.DelayAsync(TimeSpan.FromMilliseconds(1), value);
        }

        // A ValueTask can be converted into a regular task to workaround the limitations, but this will allocate
        // the task on the heap.
        var task1 = GetValueAlmostInstantly(1).AsTask();
        var normalAwaitOfValueTask1 = await task1;
        delay.Reset();
        var task2 = GetValueAlmostInstantly(2).AsTask();
        var normalAwaitOfValueTask2 = await task2;

        normalAwaitOfValueTask1.Should().Be(1);
        normalAwaitOfValueTask2.Should().Be(2);

        (await task1).Should().Be(1);
    }
}