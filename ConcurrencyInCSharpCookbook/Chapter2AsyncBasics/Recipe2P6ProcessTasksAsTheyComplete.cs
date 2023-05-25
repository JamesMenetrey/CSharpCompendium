using FluentAssertions;
using Nito.AsyncEx;

namespace ConcurrencyInCSharpCookbook.Chapter2AsyncBasics
{
    [TestClass]
    public class Recipe2P6ProcessTasksAsTheyComplete
    {
        [TestMethod]
        public async Task ProcessByCompletionOrderWithMapping()
        {
            var results = new List<int>(3);
            var task1 = WaitAndReturnAsync(50);
            var task2 = WaitAndReturnAsync(25);
            var task3 = WaitAndReturnAsync(5);
            var tasks = new[] { task1, task2, task3 };

            var completedTasks = tasks.Select(async t =>
            {
                var value = await t;
                results.Add(value);
            });
            await Task.WhenAll(completedTasks);

            results.Should().BeInAscendingOrder();
        }

        [TestMethod]
        public async Task ProcessByCompletionOrderWithAsyncEx()
        {
            var results = new List<int>(3);
            var task1 = WaitAndReturnAsync(50);
            var task2 = WaitAndReturnAsync(25);
            var task3 = WaitAndReturnAsync(5);
            var tasks = new[] { task1, task2, task3 };

            foreach (var task in tasks.OrderByCompletion())
            {
                var value = await task;
                results.Add(value);
            }

            results.Should().BeInAscendingOrder();
        }

        [TestMethod]
        public async Task BadExampleProcessByListOrderNaively()
        {
            var results = new List<int>(3);
            var task1 = WaitAndReturnAsync(50);
            var task2 = WaitAndReturnAsync(25);
            var task3 = WaitAndReturnAsync(5);
            var tasks = new[] { task1, task2, task3 };

            foreach (var task in tasks)
            {
                var result = await task;
                results.Add(result);
            }

            results.Should().BeInDescendingOrder();
        }

        private async Task<int> WaitAndReturnAsync(int value)
        {
            await Task.Delay(value);
            return value;
        }
    }
}
