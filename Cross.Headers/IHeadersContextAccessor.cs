using Cross.Headers.Enums;

namespace Cross.Headers;

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

    IReadOnlyCollection<string> UserPermissions { get; set; }

    IReadOnlyCollection<string> UserScopes { get; set; }

    bool IsUser { get; set; }
}
