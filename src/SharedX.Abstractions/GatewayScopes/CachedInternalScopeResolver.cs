using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZiggyCreatures.Caching.Fusion;

namespace SharedX.Abstractions.GatewayScopes;

public sealed class CachedInternalScopeResolver(
    IFusionCache cache,
    IInternalScopeResolver inner,
    IPathNormalizer pathNormalizer,
    InternalScopeResolverOptions opt)
    : IInternalScopeResolver
{
    public ValueTask<string[]> ResolveAsync(InternalScopeRequest request, ClaimsPrincipal user, CancellationToken ct)
    {
        var normalizedPath = pathNormalizer.Normalize(request.Path);

        // If permissions are user-specific, include a stable user key
        // (sub or nameid). This prevents “user A allowed” being reused for “user B denied”.
        var userKey = GetUserKey(user);

        var cacheKey =
            $"{opt.CacheKeyPrefix}{request.TenantId ?? "_"}:{request.Audience}:{request.HttpMethod}:{normalizedPath}:{userKey}";

        return cache.GetOrSetAsync(
            cacheKey,
            async ctx => await inner.ResolveAsync(request with { Path = normalizedPath }, user, ctx),
            options => options.SetDuration(opt.CacheTtl),
            ct);

    }

    // TOOD: Later, move to role-based caching, we can replace userKey with a roles-hash.
    private static string GetUserKey(ClaimsPrincipal user)
    {
        var sub =
            user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "anon";
        
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sub));
        return Convert.ToHexString(bytes); 
    }
}