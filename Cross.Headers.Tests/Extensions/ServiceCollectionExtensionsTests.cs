namespace Cross.Headers.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddHeaders_ShouldRegisterHeadersContextAccessorAsScoped()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddHeaders(config);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var accessor = scope.ServiceProvider.GetService<IHeadersContextAccessor>();

        accessor.Should().NotBeNull();
        accessor.Should().BeOfType<HeadersContextAccessor>();
    }
}
