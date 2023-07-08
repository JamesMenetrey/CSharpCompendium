using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter12Synchronization;

[TestClass]
public class Chapter12P3BlockingSignals
{
    [TestMethod]
    public async Task UsingBlockingSignal()
    {
        var demo = new DemonstrateManualResetEventSlim();

        var initializationTask = Task.Run(() => demo.WaitForInitialization());
        await Task.Run(() => demo.InitializeFromAnotherThread(42));

        var value = await initializationTask;

        value.Should().Be(42);
    }

    private class DemonstrateManualResetEventSlim
    {
        private readonly ManualResetEventSlim _signal = new();
        private int _value;

        public int WaitForInitialization()
        {
            _signal.Wait();
            return _value;
        }

        public void InitializeFromAnotherThread(int value)
        {
            _value = value;
            _signal.Set();
        }
    }
}