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

        headersContextAccessor.UserId = Guid.TryParse(httpContext.User.FindFirstValue("sub") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : Guid.Empty;
        headersContextAccessor.UserName = httpContext.User.FindFirstValue("username");
        headersContextAccessor.IsUser = httpContext.User.Claims.Any(x => x.Type == "username");
        headersContextAccessor.UserClaims = httpContext.User.Claims.ToList();

        if (headersContextAccessor.IsUser)
        {
            // headersContextAccessor.UserPermissions = (await iamClient.GetUserInfoAsync(CancellationToken.None)).Content?.Policies;
            headersContextAccessor.UserScopes = headersContextAccessor.UserClaims
                .Where(x => x.Type == "permission")
                .Select(x => x.Value)
                .ToList();
        }
        else
        {
            headersContextAccessor.UserScopes = headersContextAccessor.UserClaims
                .Where(x => x.Type == "scope")
                .Select(x => x.Value)
                .FirstOrDefault()?
                .Split(" ")
                .ToList() ?? new List<string>();
        }

        await _next.Invoke(httpContext);
    }
}
