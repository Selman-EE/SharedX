namespace SharedX.Abstractions.GatewayScopes;

public sealed class InternalScopeResolverOptions
{
    public string CacheKeyPrefix { get; init; } = "luna:internalscopes:";
    public TimeSpan CacheTtl { get; init; } = TimeSpan.FromSeconds(30); // short TTL
}