using System.Security.Claims;

namespace SharedX.Abstractions.GatewayScopes;

public interface IInternalPermissionStore
{
    /// <summary>
    /// Returns the scopes the current user is allowed to have for a specific downstream request.
    /// Deny-by-default: return empty if no match with 403 
    /// </summary>
    Task<string[]> GetAllowedScopesAsync(
        InternalScopeRequest request,
        ClaimsPrincipal user,
        CancellationToken ct);
}