using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter12Synchronization;

[TestClass]
public class Chapter12P1Locks
{
    [TestMethod]
    public void UsingLockForSynchronization()
    {
        var demo = new DemonstrateLock();
        
        Parallel.For(0, 1000, (_, _) => demo.Increment());

        demo.Value.Should().Be(1000);
    }

    /// <summary>
    /// 4 important guidelines when using locks:
    /// - Restrict lock visibility: the object used in the lock statement should be a private field and never should be
    /// exposed to any method outside the class.
    /// - Document what the lock protects: this becomes more important as the code grows in complexity.
    /// - Minimize the code under lock: particularly: your code should never block while holding a lock.
    /// - Never execute arbitrary code while holding a lock: arbitrary code can include raising events, invoking virtual
    /// methods, or invoking delegates.
    /// </summary>
    private class DemonstrateLock
    {
        private readonly object _mutex = new();

        public int Value { get; private set; }

        public void Increment()
        {
            lock (_mutex)
            {
                Value++;
            }
        }
    }
}