using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TodoApi.Tests;

public class SwaggerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SwaggerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_ReturnsSuccessAndValidDocument()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // Verify it's valid JSON
        var document = JsonDocument.Parse(content);
        Assert.NotNull(document);

        // Verify OpenAPI version
        Assert.True(document.RootElement.TryGetProperty("openapi", out var version));
        Assert.StartsWith("3.", version.GetString());
    }

    [Fact]
    public async Task SwaggerJson_ContainsApiInfo()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);

        // Assert
        Assert.True(document.RootElement.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("title", out var title));
        Assert.Equal("TodoApi", title.GetString());
        Assert.True(info.TryGetProperty("version", out var versionProperty));
        Assert.Equal("v1", versionProperty.GetString());
    }

    [Fact]
    public async Task SwaggerJson_ContainsAuthEndpoints()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);

        // Assert
        Assert.True(document.RootElement.TryGetProperty("paths", out var paths));

        // Check for auth endpoints
        Assert.True(paths.TryGetProperty("/api/Auth/register", out _), "Missing /api/Auth/register endpoint");
        Assert.True(paths.TryGetProperty("/api/Auth/login", out _), "Missing /api/Auth/login endpoint");
    }

    [Fact]
    public async Task SwaggerJson_ContainsSecurityDefinition()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);

        // Assert - Check for security scheme
        Assert.True(document.RootElement.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("securitySchemes", out var schemes));
        Assert.True(schemes.TryGetProperty("Bearer", out var bearer));
        Assert.True(bearer.TryGetProperty("type", out var type));
        Assert.Equal("http", type.GetString());
    }

    [Fact]
    public async Task SwaggerJson_ContainsSchemas()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);

        // Assert
        Assert.True(document.RootElement.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));

        // Verify DTOs are documented
        Assert.True(schemas.TryGetProperty("RegisterRequestDto", out _), "Missing RegisterRequestDto schema");
        Assert.True(schemas.TryGetProperty("LoginRequestDto", out _), "Missing LoginRequestDto schema");
        Assert.True(schemas.TryGetProperty("AuthResponseDto", out _), "Missing AuthResponseDto schema");
    }

    [Fact]
    public async Task SwaggerUI_ReturnsSuccessAndHtml()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("swagger-ui", content.ToLowerInvariant());
    }

    [Fact]
    public async Task SwaggerJson_WeatherEndpointIsDocumented()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);

        // Assert
        Assert.True(document.RootElement.TryGetProperty("paths", out var paths));
        Assert.True(paths.TryGetProperty("/weatherforecast", out var weather), "Missing /weatherforecast endpoint");
        Assert.True(weather.TryGetProperty("get", out var get));

        // Verify endpoint has proper documentation
        Assert.True(get.TryGetProperty("summary", out _), "Weather endpoint should have summary");
        Assert.True(get.TryGetProperty("tags", out _), "Weather endpoint should have tags");
    }

    [Fact]
    public async Task SwaggerJson_HasGlobalSecurityRequirement()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(content);

        // Assert - Check for global security requirement
        Assert.True(document.RootElement.TryGetProperty("security", out var security));
        Assert.True(security.GetArrayLength() > 0, "Should have at least one global security requirement");
    }
}
