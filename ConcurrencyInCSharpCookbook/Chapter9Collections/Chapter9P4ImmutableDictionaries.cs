using System.Collections.Immutable;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter9Collections;

[TestClass]
public class Chapter9P4ImmutableDictionaries
{
    [TestMethod]
    public void UsingImmutableDictionary()
    {
        var dic = ImmutableDictionary<int, int>.Empty;
        dic = dic.Add(123, 0); // Complexity: O(log n).
        dic = dic.Add(456, 1);
        dic = dic.Add(789, 2);
        dic.Values.Should().BeEquivalentTo(new[] { 0, 1, 2 });

        // Instead of the regular syntax dic[key] = value.
        dic = dic.SetItem(456, 3); // Complexity: O(log n).
        dic.Values.Should().BeEquivalentTo(new[] { 0, 3, 2 });

        var valueByKey = dic[456]; // Complexity: O(log n).
        valueByKey.Should().Be(3);

        dic = dic.Remove(123); // Complexity: O(log n).
        dic.Values.Should().BeEquivalentTo(new[] { 3, 2 });
    }

    [TestMethod]
    public void UsingImmutableSortedDictionary()
    {
        var dic = ImmutableSortedDictionary<int, int>.Empty;
        dic = dic.Add(123, 0);
        dic = dic.Add(456, 1);
        dic = dic.Add(789, 2);
        dic.Values.Should().BeEquivalentTo(new[] { 0, 1, 2 }, options => options.WithStrictOrdering());

        // Instead of the regular syntax dic[key] = value.
        dic = dic.SetItem(456, 3); // Complexity: O(log n).
        dic.Values.Should().BeEquivalentTo(new[] { 0, 3, 2 }, options => options.WithStrictOrdering());

        var valueByKey = dic[456]; // Complexity: O(log n).
        valueByKey.Should().Be(3);

        dic = dic.Remove(123); // Complexity: O(log n).
        dic.Values.Should().BeEquivalentTo(new[] { 3, 2 }, options => options.WithStrictOrdering());
    }
}