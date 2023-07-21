using FluentAssertions;
using Nito.AsyncEx;

namespace ConcurrencyInCSharpCookbook.Chapter14Scenarios;

[TestClass]
public class Recipe14P1InitializingSharedResources
{
    /// <summary>
    /// The native async initialization of shared resources is to use the type <see cref="Lazy{T}"/>.
    /// </summary>
    [TestMethod]
    public async Task NaiveInitializationAsync()
    {
        var numberOfInit = 0;

        var asyncInit = new NaiveInitializationAsyncDemo(async () =>
        {
            numberOfInit++;
            await Task.Yield();
            return Random.Shared.Next();
        });

        (await asyncInit.Value).Should().Be(await asyncInit.Value);
        numberOfInit.Should().Be(1);
    }

    /// <summary>
    /// Nonetheless, if the call throws an exception (e.g., a network issue), the exception will be persisted,
    /// with no other way to recover the situation.
    /// </summary>
    [TestMethod]
    public async Task NaiveInitializationAsyncCannotHandleRetrials()
    {
        var numberOfInit = 0;
        var shouldTriggerAnException = true;

        var asyncInit = new NaiveInitializationAsyncDemo(async () =>
        {
            numberOfInit++;
            await Task.Yield();
            // ReSharper disable once AccessToModifiedClosure
            if (shouldTriggerAnException) throw new Exception("Temporary exception");
            return 42;
        });

        await asyncInit.Awaiting(x => x.Value).Should().ThrowAsync<Exception>();
        await asyncInit.Awaiting(x => x.Value).Should().ThrowAsync<Exception>();

        shouldTriggerAnException = false;
        await asyncInit.Awaiting(x => x.Value).Should().ThrowAsync<Exception>();

        // The Lazy class does not attempt to retry when an exception occurs.
        numberOfInit.Should().Be(1);
    }

    /// <summary>
    /// This can be prevented, using an advanced implementation of exception handling. However, this requires some
    /// code implementation to handle such cases.
    /// </summary>
    [TestMethod]
    public async Task AdvancedInitializationAsyncWithRetrial()
    {
        var numberOfInit = 0;
        var shouldTriggerAnException = true;

        var asyncInit = new AdvancedInitializationAsyncDemo(async () =>
        {
            numberOfInit++;
            await Task.Yield();
            // ReSharper disable once AccessToModifiedClosure
            if (shouldTriggerAnException) throw new Exception("Temporary exception");
            return 42;
        });

        await asyncInit.Awaiting(x => x.Task).Should().ThrowAsync<Exception>();
        await asyncInit.Awaiting(x => x.Task).Should().ThrowAsync<Exception>();

        shouldTriggerAnException = false;
        await asyncInit.Awaiting(x => x.Task).Should().NotThrowAsync<Exception>();
        
        // The initialization logic won't be call again, since it successfully returns before.
        shouldTriggerAnException = true;
        await asyncInit.Awaiting(x => x.Task).Should().NotThrowAsync<Exception>();

        // The number of retries is of 3: two failures and one success.
        numberOfInit.Should().Be(3);
    }

    /// <summary>
    /// An alternative is to use the <see cref="Nito.AsyncEx"/> library with the type <see cref="AsyncLazy{T}"/>,
    /// designed to solve this exact situation.
    /// </summary>
    [TestMethod]
    public async Task AdvancedInitializationAsyncWithAsyncExLibrary()
    {
        var numberOfInit = 0;
        var shouldTriggerAnException = true;
        var asyncInit = new AsyncLazy<int>(async () =>
        {
            numberOfInit++;
            await Task.Yield();
            // ReSharper disable once AccessToModifiedClosure
            if (shouldTriggerAnException) throw new Exception("Temporary exception");
            return 42;
        }, AsyncLazyFlags.RetryOnFailure);

        await asyncInit.Awaiting(x => x.Task).Should().ThrowAsync<Exception>();
        await asyncInit.Awaiting(x => x.Task).Should().ThrowAsync<Exception>();

        shouldTriggerAnException = false;
        await asyncInit.Awaiting(x => x.Task).Should().NotThrowAsync<Exception>();
        
        // The initialization logic won't be call again, since it successfully returns before.
        shouldTriggerAnException = true;
        await asyncInit.Awaiting(x => x.Task).Should().NotThrowAsync<Exception>();

        // The number of retries is of 3: two failures and one success.
        numberOfInit.Should().Be(3);
    }

    private class NaiveInitializationAsyncDemo
    {
        private readonly Lazy<Task<int>> _lazyAsync;

        public NaiveInitializationAsyncDemo(Func<Task<int>> creation)
        {
            _lazyAsync = new Lazy<Task<int>>(creation);
        }

        public Task<int> Value => _lazyAsync.Value;
    }

    private class AdvancedInitializationAsyncDemo
    {
        private readonly Func<Task<int>> _factory;
        private readonly object _mutex = new();
        private Lazy<Task<int>> _lazyAsync;

        public AdvancedInitializationAsyncDemo(Func<Task<int>> factory)
        {
            _factory = factory;
            _lazyAsync = new Lazy<Task<int>>(WithRetryLogic);
        }

        private async Task<int> WithRetryLogic()
        {
            try
            {
                return await _factory();
            }
            catch (Exception)
            {
                lock (_mutex)
                {
                    _lazyAsync = new Lazy<Task<int>>(WithRetryLogic);
                }

                throw;
            }
        }

        public Task<int> Task => _lazyAsync.Value;
    }
}