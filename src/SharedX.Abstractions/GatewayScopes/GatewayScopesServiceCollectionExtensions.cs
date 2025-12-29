using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZiggyCreatures.Caching.Fusion;

namespace SharedX.Abstractions.GatewayScopes;

public static class GatewayScopesServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayScopeResolution(
        this IServiceCollection services,
        Action<InternalScopeResolverOptions>? configure = null)
    {
        var opt = new InternalScopeResolverOptions();
        configure?.Invoke(opt);

        services.AddSingleton(opt);
        services.TryAddSingleton<IPathNormalizer, DefaultPathNormalizer>();

        // core resolver (DB + tenant policy)
        services.TryAddSingleton<ITenantPolicy, NoTenantPolicy>();
        services.TryAddSingleton<DbInternalScopeResolver>();

        // IMPORTANT: do NOT call AddFusionCache/AddRedis here.
        // Host app must register IFusionCache.
        services.TryAddSingleton<IInternalScopeResolver>(sp =>
            new CachedInternalScopeResolver(
                sp.GetRequiredService<IFusionCache>(),
                sp.GetRequiredService<DbInternalScopeResolver>(),
                sp.GetRequiredService<IPathNormalizer>(),
                sp.GetRequiredService<InternalScopeResolverOptions>()));

        return services;
    }
}