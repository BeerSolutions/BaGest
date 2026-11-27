using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Versioning;
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

    [Fact]
    public async Task StylesTheSearchButtonAsTheSiteSearchButton()
    {
        var content = await GetPageAsync("/");

        // The site's search box styles apply through the search-form class; a Bootstrap
        // button variant would override the solid orange button.
        var form = FindElements(content, "form").SingleOrDefault(a => a.GetValueOrDefault("role") == "search");
        Assert.NotNull(form);
        Assert.Contains("search-form", ClassesOf(form));

        var button = FindElements(content, "button").SingleOrDefault(a => a.GetValueOrDefault("type") == "submit");
        Assert.NotNull(button);
        Assert.Equal(new[] { "btn" }, ClassesOf(button));
    }

    [Fact]
    public async Task LeavesTheSizeOfSearchResultTitlesToTheStylesheet()
    {
        await _app.AddPackageAsync(TestResources.GetResourceStream(TestResources.Package));

        var content = await GetPageAsync("/");

        // Font utilities are !important and would override the title size and weight in site.css.
        var titles = FindElements(content, "a").Where(a => ClassesOf(a).Contains("package-title")).ToList();
        Assert.Single(titles);
        Assert.Equal(new[] { "package-title", "text-decoration-none" }, ClassesOf(titles[0]));
    }

    [Fact]
    public async Task ShowsPackageTagsAsPlainChips()
    {
        var builder = new PackageBuilder
        {
            Id = "Tagged.Package",
            Version = NuGetVersion.Parse("1.0.0"),
            Description = "A package with tags.",
        };
        builder.Authors.Add("Test Author");
        builder.Tags.Add("first");
        builder.Tags.Add("second");
        builder.Files.Add(new PhysicalPackageFile { SourcePath = GetType().Assembly.Location, TargetPath = "lib/netstandard2.0/Test.dll" });
        using var package = new MemoryStream();
        builder.Save(package);
        package.Position = 0;
        await _app.AddPackageAsync(package);

        var content = await GetPageAsync("/packages/Tagged.Package/1.0.0");

        // The badge class shrinks the text and rounds the chip like a button.
        var tags = Regex.Matches(content, @"<span\s(?<attributes>[^>]*)>(?<text>[^<]*)</span>")
            .Where(m => ClassesOf(ParseAttributes(m.Groups["attributes"].Value)).Contains("tag"))
            .ToList();
        Assert.Equal(new[] { "first", "second" }, tags.Select(m => m.Groups["text"].Value));
        Assert.All(tags, m => Assert.Equal(new[] { "tag" }, ClassesOf(ParseAttributes(m.Groups["attributes"].Value))));
    }

    [Theory]
    [InlineData("/upload")]
    [InlineData("/packages/TestData/1.2.3")]
    public async Task AttachesTheInstallTabsToTheCommandBox(string path)
    {
        await _app.AddPackageAsync(TestResources.GetResourceStream(TestResources.Package));

        var content = await GetPageAsync(path);

        // A margin utility on the tab list would put a gap between the tabs and the command they switch.
        var tabs = Regex.Match(content, @"<div class=""tabbed-info[^""]*""[^>]*>\s*<ul\s(?<attributes>[^>]*)>");
        Assert.True(tabs.Success, "The install instructions were not rendered.");
        Assert.Equal(new[] { "nav", "nav-tabs" }, ClassesOf(ParseAttributes(tabs.Groups["attributes"].Value)));
    }

    [Fact]
    public async Task AttachesTheDocumentationNoteToTheUploadCommand()
    {
        var content = await GetPageAsync("/upload");

        var note = FindElements(content, "div").SingleOrDefault(a => ClassesOf(a).Contains("alert-warning"));
        Assert.NotNull(note);
        Assert.Equal(new[] { "icon-text", "alert", "alert-warning" }, ClassesOf(note));
    }

    [Fact]
    public async Task DrawsTheSourceCodeIconInTheTextColor()
    {
        await _app.AddPackageAsync(TestResources.GetResourceStream(TestResources.Package));

        var content = await GetPageAsync("/packages/TestData/1.2.3");

        var item = Regex.Match(content, @"<li>\s*<(?<tag>\w+)\s(?<attributes>[^>]*)>(?:(?!</li>).)*>Source code</a>", RegexOptions.Singleline);
        Assert.True(item.Success, "The source code link was not rendered.");
        Assert.Equal("svg", item.Groups["tag"].Value);

        var icon = ParseAttributes(item.Groups["attributes"].Value);
        Assert.Equal("currentColor", icon.GetValueOrDefault("fill"));
        Assert.Equal("true", icon.GetValueOrDefault("aria-hidden"));
        Assert.Contains("icon", ClassesOf(icon));
    }

    [Fact]
    public async Task SeparatesTheRowsOfTheVersionTable()
    {
        await _app.AddPackageAsync(TestResources.GetResourceStream(TestResources.Package));

        var content = await GetPageAsync("/packages/TestData/1.2.3");

        var table = Regex.Match(content, @"<div class=""version-list""[^>]*>\s*<table\s(?<attributes>[^>]*)>");
        Assert.True(table.Success, "The version table was not rendered.");
        Assert.Equal(new[] { "table" }, ClassesOf(ParseAttributes(table.Groups["attributes"].Value)));
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
