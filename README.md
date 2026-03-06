# Cross.Headers [![Nuget](https://img.shields.io/nuget/v/Cross.Headers.svg)](https://nuget.org/packages/Cross.Headers/) [![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/denis-peshkov/Cross.Headers/wiki)

ASP.NET Core middleware boilerplate for handling correlation id, security headers (CSP), and user context based on configuration and OpenIdConnect authentication. Provides `IHeadersContextAccessor` as a scoped service for request-scoped access to correlation id, system/business ids, language, currency, user-agent, and authenticated user data.

**Supported frameworks:** .NET 7, .NET 8

## Install NuGet package

Install the package _Cross.Headers_ [NuGet package](https://www.nuget.org/packages/Cross.Headers/) into your ASP.NET Core project:

```powershell
Install-Package Cross.Headers
```
or
```bash
dotnet add package Cross.Headers
```

## Services registration

The recommended way to register services is to use the `AddHeaders` extension:

```csharp
builder.Services.AddHeaders(builder.Configuration);
```

The `AddHeaders` method registers `IHeadersContextAccessor` as a scoped service and prepares the infrastructure for the middleware pipeline.

Also ensure that you configure OpenIdConnect authentication and set the `LanguageDefault`, `CurrencyDefault`, and `CSPFrameAncestors` values in your configuration.

## Configure middleware

Add the middleware **in this exact order**:

```csharp
app.UseMiddleware<DefaultHeadersMiddleware>(); // fills default values in IHeadersContextAccessor
app.UseMiddleware<HeadersMiddleware>();        // reads main data from headers and builds the context
app.UseMiddleware<UserHeadersMiddleware>();    // parses user data after the access token is set
```

`DefaultHeadersMiddleware` ensures that when some headers are missing, `IHeadersContextAccessor` is populated with correct default values (SystemId, BusinessId, LanguageCode, CurrencyCode, etc.).

`HeadersMiddleware` reads technical headers (correlation id, system/business id, language, currency, user-agent, access token, etc.), builds the main context, and adds technical headers to the response (e.g., correlation id and CSP).

`UserHeadersMiddleware` uses the already populated access token/claims and extracts user data from `HttpContext.User` (Id, UserName, IsUser flag, scopes, etc.), writing it to `IHeadersContextAccessor`.
