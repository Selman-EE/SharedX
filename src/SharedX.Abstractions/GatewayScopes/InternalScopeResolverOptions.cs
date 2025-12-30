namespace SharedX.Abstractions.GatewayScopes;

public sealed class InternalScopeResolverOptions
{
    public string CacheKeyPrefix { get; set; } = "luna:internalscopes:";
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromSeconds(30);
}