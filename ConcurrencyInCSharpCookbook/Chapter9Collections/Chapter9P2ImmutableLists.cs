using System.Collections.Immutable;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter9Collections;

[TestClass]
public class Chapter9P2ImmutableLists
{
    /// <summary>
    /// Watch out the complexity of <see cref="ImmutableList"/>, which is different from <see cref="List{T}"/>.
    /// The immutable variant maintains a binary tree to maximise the reuse of the memory.
    /// </summary>
    [TestMethod]
    public void UsingImmutableList()
    {
        var list = ImmutableList<int>.Empty;
        list = list.Add(1); // Complexity: O(log n).
        list = list.Insert(0, 2); // Complexity: O(log n).
        list.Should().Equal(2, 1); // Complexity: O(n).
        list[1].Should().Be(1); // Complexity: O(log n).
        list = list.RemoveAt(1); // Complexity O(log n).

        list.Should().Equal(2);
    }
}