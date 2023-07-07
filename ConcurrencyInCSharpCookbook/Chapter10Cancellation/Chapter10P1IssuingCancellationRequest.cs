using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter10Cancellation;

[TestClass]
public class Chapter10P1IssuingCancellationRequest
{
    [TestMethod]
    public async Task CancellingTask()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var isProperlyCancelled = false;

        // Plan the cancel the task in 10ms
        cancellationTokenSource.CancelAfter(10);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            isProperlyCancelled = true;
        }

        isProperlyCancelled.Should().BeTrue();
    }
}