namespace Cross.Headers.Tests.Middleware;

public class HeadersMiddlewareTests : MiddlewareTestBase
{
    private static void Pipeline(IApplicationBuilder app)
    {
        app.UseMiddleware<HeadersMiddleware>();
        app.Run(ContextEndpoint);
    }

    private static Task ContextEndpoint(HttpContext ctx)
    {
        var accessor = ctx.RequestServices.GetRequiredService<IHeadersContextAccessor>();
        return ctx.Response.WriteAsJsonAsync(new
        {
            accessor.CorrelationId,
            accessor.SystemId,
            accessor.BusinessId,
            accessor.LanguageCode,
            accessor.CurrencyCode,
            accessor.UserAgent,
            accessor.UserAgentKind,
            accessor.UserAccessToken
        });
    }

    [Test]
    public async Task ShouldGenerateCorrelationId_WhenNotProvided()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.CorrelationId.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task ShouldUseCorrelationId_WhenProvided()
    {
        var expected = Guid.NewGuid();
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation(HeadersConstants.CorrelationId, expected.ToString()));
        ctx.CorrelationId.Should().Be(expected);
    }

    [Test]
    public async Task ShouldReadAllHeaders_FromRequest()
    {
        var correlationId = Guid.NewGuid();
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
        {
            req.Headers.TryAddWithoutValidation(HeadersConstants.CorrelationId, correlationId.ToString());
            req.Headers.TryAddWithoutValidation(HeadersConstants.SystemId, "42");
            req.Headers.TryAddWithoutValidation(HeadersConstants.BusinessId, "100");
            req.Headers.TryAddWithoutValidation(HeadersConstants.LanguageCode, "RU");
            req.Headers.TryAddWithoutValidation(HeadersConstants.CurrencyCode, "RUB");
        });
        ctx.CorrelationId.Should().Be(correlationId);
        ctx.SystemId.Should().Be(42);
        ctx.BusinessId.Should().Be(100);
        ctx.LanguageCode.Should().Be("RU");
        ctx.CurrencyCode.Should().Be("RUB");
    }

    [Test]
    public async Task ShouldReturnDefault_WhenHeaderValueIsInvalid()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
        {
            req.Headers.TryAddWithoutValidation(HeadersConstants.SystemId, "invalid");
            req.Headers.TryAddWithoutValidation(HeadersConstants.BusinessId, "not-a-number");
        });
        ctx.SystemId.Should().Be(0);
        ctx.BusinessId.Should().Be(0);
    }

    [Test]
    public async Task ShouldSetUserAgentKind_DesktopBrowser_WhenUserAgentMissing()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextViaSendAsync(host);
        ctx.UserAgentKind.Should().Be((int)UserAgentKindEnum.DesktopBrowser);
    }

    [Test]
    public async Task ShouldSetUserAgentKind_DesktopBrowser()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"));
        ctx.UserAgentKind.Should().Be((int)UserAgentKindEnum.DesktopBrowser);
    }

    [Test]
    public async Task ShouldSetUserAgentKind_Android()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("User-Agent", HeadersConstants.AndroidApp));
        ctx.UserAgentKind.Should().Be((int)UserAgentKindEnum.Android);
    }

    [Test]
    public async Task ShouldSetUserAgentKind_Apple()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("User-Agent", HeadersConstants.IosApp));
        ctx.UserAgentKind.Should().Be((int)UserAgentKindEnum.Apple);
    }

    [Test]
    public async Task ShouldSetUserAgentKind_MobileBrowser_WhenMobileCheckMatches()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 10; Mobile) Firefox/68.0"));
        ctx.UserAgentKind.Should().Be((int)UserAgentKindEnum.MobileBrowser);
    }

    [Test]
    public async Task ShouldSetUserAgentKind_MobileBrowser_WhenMobileVersionCheckMatches()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("User-Agent", "1207MobileBrowser/1.0"));
        ctx.UserAgentKind.Should().Be((int)UserAgentKindEnum.MobileBrowser);
    }

    [Test]
    public async Task ShouldStripBearerPrefix_FromUserAccessToken()
    {
        const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test";
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextFromRequestAsync(host, r => r.AddHeader(HeadersConstants.Authorization, "Bearer " + token));
        ctx.UserAccessToken.Should().Be(token);
    }

    [Test]
    public async Task ShouldAddCorrelationIdToResponse()
    {
        var expected = Guid.NewGuid();
        using var host = CreateHost(Pipeline);
        var response = await SendRequestAsync(host, r => r.AddHeader(HeadersConstants.CorrelationId, expected.ToString()));
        response.Headers.TryGetValues(HeadersConstants.CorrelationId, out var values).Should().BeTrue();
        values!.FirstOrDefault().Should().Be(expected.ToString());
    }

    [Test]
    public async Task ShouldAddCspHeader_WhenConfigured()
    {
        using var host = CreateHost(Pipeline);
        var response = await SendRequestAsync(host);
        response.Headers.TryGetValues("Content-Security-Policy", out var values).Should().BeTrue();
        values!.FirstOrDefault().Should().Contain("frame-ancestors");
    }
}
