using System.Security.Claims;

namespace SharedX.Abstractions.GatewayScopes;

public interface ITenantPolicy
{
    /// <summary>
    /// Optionally reduce/transform scopes for tenant-specific rules.
    /// Return empty to deny.
    /// </summary>
    Task<string[]> ApplyAsync(
        string tenantId,
        InternalScopeRequest request,
        ClaimsPrincipal user,
        string[] scopes,
        CancellationToken ct);
}