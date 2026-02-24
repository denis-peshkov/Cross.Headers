# Cross.Headers

## Install NuGet package

Install the package into your ASP.NET Core project:

```bash
dotnet add package Cross.Headers
```

## Services registration

Рекомендуемый способ регистрации — использовать расширение `AddHeaders`:

```csharp
builder.Services.AddHeaders(builder.Configuration);
```

Метод `AddHeaders` регистрирует `IHeadersContextAccessor` как scoped-сервис и подготавливает инфраструктуру для работы мидлварей.

Также убедитесь, что вы подключаете аутентификацию OpenIdConnect и настраиваете значения `LanguageDefault`, `CurrencyDefault`, `CSPFrameAncestors` в конфигурации.

## Configure middleware

Добавьте мидлвари **строго в таком порядке**:

```csharp
app.UseMiddleware<DefaultHeadersMiddleware>(); // заполняет дефолтные значения в IHeadersContextAccessor
app.UseMiddleware<HeadersMiddleware>();        // читает основные данные из заголовков и формирует контекст
app.UseMiddleware<UserHeadersMiddleware>();    // парсит пользовательские данные после установки access token
```

`DefaultHeadersMiddleware` отвечает за то, чтобы при отсутствии части заголовков `IHeadersContextAccessor` был заполнен корректными дефолтными значениями (SystemId, BusinessId, LanguageCode, CurrencyCode и т.п.).

`HeadersMiddleware` читает технические заголовки (correlation id, system/business id, язык, валюту, user-agent, access token и др.), формирует основной контекст и добавляет технические заголовки в ответ (например, correlation id и CSP).

`UserHeadersMiddleware` использует уже заполненный access token/claims и достаёт из `HttpContext.User` пользовательские данные (Id, UserName, флаг IsUser, scopes и т.д.), записывая их в `IHeadersContextAccessor`.
