namespace Cross.Headers.Middleware;

public class DefaultHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DefaultHeadersMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public DefaultHeadersMiddleware(
        RequestDelegate next,
        ILogger<DefaultHeadersMiddleware> logger,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configuration);

        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public async Task InvokeAsync(HttpContext httpContext, IHeadersContextAccessor headersContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(headersContextAccessor);

        if (headersContextAccessor.SystemId == 0)
        {
            _logger.LogTrace("SystemId not set, setting to 1");
            headersContextAccessor.SystemId = 1;
        }

        if (headersContextAccessor.BusinessId == 0)
        {
            _logger.LogTrace("BusinessId not set, setting to 1");
            headersContextAccessor.BusinessId = 1;
        }

        if (headersContextAccessor.LanguageCode == null)
        {
            var languageDefault = _configuration.GetValue<string>("LanguageDefault") ?? "EN";
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogTrace($"LanguageCode not set, setting to {languageDefault}");
            headersContextAccessor.LanguageCode = languageDefault;
        }

        if (headersContextAccessor.CurrencyCode == null)
        {
            var currencyDefault = _configuration.GetValue<string>("CurrencyDefault") ?? "USD";
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogTrace($"CurrencyCode not set, setting to {currencyDefault}");
            headersContextAccessor.CurrencyCode = currencyDefault;
        }

        await _next(httpContext);
    }
}
