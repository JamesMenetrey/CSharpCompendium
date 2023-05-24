using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics;

[TestClass]
public class Recipe2P2ReturnCompleteTask
{
    [TestMethod]
    public async Task DemonstrateSuccessfulTask()
    {
        await ReturnSuccessfulTask();
    }

    [TestMethod]
    public async Task DemonstrateSuccessfulTaskWithValue()
    {
        const int value = 42;
        
        var returnedValue = await ReturnSuccessfulTask(value);
        returnedValue.Should().Be(value);
    }

    [TestMethod]
    public async Task DemonstrateFailedTask()
    {
        await this.Invoking(async t => await t.ReturnFailedTask())
            .Should().ThrowAsync<Exception>().WithMessage("Task Failed");
    }

    [TestMethod]
    public async Task DemonstrateCancelledTask()
    {
        var source = new CancellationTokenSource();
        source.Cancel();

        await this.Invoking(async t => await t.ReturnCancelledTask(source.Token))
            .Should().ThrowAsync<TaskCanceledException>()
            .Where(e => e.CancellationToken == source.Token);
    }

    private Task ReturnSuccessfulTask() => Task.CompletedTask;
    private Task<T> ReturnSuccessfulTask<T>(T value) => Task.FromResult(value);
    private Task ReturnFailedTask() => Task.FromException(new Exception("Task failed"));

    private Task ReturnCancelledTask(CancellationToken cancellationToken) => Task.FromCanceled(cancellationToken);
}