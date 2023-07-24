using System.Collections.Immutable;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter09Collections;

[TestClass]
public class Chapter9P3ImmutableSets
{
    [TestMethod]
    public void UsingImmutableHashSet()
    {
        var set = ImmutableHashSet<int>.Empty;
        set = set.Add(1); // Complexity: O(log n).
        set = set.Add(2);
        set = set.Add(3);
        set = set.Add(3);
        set.Should().BeEquivalentTo(new[] {1, 2, 3});

        set = set.Remove(2); // Complexity: O(log n).
        set.Should().BeEquivalentTo(new[] {1, 3});
    }

    [TestMethod]
    public void UsingImmutableSortedSet()
    {
        var set = ImmutableSortedSet<int>.Empty;
        set = set.Add(1); // Complexity: O(log n).
        set = set.Add(2);
        set = set.Add(3);
        set = set.Add(3);
        set.Should().BeEquivalentTo(new[] {1, 2, 3}, options => options.WithStrictOrdering());

        set = set.Remove(2); // Complexity: O(log n).
        set.Should().BeEquivalentTo(new[] {1, 3}, options => options.WithStrictOrdering());

        var valueByIndex = set[1]; // Complexity: O(log n).
        valueByIndex.Should().Be(3);
    }
}