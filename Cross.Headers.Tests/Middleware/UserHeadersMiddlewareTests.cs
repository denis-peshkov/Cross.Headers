namespace Cross.Headers.Tests.Middleware;

public class UserHeadersMiddlewareTests : MiddlewareTestBase
{
    private static void Pipeline(IApplicationBuilder app)
    {
        app.UseMiddleware<HeadersMiddleware>();
        app.UseMiddleware<DefaultHeadersMiddleware>();
        app.Use(FakeUserMiddleware);
        app.UseMiddleware<UserHeadersMiddleware>();
        app.Run(ContextEndpoint);
    }

    private static Task FakeUserMiddleware(HttpContext ctx, RequestDelegate next)
    {
        if (ctx.Request.Headers.TryGetValue("X-Test-User", out var userHeader) && userHeader.Count > 0)
        {
            var parts = userHeader[0]!.Split('|');
            var claims = new List<System.Security.Claims.Claim>();
            if (parts.Length >= 1 && Guid.TryParse(parts[0], out var userId))
            {
                claims.Add(new System.Security.Claims.Claim("sub", userId.ToString()));
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()));
            }
            if (parts.Length >= 2)
                claims.Add(new System.Security.Claims.Claim("username", parts[1]));
            if (parts.Length >= 3)
            {
                claims.Add(new System.Security.Claims.Claim("scope", parts[2]));
                foreach (var scope in parts[2].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    claims.Add(new System.Security.Claims.Claim("permission", scope));
            }
            ctx.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims, "Test"));
        }
        return next(ctx);
    }

    private static Task ContextEndpoint(HttpContext ctx)
    {
        var accessor = ctx.RequestServices.GetRequiredService<IHeadersContextAccessor>();
        return ctx.Response.WriteAsJsonAsync(new
        {
            accessor.UserId,
            accessor.UserName,
            accessor.IsUser,
            UserScopes = accessor.UserScopes ?? Array.Empty<string>()
        });
    }

    [Test]
    public async Task ShouldSetUserId_FromUserClaims()
    {
        var userId = Guid.NewGuid();
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("X-Test-User", $"{userId}|testuser|read write"));
        ctx.UserId.Should().Be(userId);
    }

    [Test]
    public async Task ShouldSetUserName_FromUserClaims()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("X-Test-User", $"{Guid.NewGuid()}|john.doe|read"));
        ctx.UserName.Should().Be("john.doe");
    }

    [Test]
    public async Task ShouldSetIsUser_WhenUsernameClaimPresent()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("X-Test-User", $"{Guid.NewGuid()}|user|"));
        ctx.IsUser.Should().BeTrue();
    }

    [Test]
    public async Task ShouldSetIsUserFalse_WhenNoUser()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host);
        ctx.IsUser.Should().BeFalse();
    }

    [Test]
    public async Task ShouldSetUserScopes_FromUserClaims()
    {
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("X-Test-User", $"{Guid.NewGuid()}|user|read write admin"));
        ctx.UserScopes.Should().NotBeNull();
        ctx.UserScopes.Should().BeEquivalentTo("read", "write", "admin");
    }

    [Test]
    public async Task ShouldSetUserId_FromNameIdentifier_WhenSubMissing()
    {
        var userId = Guid.NewGuid();
        using var host = CreateHost(Pipeline);
        var ctx = await GetContextAsync(host, configureRequest: req =>
            req.Headers.TryAddWithoutValidation("X-Test-User", $"{userId}|testuser|"));
        ctx.UserId.Should().Be(userId);
    }
}
