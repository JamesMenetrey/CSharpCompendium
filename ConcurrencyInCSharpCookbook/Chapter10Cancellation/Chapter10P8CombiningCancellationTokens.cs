using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter10Cancellation;

[TestClass]
public class Chapter10P8CombiningCancellationTokens
{
    [TestMethod]
    public void ComposeAndCancelToken()
    {
        var parentSource = new CancellationTokenSource();
        var parent = parentSource.Token;
        var childSource1 = CancellationTokenSource.CreateLinkedTokenSource(parent);
        var child1 = childSource1.Token;
        var childSource2 = CancellationTokenSource.CreateLinkedTokenSource(parent);
        var child2 = childSource2.Token;

        childSource1.Cancel();
        parent.IsCancellationRequested.Should().BeFalse();
        child1.IsCancellationRequested.Should().BeTrue();

        parentSource.Cancel();
        parent.IsCancellationRequested.Should().BeTrue();
        child2.IsCancellationRequested.Should().BeTrue();
    }
}