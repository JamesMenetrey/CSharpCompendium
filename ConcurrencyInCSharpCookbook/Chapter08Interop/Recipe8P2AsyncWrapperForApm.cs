using System.Net;
using FluentAssertions;

namespace ConcurrencyInCSharpCookbook.Chapter08Interop;

[TestClass]
public class Recipe8P2AsyncWrapperForApm
{
    [TestMethod]
    public async Task DownloadStringAsync()
    {
        var response = await InteropWithApmDownloadAsync(new Uri("https://www.google.com"));
        using var reader = new StreamReader(response.GetResponseStream());
        var content = await reader.ReadToEndAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("html");
    }

    /// <summary>
    /// The interop can be accomplished by using the automatic wrapper <c>FromAsync</c> methods from
    /// <see cref="TaskFactory"/> for APM (Asynchronous Programming Model).
    /// APM has paired methods called BeginSomething and EndSomething.
    /// A manual wrapper can be written using <see cref="TaskCompletionSource"/> as well if needed.
    /// </summary>
    private Task<WebResponse> InteropWithApmDownloadAsync(Uri address)
    {
#pragma warning disable SYSLIB0014
        var request = WebRequest.CreateHttp(address);
#pragma warning restore SYSLIB0014

        return Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
    }
}