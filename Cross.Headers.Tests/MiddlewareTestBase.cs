namespace Cross.Headers.Tests;

public abstract class MiddlewareTestBase
{
    protected static IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LanguageDefault"] = "EN",
                ["CurrencyDefault"] = "USD",
                ["CSPFrameAncestors"] = "'self'"
            })
            .Build();

    protected static IHost CreateHost(Action<IApplicationBuilder> configurePipeline)
    {
        var config = CreateConfig();
        return new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    services.AddSingleton<IConfiguration>(config);
                    services.AddHeaders(config);
                });
                web.Configure(configurePipeline);
            })
            .Build();
    }

    protected static async Task<ContextDto> GetContextAsync(IHost host, string path = "/", Action<HttpRequestMessage>? configureRequest = null, bool omitUserAgent = false)
    {
        await host.StartAsync();
        var client = omitUserAgent ? host.GetTestServer().CreateClient() : host.GetTestClient();
        if (omitUserAgent)
            client.DefaultRequestHeaders.Remove("User-Agent");

        HttpResponseMessage response;
        if (configureRequest != null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            configureRequest(request);
            response = await client.SendAsync(request);
        }
        else
        {
            response = await client.GetAsync(path);
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<ContextDto>(json, options)!;
    }

    protected static async Task<ContextDto> GetContextFromRequestAsync(IHost host, Action<RequestBuilder>? configureRequest = null)
    {
        await host.StartAsync();
        var builder = host.GetTestServer().CreateRequest("/");
        configureRequest?.Invoke(builder);
        var response = await builder.GetAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<ContextDto>(json, options)!;
    }

    protected static async Task<ContextDto> GetContextViaSendAsync(IHost host, Action<HttpContext>? configureContext = null)
    {
        await host.StartAsync();
        var httpContext = await host.GetTestServer().SendAsync(ctx =>
        {
            configureContext?.Invoke(ctx);
        });
        httpContext.Response.StatusCode.Should().Be(200);
        using var reader = new StreamReader(httpContext.Response.Body);
        var json = await reader.ReadToEndAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return System.Text.Json.JsonSerializer.Deserialize<ContextDto>(json, options)!;
    }

    protected static async Task<HttpResponseMessage> SendRequestAsync(IHost host, Action<RequestBuilder>? configureRequest = null)
    {
        await host.StartAsync();
        var builder = host.GetTestServer().CreateRequest("/");
        configureRequest?.Invoke(builder);
        return await builder.GetAsync();
    }

    protected sealed class ContextDto
    {
        public Guid CorrelationId { get; set; }
        public int SystemId { get; set; }
        public int BusinessId { get; set; }
        public string? LanguageCode { get; set; }
        public string? CurrencyCode { get; set; }
        public string? UserAgent { get; set; }
        public int UserAgentKind { get; set; }
        public string? UserAccessToken { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsUser { get; set; }
        public string[]? UserScopes { get; set; }
    }
}
