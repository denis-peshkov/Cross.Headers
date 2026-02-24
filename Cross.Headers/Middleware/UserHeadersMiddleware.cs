namespace Cross.Headers.Middleware;

public class UserHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserHeadersMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public UserHeadersMiddleware(
        RequestDelegate next,
        ILogger<UserHeadersMiddleware> logger,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configuration);

        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext httpContext, IHeadersContextAccessor headersContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(headersContextAccessor);

        Guid.TryParse(httpContext.User.FindFirstValue("sub") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
        headersContextAccessor.UserId = userId;
        // headersContextAccessor.UserId = !string.IsNullOrEmpty(httpContext.User.FindFirstValue("sub"))
        //     ? Guid.Parse(httpContext.User.FindFirstValue("sub"))
        //     : string.IsNullOrEmpty(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
        //         ? Guid.Parse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
        //         : Guid.Empty;
        headersContextAccessor.UserName = httpContext.User.FindFirstValue("username");
        headersContextAccessor.IsUser = httpContext.User.Claims.Any(x => x.Type == "username");
        // headersContextAccessor.IsUser = GetFirstHeaderValueOrDefault<bool>(headers, HeadersConstants.Authorization);
        // headersContextAccessor.UserPermissions = GetFirstHeaderValueOrDefault<IReadOnlyCollection<string>>(headers, HeadersConstants.Authorization);
        // headersContextAccessor.UserScopes = GetFirstHeaderValueOrDefault<IReadOnlyCollection<string>>(headers, HeadersConstants.Authorization);

        if (headersContextAccessor.IsUser)
        {
            //headersContextAccessor.UserPermissions = (await iamClient.GetUserInfoAsync(CancellationToken.None)).Content?.Policies;
        }
        headersContextAccessor.UserScopes = httpContext.User.Claims.Where(x => x.Type == "scope").Select(x => x.Value).FirstOrDefault()?.Split(" ");

        await _next.Invoke(httpContext);
    }
}
