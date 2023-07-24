using System.Net;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter08Interop;

[TestClass]
public class Recipe8P1AsyncWrapperForEap
{
    [TestMethod]
    public async Task DownloadStringAsync()
    {
        var content = await InteropWithEapDownloadAsync(new Uri("https://www.google.com"));
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("html");
    }

    /// <summary>
    /// The EAP (Event-based Asynchronous Pattern) exposes an action as a method SomethingAsync and a paired event
    /// SomethingCompleted. We can use <see cref="TaskCompletionSource"/> to interop with this pattern.
    /// </summary>
    private Task<string> InteropWithEapDownloadAsync(Uri address)
    {
#pragma warning disable SYSLIB0014
        var client = new WebClient();
#pragma warning restore SYSLIB0014
        var taskSource = new TaskCompletionSource<string>();

        void ClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // Unsubscribe this handler to avoid memory leaks
            client.DownloadStringCompleted -= ClientOnDownloadStringCompleted;

            if (e.Error != null)
                taskSource.SetException(e.Error);
            else if (e.Cancelled)
                taskSource.SetCanceled();
            else
                taskSource.SetResult(e.Result);
        }

        client.DownloadStringCompleted += ClientOnDownloadStringCompleted;
        client.DownloadStringAsync(address);

        return taskSource.Task;
    }
}