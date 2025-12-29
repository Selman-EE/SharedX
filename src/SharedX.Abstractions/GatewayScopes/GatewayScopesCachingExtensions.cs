using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace SharedX.Abstractions.GatewayScopes;

public static class GatewayScopesCachingExtensions
{
    public static IServiceCollection AddGatewayScopesFusionCacheWithRedis(
        this IServiceCollection services,
        IConfiguration configuration,
        string redisConnStringKey =
            "Redis:ConnectionString") // this must come from inside the API via secret or Vault
    {
        var cs = configuration[redisConnStringKey];
        ArgumentException.ThrowIfNullOrWhiteSpace(cs);

        services.AddStackExchangeRedisCache(o => o.Configuration = cs);
        services.AddFusionCache();

        return services;
    }

    // overload if you prefer passing conn string directly
    public static IServiceCollection AddGatewayScopesFusionCacheWithRedis(
        this IServiceCollection services,
        string redisConnectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(redisConnectionString);
        
        services.AddStackExchangeRedisCache(o => o.Configuration = redisConnectionString);
        services.AddFusionCache();

        return services;
    }
}