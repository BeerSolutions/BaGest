using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BaGest.Tests;

public class LayoutIntegrationTests : IDisposable
{
    private readonly BaGestApplication _app;
    private readonly HttpClient _client;

    public LayoutIntegrationTests(ITestOutputHelper output)
    {
        _app = new BaGestApplication(output);
        _client = _app.CreateClient();
    }

    [Theory]
    [InlineData("/", "Packages")]
    [InlineData("/upload", "Upload")]
    [InlineData("/stats", "Statistics")]
    public async Task MarksOnlyTheCurrentPageAsActiveInTheNavigation(string path, string expectedActive)
    {
        var content = await GetPageAsync(path);

        var links = Regex.Matches(content, @"<a\s(?<attributes>[^>]*)><span>(?<text>[^<]+)</span></a>")
            .Select(m => (Text: m.Groups["text"].Value, Classes: ClassesOf(m.Groups["attributes"].Value)))
            .ToList();

        Assert.Contains(links, l => l.Text == expectedActive);
        Assert.All(links, l => Assert.Contains("nav-link", l.Classes));
        Assert.Equal(new[] { expectedActive }, links.Where(l => l.Classes.Contains("active")).Select(l => l.Text));
        Assert.DoesNotMatch(@"<a\b[^>]*\snav-link(?=[\s=>])", content);
    }

    public void Dispose()
    {
        _client.Dispose();
        _app.Dispose();
    }

    private async Task<string> GetPageAsync(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    private static string[] ClassesOf(string attributes)
    {
        var match = Regex.Match(attributes, "class=\"(?<value>[^\"]*)\"");
        return match.Success
            ? match.Groups["value"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();
    }
}
