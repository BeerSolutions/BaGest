using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BaGest.Tests;

public class UiCultureIntegrationTests : IDisposable
{
    // Same expression as the test host uses to pin "now" (see BaGestApplication).
    private static readonly DateTime PublishedDate = DateTime.Parse("2020-01-01T00:00:00.000Z");

    private readonly BaGestApplication _app;
    private readonly HttpClient _client;
    private readonly Stream _packageStream;

    public UiCultureIntegrationTests(ITestOutputHelper output)
    {
        _app = new BaGestApplication(output);
        _client = _app.CreateClient();
        _packageStream = TestResources.GetResourceStream(TestResources.Package);
    }

    // The test host pins "now" to 2020-01-01T00:00:00Z, which is the package's published date.
    [Theory]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("pl-PL")]
    [InlineData("de-DE")]
    [InlineData("ja-JP")]
    public async Task PackagePageRendersVersionDateInBrowserCulture(string acceptLanguage)
    {
        await _app.AddPackageAsync(_packageStream);

        var expected = $"<td>{PublishedDate.ToString("d", CultureInfo.GetCultureInfo(acceptLanguage))}</td>";

        using var request = new HttpRequestMessage(HttpMethod.Get, "packages/TestData");
        request.Headers.TryAddWithoutValidation("Accept-Language", acceptLanguage);
        using var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expected, content);
    }

    [Fact]
    public async Task PackagePageKeepsDefaultDateFormatWithoutAcceptLanguage()
    {
        await _app.AddPackageAsync(_packageStream);

        using var response = await _client.GetAsync("packages/TestData");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"<td>{PublishedDate.ToShortDateString()}</td>", content);
    }

    public void Dispose()
    {
        _packageStream.Dispose();
        _client.Dispose();
        _app.Dispose();
    }
}
