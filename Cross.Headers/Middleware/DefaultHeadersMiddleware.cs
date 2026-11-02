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

        if (string.IsNullOrWhiteSpace(headersContextAccessor.CountryCode))
        {
            var countryDefault = ResolveDefault(_configuration, "CountryDefault", "US");
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogTrace("CountryCode not set, setting to {CountryDefault}", countryDefault);
            headersContextAccessor.CountryCode = countryDefault;
        }

        if (string.IsNullOrWhiteSpace(headersContextAccessor.LanguageCode))
        {
            var languageDefault = ResolveDefault(_configuration, "LanguageDefault", "EN");
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogTrace("LanguageCode not set, setting to {LanguageDefault}", languageDefault);
            headersContextAccessor.LanguageCode = languageDefault;
        }

        if (string.IsNullOrWhiteSpace(headersContextAccessor.CurrencyCode))
        {
            var currencyDefault = ResolveDefault(_configuration, "CurrencyDefault", "USD");
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogTrace("CurrencyCode not set, setting to {CurrencyDefault}", currencyDefault);
            headersContextAccessor.CurrencyCode = currencyDefault;
        }

        await _next(httpContext);
    }

    private static string ResolveDefault(IConfiguration configuration, string key, string fallback)
    {
        var value = configuration.GetValue<string>(key);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
