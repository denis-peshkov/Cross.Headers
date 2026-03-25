[![License](https://img.shields.io/github/license/denis-peshkov/Cross.Headers)](LICENSE)
[![GitHub Release Date](https://img.shields.io/github/release-date/denis-peshkov/Cross.Headers?label=released)](https://github.com/denis-peshkov/Cross.Headers/releases)
[![NuGetVersion](https://img.shields.io/nuget/v/Cross.Headers.svg)](https://nuget.org/packages/Cross.Headers/)
[![NugetDownloads](https://img.shields.io/nuget/dt/Cross.Headers.svg)](https://nuget.org/packages/Cross.Headers/)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Cross.Headers&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Cross.Headers)
[![issues](https://img.shields.io/github/issues/denis-peshkov/Cross.Headers)](https://github.com/denis-peshkov/Cross.Headers/issues)
[![.NET PR](https://github.com/denis-peshkov/Cross.Headers/actions/workflows/dotnet.yml/badge.svg?event=pull_request)](https://github.com/denis-peshkov/Cross.Headers/actions/workflows/dotnet.yml)

![Size](https://img.shields.io/github/repo-size/denis-peshkov/Cross.Headers)
[![GitHub contributors](https://img.shields.io/github/contributors/denis-peshkov/Cross.Headers)](https://github.com/denis-peshkov/Cross.Headers/contributors)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/denis-peshkov/Cross.Headers/latest?label=new+commits)](https://github.com/denis-peshkov/Cross.Headers/commits/master)
![Activity](https://img.shields.io/github/commit-activity/w/denis-peshkov/Cross.Headers)
![Activity](https://img.shields.io/github/commit-activity/m/denis-peshkov/Cross.Headers)
![Activity](https://img.shields.io/github/commit-activity/y/denis-peshkov/Cross.Headers)

# Cross.Headers

ASP.NET Core middleware boilerplate for handling correlation id, security headers (CSP), and user context based on configuration and OpenIdConnect authentication. Provides `IHeadersContextAccessor` as a scoped service for request-scoped access to correlation id, system/business ids, language, currency, user-agent, and authenticated user data.

**Supported frameworks:** .NET 6, .NET 7, .NET 8, .NET 9, .NET 10

## Install NuGet package

Install the _Cross.Headers_ [NuGet package](https://www.nuget.org/packages/Cross.Headers/) into your .NET project:

```powershell
Install-Package Cross.Headers
```
or
```bash
dotnet add package Cross.Headers
```

## IHeadersContextAccessor

`IHeadersContextAccessor` is registered as a scoped service. Inject it via DI anywhere in your application (controllers, services, middleware) to access request-scoped context populated by the middleware pipeline. Within a single HTTP request, the same instance is shared across all consumers, so you get consistent correlation id, user data, and header-derived values for the entire request lifecycle.

```csharp
public interface IHeadersContextAccessor
{
    Guid CorrelationId { get; set; }
    int SystemId { get; set; }
    int BusinessId { get; set; }
    string? LanguageCode { get; set; }
    string? CurrencyCode { get; set; }
    string? UserAgent { get; set; }
    UserAgentKindEnum UserAgentKind { get; set; }
    Guid UserId { get; set; }
    string? UserName { get; set; }
    string? UserAccessToken { get; set; }
    IReadOnlyCollection<Claim> UserClaims { get; set; }
    IReadOnlyCollection<string> UserPermissions { get; set; }
    IReadOnlyCollection<string> UserScopes { get; set; }
    bool IsUser { get; set; }
}
```

## Library usage

### Services registration

The recommended way to register services is to use the `AddHeaders` extension:

```csharp
builder.Services.AddHeaders(builder.Configuration);
```

The `AddHeaders` method registers `IHeadersContextAccessor` as a scoped service and prepares the infrastructure for the middleware pipeline.

Also ensure that you configure OpenIdConnect authentication and set the `LanguageDefault`, `CurrencyDefault`, and `CSPFrameAncestors` values in your configuration.

### Configure middleware

Add the middleware **in this exact order**:

```csharp
// CORS MUST be before middleware that reads headers, so preflight (OPTIONS) requests are handled correctly
app.UseCors("UI");

// If your middleware reads headers and builds context — place it AFTER CORS, otherwise it will receive preflight requests
app.UseMiddleware<HeadersMiddleware>();
app.UseMiddleware<DefaultHeadersMiddleware>();

app.UseAuthentication();  // should be before UseMvc() call
app.UseMiddleware<UserHeadersMiddleware>();
app.UseAuthorization();   // should be before UseMvc() call
```

**Cross.Headers middleware:**

- `HeadersMiddleware` — reads technical headers (correlation id, system/business id, language, currency, user-agent, access token, etc.), builds the main context, and adds technical headers to the response (e.g., correlation id and CSP).
- `DefaultHeadersMiddleware` — fills default values in `IHeadersContextAccessor` when headers are missing (SystemId, BusinessId, LanguageCode, CurrencyCode, etc.).
- `UserHeadersMiddleware` — uses the already populated access token/claims and extracts user data from `HttpContext.User` (Id, UserName, IsUser flag, scopes, etc.), writing it to `IHeadersContextAccessor`.
