using Cross.Headers.Enums;

namespace Cross.Headers;

public class HeadersContextAccessor : IHeadersContextAccessor
{
    public Guid CorrelationId { get; set; }

    public int SystemId { get; set; }

    public int BusinessId { get; set; }

    public string? LanguageCode { get; set; }

    public string? CurrencyCode { get; set; }

    public string? UserAgent { get; set; }

    public UserAgentKindEnum UserAgentKind { get; set; }

    public Guid UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserAccessToken { get; set; }

    public IReadOnlyCollection<string> UserPermissions { get; set; } = Array.Empty<string>();

    public IReadOnlyCollection<string> UserScopes { get; set; } = Array.Empty<string>();

    public bool IsUser { get; set; }
}
