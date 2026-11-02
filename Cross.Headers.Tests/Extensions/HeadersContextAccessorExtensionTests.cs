namespace Cross.Headers.Tests.Extensions;

public class HeadersContextAccessorExtensionTests
{
    [Test]
    public void MapFrom_ShouldCopyAllMappedProperties()
    {
        var src = new HeadersContextAccessor
        {
            CorrelationId = Guid.NewGuid(),
            SystemId = 10,
            BusinessId = 20,
            CountryCode = "UA",
            LanguageCode = "UK",
            CurrencyCode = "UAH",
            UserAgent = "iosApp",
            UserAgentKind = UserAgentKindEnum.Apple,
            UserId = Guid.NewGuid(),
            UserName = "john",
            UserAccessToken = "token",
            UserClaims = new[] { new Claim("scope", "read"), new Claim("username", "john") },
            UserPermissions = new[] { "p1" },
            UserScopes = new[] { "s1", "s2" },
            IsUser = true
        };
        var dest = new HeadersContextAccessor();

        dest.MapFrom(src);

        dest.CorrelationId.Should().Be(src.CorrelationId);
        dest.SystemId.Should().Be(src.SystemId);
        dest.BusinessId.Should().Be(src.BusinessId);
        dest.CountryCode.Should().Be(src.CountryCode);
        dest.LanguageCode.Should().Be(src.LanguageCode);
        dest.CurrencyCode.Should().Be(src.CurrencyCode);
        dest.UserAgent.Should().Be(src.UserAgent);
        dest.UserAgentKind.Should().Be(src.UserAgentKind);
        dest.UserId.Should().Be(src.UserId);
        dest.UserName.Should().Be(src.UserName);
        dest.UserAccessToken.Should().Be(src.UserAccessToken);
        dest.UserClaims.Should().BeEquivalentTo(src.UserClaims);
        dest.UserPermissions.Should().BeEquivalentTo(src.UserPermissions);
        dest.UserScopes.Should().BeEquivalentTo(src.UserScopes);
        dest.IsUser.Should().Be(src.IsUser);
    }
}
