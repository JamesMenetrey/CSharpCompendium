using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter04ParallelBasics;

/// <summary>
/// Using <see cref="Task"/> for parallel processing is completely different than using
/// <see cref="Task"/> for asynchronous processing. Indeed, <see cref="Task"/> serves
/// two purposes in concurrent programming: it can be parallel task or an asynchronous task.
/// </summary>
[TestClass]
public class Recipe4P4DynamicParallelism
{
    [TestMethod]
    public void ProcessTree()
    {
        var task = Task.Factory.StartNew(
            function: () => SumSubNodes(GenerateTree()),
            cancellationToken: CancellationToken.None,
            creationOptions: TaskCreationOptions.None,
            scheduler: TaskScheduler.Default);

        task.Wait();

        task.Result.Should().Be(20);
    }

    /// <summary>
    /// Sum the sub nodes and using <see cref="TaskCreationOptions.AttachedToParent"/> so the
    /// parent task is awaiting the child nodes.
    /// </summary>
    private int SumSubNodes(Node node)
    {
        Task<int>?[] tasks = new Task<int>[2];

        if (node.Left != null)
        {
            tasks[0] = Task.Factory.StartNew(
                function: () => SumSubNodes(node.Left),
                cancellationToken: CancellationToken.None,
                creationOptions: TaskCreationOptions.AttachedToParent,
                scheduler: TaskScheduler.Default);
        }
        
        if (node.Right != null)
        {
            tasks[1] = Task.Factory.StartNew(
                function: () => SumSubNodes(node.Right),
                cancellationToken: CancellationToken.None,
                creationOptions: TaskCreationOptions.AttachedToParent,
                scheduler: TaskScheduler.Default);
        }

        var notNullTasks = tasks.Where(t => t != null).OfType<Task<int>>().ToArray();
        Task.WhenAll(notNullTasks).Wait();

        return node.Value + notNullTasks.Sum(t => t.Result);
    }

    private Node GenerateTree()
    {
        return new Node(
            new Node(
                new Node(null, null, 1),
                new Node(null, null, 4),
                2),
            new Node(null,
                new Node(
                    null,
                    new Node(
                        null, null, 3),
                    2
                ),
                3),
            5
        );
    }

    private record Node(Node? Left, Node? Right, int Value);
}