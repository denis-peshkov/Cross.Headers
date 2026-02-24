# Boilerplate.Headers

## Configure HeadersMiddleware


Have to add a new one row into `Startup.cs` after the `ErrorHandlerMiddleware`:

```csharp
    app.UseMiddleware<HeadersMiddleware>();
```

In case you would like to custom middleware, have to install these packages from the console using the dotnet add package command.

Example for your packages:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect --version 7.0.20
dotnet add package Microsoft.Extensions.Configuration.Binder --version 7.0.4
```

Than, create a file `HeadersMiddleware.cs` in the project:

```csharp
public class HeadersMiddleware
{
    private static string CorrelationId => "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly string[] _whiteList;

    public HeadersMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _whiteList = configuration.GetValue<string>("CSPFrameAncestors").Split(",").ToArray();
    }

    public async Task InvokeAsync(HttpContext httpContext, IHeadersContextAccessor headersContextAccessor, IConfiguration configuration)
    {
        var headers = GetAllHeadersLowerCase(httpContext);

        var correlationId = GetFirstHeaderValueOrDefault<string>(headers, CorrelationId);
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            httpContext.Request.Headers.Append(CorrelationId, correlationId);
        }

        if (headersContextAccessor != null)
        {
            headersContextAccessor.CorrelationId = Guid.Parse(correlationId);
            Guid.TryParse(httpContext.User.FindFirstValue("sub") ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            headersContextAccessor.UserId = userId;
            headersContextAccessor.UserName = httpContext.User.FindFirstValue("username");
            headersContextAccessor.IsUser = httpContext.User.Claims.Any(x => x.Type == "username");
            var authorization = GetFirstHeaderValueOrDefault<string>(headers, "Authorization");
            if (!string.IsNullOrEmpty(authorization))
            {
                headersContextAccessor.UserAccessToken = authorization.Replace("Bearer ", "");
            }
            if (headersContextAccessor.IsUser)
            {
                //headersContextAccessor.UserPermissions = (await iamClient.GetUserInfoAsync(CancellationToken.None)).Content?.Policies;
            }
            headersContextAccessor.UserScopes = httpContext.User.Claims.Where(x => x.Type == "scope").Select(x => x.Value).FirstOrDefault()?.Split(" ");
            // headersContextAccessor.CustomerId = int.TryParse(GetFirstHeaderValueOrDefault<string>(headers, CustomerId), out var customerId) ? customerId : int.MaxValue;
            headersContextAccessor.LanguageCode = configuration.GetValue<string>("LanguageDefault") ?? "EN";
            headersContextAccessor.CurrencyCode = configuration.GetValue<string>("CurrencyDefault") ?? "USD";
        }

        httpContext.Response.OnStarting(() =>
        {
            if (!httpContext.Response.Headers.ContainsKey(CorrelationId))
            {
                httpContext.Response.Headers.Append(CorrelationId, correlationId);
            }
            return Task.CompletedTask;
        });

        httpContext.Response.Headers.Add("Content-Security-Policy", $"frame-ancestors {string.Join(" ",_whiteList)}");

        await _next.Invoke(httpContext);
    }

    private static T? GetFirstHeaderValueOrDefault<T>(IReadOnlyDictionary<string, StringValues> headers, string headerKey)
    {
        var toReturn = default(T);

        if (!headers.TryGetValue(headerKey.ToLower(), out var headerValues))
        {
            return toReturn;
        }

        var valueString = headerValues.FirstOrDefault();
        if (valueString != null)
        {
            toReturn = (T)Convert.ChangeType(valueString, typeof(T));
        }

        return toReturn;
    }

    private static Dictionary<string, StringValues> GetAllHeadersLowerCase(HttpContext httpContext)
        => httpContext.Request.Headers
            .Select(x => x)
            .ToDictionary(x => x.Key.ToLower(), x => x.Value);
}
```
