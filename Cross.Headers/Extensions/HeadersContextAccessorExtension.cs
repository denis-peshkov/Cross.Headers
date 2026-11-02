namespace Cross.Headers.Extensions;

public static class HeadersContextAccessorExtension
{
    public static void MapFrom(this IHeadersContextAccessor dest, IHeadersContextAccessor src)
    {
        dest.CorrelationId = src.CorrelationId;
        dest.SystemId = src.SystemId;
        dest.BusinessId = src.BusinessId;
        dest.CountryCode = src.CountryCode;
        dest.LanguageCode = src.LanguageCode;
        dest.CurrencyCode = src.CurrencyCode;
        dest.UserAgent = src.UserAgent;
        dest.UserAgentKind = src.UserAgentKind;
        dest.UserId = src.UserId;
        dest.UserName = src.UserName;
        dest.UserAccessToken = src.UserAccessToken;
        dest.UserClaims = src.UserClaims;
        dest.UserPermissions = src.UserPermissions;
        dest.UserScopes = src.UserScopes;
        dest.IsUser = src.IsUser;
    }
}
