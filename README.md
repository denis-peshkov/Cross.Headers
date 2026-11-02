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

**Target frameworks:** `net6.0`, `net7.0`, `net8.0`, `net9.0`, `net10.0`

## Features

- **`AddHeaders`** — registers `IHeadersContextAccessor` as scoped.
- **`HeadersMiddleware`** — reads technical request headers into context and writes correlation id + CSP to the response.
- **`DefaultHeadersMiddleware`** — applies defaults when headers are missing (`SystemId`, `BusinessId`, `CountryCode`, `LanguageCode`, `CurrencyCode`).
- **`UserHeadersMiddleware`** — fills user id/name/scopes/claims from `HttpContext.User` (after authentication).
- **`IHeadersContextAccessor.MapFrom`** — copies mapped context fields between accessors (including `UserClaims`).

## Install NuGet package

Install the _Cross.Headers_ [NuGet package](https://www.nuget.org/packages/Cross.Headers/) into your .NET project:

```powershell
Install-Package Cross.Headers
```
or
```bash
dotnet add package Cross.Headers
```

## Request headers

| Header | Context property | Notes |
| --- | --- | --- |
| `X-Correlation-Id` | `CorrelationId` | Generated when missing or invalid |
| `X-System-Id` | `SystemId` | Default `1` |
| `X-Business-Id` | `BusinessId` | Default `1` |
| `X-Country-Code` | `CountryCode` | ISO 3166-1 alpha-2 (e.g. `UA`, `US`); default from `CountryDefault` or `US` |
| `X-Language-Code` | `LanguageCode` | ISO 639-1 (e.g. `EN`, `RU`, `UK`); Ukrainian language = `UK`, not `UA` |
| `X-Currency-Code` | `CurrencyCode` | ISO 4217 (e.g. `EUR`, `USD`); default from `CurrencyDefault` or `USD` |
| `User-Agent` | `UserAgent` / `UserAgentKind` | Detects Android/iOS app markers and mobile browsers |
| `Authorization` | `UserAccessToken` | Optional `Bearer ` prefix is stripped |

## IHeadersContextAccessor

`IHeadersContextAccessor` is registered as a scoped service. Inject it via DI anywhere in your application (controllers, services, middleware) to access request-scoped context populated by the middleware pipeline. Within a single HTTP request, the same instance is shared across all consumers.

```csharp
public interface IHeadersContextAccessor
{
    Guid CorrelationId { get; set; }
    int SystemId { get; set; }
    int BusinessId { get; set; }
    string? CountryCode { get; set; }
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

```csharp
builder.Services.AddHeaders(builder.Configuration);
```

`AddHeaders` registers `IHeadersContextAccessor` as a scoped service.

Also configure OpenIdConnect authentication and set defaults in configuration:

```json
{
  "CountryDefault": "US",
  "LanguageDefault": "EN",
  "CurrencyDefault": "USD",
  "CSPFrameAncestors": "'self'"
}
```

### Configure middleware

Add the middleware **in this exact order**:

```csharp
// CORS MUST be before middleware that reads headers, so preflight (OPTIONS) requests are handled correctly
app.UseCors("UI");

// If your middleware reads headers and builds context — place it AFTER CORS, otherwise it will receive preflight requests
app.UseMiddleware<HeadersMiddleware>();
app.UseMiddleware<DefaultHeadersMiddleware>();

app.UseAuthentication();  // should be before UseMvc() / MapControllers()
app.UseMiddleware<UserHeadersMiddleware>();
app.UseAuthorization();   // should be before UseMvc() / MapControllers()
```

**Cross.Headers middleware:**

- `HeadersMiddleware` — reads technical headers (correlation id, system/business/country id, language, currency, user-agent, access token, etc.), builds the main context, and adds technical headers to the response (correlation id and CSP).
- `DefaultHeadersMiddleware` — fills default values in `IHeadersContextAccessor` when headers are missing (`SystemId`, `BusinessId`, `CountryCode`, `LanguageCode`, `CurrencyCode`).
- `UserHeadersMiddleware` — extracts user data from `HttpContext.User` (`UserId`, `UserName`, `IsUser`, scopes/permissions, claims) into `IHeadersContextAccessor`.

## Release notes

See [ReleaseNotes.txt](ReleaseNotes.txt) and [GitHub Releases](https://github.com/denis-peshkov/Cross.Headers/releases).

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/your-change`).
3. Commit your changes with a clear message.
4. Push the branch and open a pull request.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
