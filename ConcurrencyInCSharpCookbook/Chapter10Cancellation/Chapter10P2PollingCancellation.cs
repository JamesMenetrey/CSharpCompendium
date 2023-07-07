using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter10Cancellation;

[TestClass]
public class Chapter10P2PollingCancellation
{
    [TestMethod]
    public async Task PollingCancellation()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var isProperlyCancelled = false;
        
        // Plan the cancellation in 10ms.
        cancellationTokenSource.CancelAfter(10);
        
        // Demonstrate how to poll and cancel an async processing.

        try
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Yield();
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
            });
        }
        catch (OperationCanceledException)
        {
            isProperlyCancelled = true;
        }

        isProperlyCancelled.Should().BeTrue();
    }
}