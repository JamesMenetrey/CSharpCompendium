using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter11FunctionalStyle;

[TestClass]
public class Chapter11P2AsyncConstructionWithFactories
{
    [TestMethod]
    public async Task UseFactoryInsteadOfConstructorForAsyncConstruction()
    {
        var complexObject = await ComplexClassToConstruct.CreateAsync();
        complexObject.IsConstructedAsynchronously.Should().BeTrue();
    }

    private class ComplexClassToConstruct
    {
        /// <summary>
        /// Exposes a static method as a factory for async logic.
        /// </summary>
        public static async Task<ComplexClassToConstruct> CreateAsync()
        {
            await Task.Yield();
            return new ComplexClassToConstruct
            {
                IsConstructedAsynchronously = true
            };
        }
        
        /// <summary>
        /// The constructor is private so nobody except the factory can create it.
        /// </summary>
        private ComplexClassToConstruct()
        {
        }

        public bool IsConstructedAsynchronously { get; private init; }
    }
}