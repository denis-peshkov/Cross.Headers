namespace Cross.Headers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IHeadersContextAccessor, HeadersContextAccessor>();

        return services;
    }
}
