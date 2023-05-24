using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics;

[TestClass]
public class Recipe2P1Timeout
{
    [TestMethod]
    public async Task TimeoutWithDelay()
    {
        var timeout = Task.Delay(50);
        var task = LongRunningTasks();

        var firstFinishedTask = await Task.WhenAny(task, timeout);
        firstFinishedTask.Should().Be(timeout);
    }

    [TestMethod]
    public async Task TimeoutWithCancellationToken()
    {
        var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var task = LongRunningTasks();
        var timeout = Task.Delay(Timeout.InfiniteTimeSpan, cancelTokenSource.Token);

        var firstFinishedTask = await Task.WhenAny(task, timeout);
        firstFinishedTask.Should().Be(timeout);
    }

    private Task LongRunningTasks()
    {
        return Task.Delay(5000);
    }
}