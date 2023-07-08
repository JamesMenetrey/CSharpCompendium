using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter11FunctionalStyle;

[TestClass]
public class Chapter11P3AsyncInitPattern
{
    /// <summary>
    /// If the object is created using reflection (e.g., using a DI framework), the asynchronous initialization pattern
    /// helps initializing classes requiring async initialization logic.
    /// </summary>
    [TestMethod]
    public async Task UseClassWithAsyncInitPattern()
    {
        var complexObject = new ComplexClass();
        await complexObject.Initialization;

        complexObject.IsConstructedAsynchronously.Should().BeTrue();
    }
    
    /// <summary>
    /// Mark a type as requiring asynchronous initialization and provides the result of that initialization.
    /// </summary>
    private interface IAsyncInitialization
    {
        /// <summary>
        /// The result of th asynchronous initialization of this instance.
        /// </summary>
        Task Initialization { get; }
    }
    
    private class ComplexClass : IAsyncInitialization
    {
        public ComplexClass()
        {
            Initialization = InitializeAsync();
        }
        
        public Task Initialization { get; }
        public bool IsConstructedAsynchronously { get; private set; }

        private async Task InitializeAsync()
        {
            await Task.Yield();
            IsConstructedAsynchronously = true;
            // This method can compose other objects by awaiting their property Initialization.
        }
    }
}