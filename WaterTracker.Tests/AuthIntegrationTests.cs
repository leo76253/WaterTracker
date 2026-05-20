using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using WaterTracker.Api.Dtos;

namespace WaterTracker.Tests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(WebHostDefaults.EnvironmentKey, "Testing");
        });
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_ShouldSucceed_WithValidData()
    {
        var email = $"test.{Guid.NewGuid()}@example.com";

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test1234!", "John", "Doe"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        Assert.Equal(email, body.Email);
        Assert.Equal("John", body.Forename);
        Assert.Equal("Doe", body.Surname);
        Assert.NotEmpty(body.UserId);
    }

    [Fact]
    public async Task Register_ShouldFail_WithDuplicateEmail()
    {
        var email = $"dup.{Guid.NewGuid()}@example.com";

        var first = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test1234!", "John", "Doe"));
        first.EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test1234!", "Jane", "Smith"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldFail_WithMissingFields()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldSucceed_WithValidCredentials()
    {
        var email = $"login.{Guid.NewGuid()}@example.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test1234!", "Test", "User"));
        registerResponse.EnsureSuccessStatusCode();

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", "Test1234!"),
            new KeyValuePair<string, string>("client_id", "watertracker-blazor")
        ]);

        var response = await _client.PostAsync("/connect/token", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(json);
        Assert.NotEmpty(json!["access_token"].ToString()!);
    }

    [Fact]
    public async Task Login_ShouldFail_WithWrongPassword()
    {
        var email = $"wrongpw.{Guid.NewGuid()}@example.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test1234!", "Test", "User"));
        registerResponse.EnsureSuccessStatusCode();

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", "WrongPassword1!"),
            new KeyValuePair<string, string>("client_id", "watertracker-blazor")
        ]);

        var response = await _client.PostAsync("/connect/token", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldFail_WithNonExistentEmail()
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", $"nonexistent.{Guid.NewGuid()}@example.com"),
            new KeyValuePair<string, string>("password", "Test1234!"),
            new KeyValuePair<string, string>("client_id", "watertracker-blazor")
        ]);

        var response = await _client.PostAsync("/connect/token", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnUser_WithValidToken()
    {
        var email = $"me.{Guid.NewGuid()}@example.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Test1234!", "John", "Smith"));
        registerResponse.EnsureSuccessStatusCode();

        var tokenContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", "Test1234!"),
            new KeyValuePair<string, string>("client_id", "watertracker-blazor")
        ]);

        var tokenResponse = await _client.PostAsync("/connect/token", tokenContent);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var token = tokenJson!["access_token"].ToString()!;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal(email, body!["email"].ToString());
        Assert.Equal("John", body["forename"].ToString());
        Assert.Equal("Smith", body["surname"].ToString());
    }

    [Fact]
    public async Task Me_ShouldReturn401_WithoutToken()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturn401_WithInvalidToken()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token-value");

        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
