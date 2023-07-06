using System.Collections.Immutable;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter9Collections;

[TestClass]
public class Chapter9P1ImmutableStacksAndQueues
{
    [TestMethod]
    public void UsingImmutableStack()
    {
        var stack = ImmutableStack<int>.Empty;
        stack = stack.Push(1);
        stack = stack.Push(2);
        stack = stack.Push(3);
        stack.Should().Equal(3, 2, 1);
        
        stack = stack.Pop(out var popped);
        stack.Should().Equal(2, 1);
        popped.Should().Be(3);
    }

    [TestMethod]
    public void UsingImmutableQueue()
    {
        var queue = ImmutableQueue<int>.Empty;
        queue = queue.Enqueue(1);
        queue = queue.Enqueue(2);
        queue = queue.Enqueue(3);
        queue.Should().Equal(1, 2, 3);

        queue = queue.Dequeue(out var dequeued);
        queue.Should().Equal(2, 3);
        dequeued.Should().Be(1);
    }
}