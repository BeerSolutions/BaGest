using System;
using System.Collections.Generic;
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

        var links = Regex.Matches(content, @"<a\s(?<attributes>[^>]*)>\s*<span>(?<text>[^<]+)</span>\s*</a>")
            .Select(m => (Text: m.Groups["text"].Value, Attributes: ParseAttributes(m.Groups["attributes"].Value)))
            .ToList();

        Assert.Equal(new[] { "Packages", "Upload", "Statistics", "Documentation" }, links.Select(l => l.Text));
        Assert.All(links, l => Assert.Contains("nav-link", ClassesOf(l.Attributes)));
        Assert.All(links, l => Assert.False(l.Attributes.ContainsKey("nav-link"), "The tag helper marker attribute was rendered."));
        Assert.Equal(new[] { expectedActive }, links.Where(l => ClassesOf(l.Attributes).Contains("active")).Select(l => l.Text));
        Assert.Equal(new[] { expectedActive }, links.Where(l => l.Attributes.GetValueOrDefault("aria-current") == "page").Select(l => l.Text));
    }

    [Fact]
    public async Task CollapsesTheNavigationBehindAToggleButton()
    {
        var content = await GetPageAsync("/");

        var toggler = FindElements(content, "button").SingleOrDefault(a => ClassesOf(a).Contains("navbar-toggler"));
        Assert.NotNull(toggler);
        Assert.Equal("collapse", toggler.GetValueOrDefault("data-bs-toggle"));
        Assert.Equal("#main-navigation", toggler.GetValueOrDefault("data-bs-target"));
        Assert.Equal("main-navigation", toggler.GetValueOrDefault("aria-controls"));
        Assert.Equal("false", toggler.GetValueOrDefault("aria-expanded"));

        var collapse = FindElements(content, "div").SingleOrDefault(a => a.GetValueOrDefault("id") == "main-navigation");
        Assert.NotNull(collapse);
        Assert.Contains("collapse", ClassesOf(collapse));
        Assert.Contains("navbar-collapse", ClassesOf(collapse));

        // The menu is inside the collapsed area; the search form follows it and stays visible.
        var collapseStart = content.IndexOf("id=\"main-navigation\"", StringComparison.Ordinal);
        var menuEnd = content.IndexOf("</ul>", collapseStart, StringComparison.Ordinal);
        var collapseEnd = content.IndexOf("</div>", menuEnd, StringComparison.Ordinal);
        var searchForm = content.IndexOf("role=\"search\"", StringComparison.Ordinal);
        Assert.True(menuEnd > collapseStart && collapseEnd > menuEnd, "The navigation menu must be inside the collapsed area.");
        Assert.True(searchForm > collapseEnd, "The search form must not be inside the collapsed navigation.");

        Assert.Contains("/_content/BaGest.Web/lib/bootstrap/dist/js/bootstrap.bundle.min.js", content);
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

    private static IEnumerable<Dictionary<string, string>> FindElements(string content, string tagName) =>
        Regex.Matches(content, $@"<{tagName}\s(?<attributes>[^>]*)>")
            .Select(m => ParseAttributes(m.Groups["attributes"].Value));

    private static Dictionary<string, string> ParseAttributes(string attributes) =>
        Regex.Matches(attributes, @"(?<name>[\w:-]+)(?:=""(?<value>[^""]*)"")?")
            .ToDictionary(m => m.Groups["name"].Value, m => m.Groups["value"].Value);

    private static string[] ClassesOf(IReadOnlyDictionary<string, string> attributes) =>
        attributes.TryGetValue("class", out var value)
            ? value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();
}
