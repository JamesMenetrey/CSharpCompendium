using FluentAssertions;
using Nito.AsyncEx;

namespace ConcurrencyInCSharpCookbook.Chapter12Synchronization;

[TestClass]
public class Chapter12P2AsyncLocks
{
    /// <summary>
    /// This sample demonstrates the built-in type <see cref="SemaphoreSlim" />.
    /// </summary>
    [TestMethod]
    public async Task UsingSemaphoreSlimForSynchronization()
    {
        var demo = new DemonstrateSemaphoreSlim();

        await Parallel.ForEachAsync(Enumerable.Range(0, 1000), async (_, _) => await demo.IncrementAsync());

        demo.Value.Should().Be(1000);
    }

    /// <summary>
    /// This sample demonstrates the <see cref="Nito.AsyncEx"/> library type <see cref="AsyncLock"/>.
    /// </summary>
    [TestMethod]
    public async Task UsingAsyncLockForSynchronization()
    {
        var demo = new DemonstrateAsyncLock();

        await Parallel.ForEachAsync(Enumerable.Range(0, 1000), async (_, _) => await demo.IncrementAsync());

        demo.Value.Should().Be(1000);
    }

    private class DemonstrateSemaphoreSlim
    {
        private readonly SemaphoreSlim _mutex = new(1);

        public int Value { get; private set; }

        public async Task IncrementAsync()
        {
            await _mutex.WaitAsync();
            Value++;
            _mutex.Release();
        }
    }

    private class DemonstrateAsyncLock
    {
        private readonly AsyncLock _mutex = new();
        
        public int Value { get; private set; }
        
        public async Task IncrementAsync()
        {
            using (await _mutex.LockAsync())
            {
                Value++;
            }
        }
    }
}