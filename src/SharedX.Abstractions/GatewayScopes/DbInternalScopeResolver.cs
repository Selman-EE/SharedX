using System.Security.Claims;

namespace SharedX.Abstractions.GatewayScopes;

public sealed class DbInternalScopeResolver(IInternalPermissionStore store, ITenantPolicy tenantPolicy)
    : IInternalScopeResolver
{
    public async ValueTask<string[]> ResolveAsync(InternalScopeRequest request, ClaimsPrincipal user, CancellationToken ct)
    {
        // Deny-by-default
        var allowed = await store.GetAllowedScopesAsync(request, user, ct);
        if (allowed.Length == 0)
            return [];

        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            allowed = await tenantPolicy.ApplyAsync(request.TenantId!, request, user, allowed, ct);
            if (allowed.Length == 0)
                return [];
        }

        // normalize for consistency
        return allowed
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}