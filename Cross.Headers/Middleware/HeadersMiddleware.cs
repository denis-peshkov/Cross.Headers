using Cross.Headers.Constants;
using Cross.Headers.Enums;

namespace Cross.Headers.Middleware;

public class HeadersMiddleware
{
    private static readonly Regex _mobileCheck =
        new(HeadersConstants.MobileCheck, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex _mobileVersionCheck =
        new(HeadersConstants.MobileVersionCheck, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly RequestDelegate _next;
    private readonly ILogger<HeadersMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public HeadersMiddleware(
        RequestDelegate next,
        ILogger<HeadersMiddleware> logger,
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

        var headers = GetAllHeadersLowerCase(httpContext);

        var correlationId = GetFirstHeaderValueOrDefault<Guid>(headers, HeadersConstants.CorrelationId);
        if (correlationId == Guid.Empty)
        {
            correlationId = Guid.NewGuid();
            httpContext.Request.Headers[HeadersConstants.CorrelationId] = correlationId.ToString();
        }

        headersContextAccessor.CorrelationId = correlationId;
        headersContextAccessor.SystemId = GetFirstHeaderValueOrDefault<int>(headers, HeadersConstants.SystemId);
        headersContextAccessor.BusinessId = GetFirstHeaderValueOrDefault<int>(headers, HeadersConstants.BusinessId);
        headersContextAccessor.LanguageCode = GetFirstHeaderValueOrDefault<string>(headers, HeadersConstants.LanguageCode);
        headersContextAccessor.CurrencyCode = GetFirstHeaderValueOrDefault<string>(headers, HeadersConstants.CurrencyCode);
        headersContextAccessor.UserAgent = GetFirstHeaderValueOrDefault<string>(headers, HeadersConstants.UserAgent);
        headersContextAccessor.UserAgentKind = GetUserAgentKind(httpContext);
        headersContextAccessor.UserAccessToken = GetFirstHeaderValueOrDefault<string>(headers, HeadersConstants.Authorization);
        if (!string.IsNullOrEmpty(headersContextAccessor.UserAccessToken))
        {
            headersContextAccessor.UserAccessToken = headersContextAccessor.UserAccessToken.Replace("Bearer ", "");
        }

        httpContext.Response.OnStarting(() =>
        {
            if (!httpContext.Response.Headers.ContainsKey(HeadersConstants.CorrelationId))
            {
                httpContext.Response.Headers.Add(HeadersConstants.CorrelationId, correlationId.ToString());
            }

            var whiteList = _configuration.GetValue<string>("CSPFrameAncestors")?.Split(",");
            if (whiteList is { Length: > 0 })
            {
                httpContext.Response.Headers.Add("Content-Security-Policy", $"frame-ancestors {string.Join(" ", whiteList)}");
            }

            return Task.CompletedTask;
        });

        await _next.Invoke(httpContext);
    }

    private static T GetFirstHeaderValueOrDefault<T>(IReadOnlyDictionary<string, StringValues> headers, string headerKey)
    {
        var toReturn = default(T);

        if (!headers.TryGetValue(headerKey.ToLower(), out var headerValues))
        {
            return toReturn;
        }

        var valueString = headerValues.FirstOrDefault();
        if (valueString != null)
        {
            try
            {
                toReturn = (T)Convert.ChangeType(valueString, typeof(T));
            }
            catch
            {
                toReturn = default;
            }
        }

        return toReturn;
    }

    private static Dictionary<string, StringValues> GetAllHeadersLowerCase(HttpContext httpContext)
        => httpContext.Request.Headers
            .Select(x => x)
            .ToDictionary(x => x.Key.ToLower(), x => x.Value);

    private static UserAgentKindEnum GetUserAgentKind(HttpContext httpContext)
    {
        var userAgents = httpContext?.Request.Headers.UserAgent;

        if (!userAgents.HasValue)
        {
            return UserAgentKindEnum.DesktopBrowser;
        }

        if (userAgents.Value.Any(x => string.Equals(x, HeadersConstants.AndroidApp, StringComparison.OrdinalIgnoreCase)))
        {
            return UserAgentKindEnum.Android;
        }

        if (userAgents.Value.Any(x => string.Equals(x, HeadersConstants.IosApp, StringComparison.OrdinalIgnoreCase)))
        {
            return UserAgentKindEnum.Apple;
        }

        var userAgent = userAgents.Value.FirstOrDefault();

        if (userAgent?.Length > 4 && (_mobileCheck.IsMatch(userAgent) || _mobileVersionCheck.IsMatch(userAgent[..4])))
        {
            return UserAgentKindEnum.MobileBrowser;
        }

        return UserAgentKindEnum.DesktopBrowser;
    }
}
