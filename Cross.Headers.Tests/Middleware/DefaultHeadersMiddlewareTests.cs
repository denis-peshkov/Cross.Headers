namespace Cross.Headers.Tests.Middleware;

public class DefaultHeadersMiddlewareTests : MiddlewareTestBase
{
    private static void Pipeline(IApplicationBuilder app)
    {
        app.UseMiddleware<HeadersMiddleware>();
        app.UseMiddleware<DefaultHeadersMiddleware>();
        app.Run(ContextEndpoint);
    }

    private static Task ContextEndpoint(HttpContext ctx)
    {
        var accessor = ctx.RequestServices.GetRequiredService<IHeadersContextAccessor>();
        return ctx.Response.WriteAsJsonAsync(new
        {
            accessor.SystemId,
            accessor.BusinessId,
            accessor.CountryCode,
            accessor.LanguageCode,
            accessor.CurrencyCode,
        });
    }

    [Test]
    public async Task ShouldSetSystemIdTo1_WhenNotProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.SystemId.Should().Be(1);
    }

    [Test]
    public async Task ShouldSetBusinessIdTo1_WhenNotProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.BusinessId.Should().Be(1);
    }

    [Test]
    public async Task ShouldSetCountryCodeFromConfig_WhenNotProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.CountryCode.Should().Be("US");
    }

    [Test]
    public async Task ShouldSetLanguageCodeFromConfig_WhenNotProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.LanguageCode.Should().Be("EN");
    }

    [Test]
    public async Task ShouldSetCurrencyCodeFromConfig_WhenNotProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.CurrencyCode.Should().Be("USD");
    }

    [Test]
    public async Task ShouldNotOverwrite_WhenHeadersProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
        {
            req.Headers.TryAddWithoutValidation(HeadersConstants.SystemId, "10");
            req.Headers.TryAddWithoutValidation(HeadersConstants.BusinessId, "20");
            req.Headers.TryAddWithoutValidation(HeadersConstants.CountryCode, "UA");
            req.Headers.TryAddWithoutValidation(HeadersConstants.LanguageCode, "RU");
            req.Headers.TryAddWithoutValidation(HeadersConstants.CurrencyCode, "RUB");
        });
        ctx.SystemId.Should().Be(10);
        ctx.BusinessId.Should().Be(20);
        ctx.CountryCode.Should().Be("UA");
        ctx.LanguageCode.Should().Be("RU");
        ctx.CurrencyCode.Should().Be("RUB");
    }

    [Test]
    public async Task ShouldSetCountryCodeToUs_WhenCountryDefaultMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LanguageDefault"] = "EN",
                ["CurrencyDefault"] = "USD",
                ["CSPFrameAncestors"] = "'self'"
            })
            .Build();

        using var host = new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    services.AddSingleton<IConfiguration>(config);
                    services.AddHeaders(config);
                });
                web.Configure(Pipeline);
            })
            .Build();

        var ctx = await GetContextAsync(host);
        ctx.CountryCode.Should().Be("US");
    }
}
