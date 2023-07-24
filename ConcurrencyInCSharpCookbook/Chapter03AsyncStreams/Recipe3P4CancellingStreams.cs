using System.Runtime.CompilerServices;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter03AsyncStreams;

[TestClass]
public class Recipe3P4CancellingStreams
{
    /// <summary>
    /// The cancellation token can be directly passed when calling the async enumerable function.
    /// </summary>
    [TestMethod]
    public async Task CancelByPassingTokenDirectly()
    {
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;
        var lastNumber = 0;

        await foreach (var number in InfiniteEnumerable(token))
        {
            lastNumber = number;
            if (number == 1) tokenSource.Cancel();
        }

        lastNumber.Should().Be(1);
    }

    /// <summary>
    /// The cancellation token can be injected afterwards, using the extension function <c>WithCancellation</c>.
    /// Since the token passed as argument of the function <c>WithCancellation</c> and the parameter <b>token</b>
    /// of the function <see cref="InfiniteEnumerable"/> are not explicitly linked, the compiler requires the usage
    /// of the attribute <see cref="EnumeratorCancellationAttribute"/>.
    /// Hence, the compiler infers where the passed token must be used when calling the async enumerable function.
    /// </summary>
    [TestMethod]
    public async Task CancelByPassingTokenIndirectly()
    {
        var numbers = InfiniteEnumerable();
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;
        var lastNumber = 0;
        
        await foreach (var number in numbers.WithCancellation(token))
        {
            lastNumber = number;
            if (number == 1) tokenSource.Cancel();
        }


        lastNumber.Should().Be(1);

    }

    /// <summary>
    /// The parameter is decorated with <see cref="EnumeratorCancellationAttribute"/> in case the cancellation token
    /// is passed indirectly, so the compiler is instructed to link any token used with a call to <c>WithCancellation</c>.
    /// </summary>
    private async IAsyncEnumerable<int> InfiniteEnumerable([EnumeratorCancellation] CancellationToken token = default)
    {
        var n = 0;
        while (!token.IsCancellationRequested)
        {
            yield return n++;
            await Task.Yield();
        }
    }
}
