using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BaGest.Web;
using Xunit;
using Xunit.Abstractions;

namespace BaGest.Tests;

public class PackageDeprecationIntegrationTests : IDisposable
{
    private const string ApiKey = "deprecation-test-key";
    private const string DeprecationUrl = "api/v2/package/TestData/1.2.3/deprecation";
    private const string RegistrationIndexUrl = "v3/registration/TestData/index.json";

    private readonly BaGestApplication _app;
    private readonly HttpClient _client;
    private readonly Stream _packageStream;

    public PackageDeprecationIntegrationTests(ITestOutputHelper output)
    {
        _app = new BaGestApplication(output, inMemoryConfiguration: dict => dict.Add("ApiKey", ApiKey));
        _client = _app.CreateClient();
        _packageStream = TestResources.GetResourceStream(TestResources.Package);
    }

    [Fact]
    public async Task Deprecate_AddsDeprecationToRegistration()
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);

        // Act
        using var response = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """
            {
              "reasons": ["legacy", "CriticalBugs"],
              "message": "Use TestData.Next instead.",
              "alternatePackageId": "TestData.Next",
              "alternatePackageVersionRange": "2.0.0"
            }
            """);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("""
            {
              "reasons": [
                "Legacy",
                "CriticalBugs"
              ],
              "message": "Use TestData.Next instead.",
              "alternatePackage": {
                "id": "TestData.Next",
                "range": "[2.0.0, )"
              }
            }
            """, await GetDeprecationJsonOrNullAsync());
    }

    [Fact]
    public async Task Deprecate_WithoutAlternateVersionRange_AllowsAnyVersion()
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);

        // Act
        using var response = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """
            { "reasons": ["Other"], "alternatePackageId": "TestData.Next" }
            """);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("""
            {
              "reasons": [
                "Other"
              ],
              "alternatePackage": {
                "id": "TestData.Next",
                "range": "*"
              }
            }
            """, await GetDeprecationJsonOrNullAsync());
    }

    [Fact]
    public async Task Deprecate_Twice_ReplacesDeprecation()
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);
        using var first = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """
            { "reasons": ["Legacy"], "message": "Old message", "alternatePackageId": "TestData.Next" }
            """);

        // Act
        using var response = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """
            { "reasons": ["CriticalBugs"] }
            """);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("""
            {
              "reasons": [
                "CriticalBugs"
              ]
            }
            """, await GetDeprecationJsonOrNullAsync());
    }

    [Fact]
    public async Task Undeprecate_RemovesDeprecationFromRegistration()
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);
        using var deprecate = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """
            { "reasons": ["Legacy"] }
            """);

        // Act
        using var response = await SendAsync(HttpMethod.Delete, DeprecationUrl, ApiKey);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deprecate.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(await GetDeprecationJsonOrNullAsync());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "reasons": [] }""")]
    [InlineData("""{ "reasons": ["Obsolete"] }""")]
    [InlineData("""{ "reasons": ["Legacy"], "alternatePackageVersionRange": "1.0.0" }""")]
    [InlineData("""{ "reasons": ["Legacy"], "alternatePackageId": "TestData.Next", "alternatePackageVersionRange": "not a range" }""")]
    [InlineData("""{ "reasons": "Legacy" }""")]
    [InlineData("not json")]
    [InlineData("")]
    public async Task Deprecate_InvalidRequest_ReturnsBadRequest(string body)
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);

        // Act
        using var response = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(await GetDeprecationJsonOrNullAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("wrong-key")]
    public async Task Deprecate_InvalidApiKey_ReturnsUnauthorized(string apiKey)
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);

        // Act
        using var response = await SendAsync(HttpMethod.Put, DeprecationUrl, apiKey, """{ "reasons": ["Legacy"] }""");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(await GetDeprecationJsonOrNullAsync());
    }

    [Fact]
    public async Task Undeprecate_InvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);
        using var deprecate = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """{ "reasons": ["Legacy"] }""");

        // Act
        using var response = await SendAsync(HttpMethod.Delete, DeprecationUrl, "wrong-key");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(await GetDeprecationJsonOrNullAsync());
    }

    [Theory]
    [InlineData("api/v2/package/PackageDoesNotExist/1.0.0/deprecation")]
    [InlineData("api/v2/package/TestData/9.9.9/deprecation")]
    [InlineData("api/v2/package/TestData/not-a-version/deprecation")]
    public async Task Deprecate_PackageDoesNotExist_ReturnsNotFound(string url)
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);

        // Act
        using var response = await SendAsync(HttpMethod.Put, url, ApiKey, """{ "reasons": ["Legacy"] }""");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Undeprecate_PackageDoesNotExist_ReturnsNotFound()
    {
        // Act
        using var response = await SendAsync(HttpMethod.Delete, DeprecationUrl, ApiKey);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deprecate_UnlistedPackage_IsAllowed()
    {
        // Arrange
        await _app.AddPackageAsync(_packageStream);
        using var unlist = await SendAsync(HttpMethod.Delete, "api/v2/package/TestData/1.2.3", ApiKey);

        // Act
        using var response = await SendAsync(HttpMethod.Put, DeprecationUrl, ApiKey, """{ "reasons": ["Legacy"] }""");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, unlist.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string apiKey, string jsonBody = null)
    {
        using var request = new HttpRequestMessage(method, url);

        if (apiKey != null)
        {
            request.Headers.Add(HttpRequestExtensions.ApiKeyHeader, apiKey);
        }

        if (jsonBody != null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        return await _client.SendAsync(request);
    }

    private async Task<string> GetDeprecationJsonOrNullAsync()
    {
        using var response = await _client.GetAsync(RegistrationIndexUrl);
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var catalogEntry = document.RootElement
            .GetProperty("items")[0]
            .GetProperty("items")[0]
            .GetProperty("catalogEntry");

        if (!catalogEntry.TryGetProperty("deprecation", out var deprecation))
        {
            return null;
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(deprecation.GetRawText()));
        return stream.ToPrettifiedJson();
    }

    public void Dispose()
    {
        _packageStream.Dispose();
        _client.Dispose();
        _app.Dispose();
    }
}
