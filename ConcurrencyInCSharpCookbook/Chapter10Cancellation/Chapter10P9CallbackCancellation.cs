using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter10Cancellation;

[TestClass]
public class Chapter10P9CallbackCancellation
{
    [TestMethod]
    public async Task CancelTokenWithCallback()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        var isCancelled = false;

        // Register a logic when the token is cancelled.
        token.Register(() => isCancelled = true);
        source.CancelAfter(10);

        await Task.Run(async () =>
        {
            while (!isCancelled)
            {
                await Task.Yield();
            }
        });

        isCancelled.Should().BeTrue();
    }
}