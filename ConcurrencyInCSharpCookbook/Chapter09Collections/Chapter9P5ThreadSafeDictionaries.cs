using System.Collections.Concurrent;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter09Collections;

[TestClass]
public class Chapter9P5ThreadSafeDictionaries
{
    [TestMethod]
    public void UsingThreadSafeDictionary()
    {
        var dic = new ConcurrentDictionary<int, int>();
        dic.AddOrUpdate(123, 0, (key, oldValue) => 0);
        dic.AddOrUpdate(456, 1, (key, oldValue) => 1);
        dic.AddOrUpdate(789, 2, (key, oldValue) => 2);
        dic.Values.Should().BeEquivalentTo(new[] {0, 1, 2});

        dic.AddOrUpdate(456, 42, (key, oldValue) => 3);
        dic.Values.Should().BeEquivalentTo(new[] {0, 3, 2});
        
        // An easy way to add or update, but without the ability to retrieve the existing value using indexers.
        dic[321] = 4;
        dic[321].Should().Be(4);
        dic[321] = 5;
        dic[321].Should().Be(5);

        var couldReadValue = dic.TryGetValue(321, out var readValue);
        couldReadValue.Should().BeTrue();
        readValue.Should().Be(5);

        var couldRemoveValue = dic.TryRemove(321, out var removeValue);
        couldRemoveValue.Should().BeTrue();
        removeValue.Should().Be(5);
    }
}