using System.Security.Claims;

namespace SharedX.Abstractions.GatewayScopes;

public sealed class NoTenantPolicy : ITenantPolicy
{
    public Task<string[]> ApplyAsync(string tenantId, InternalScopeRequest request, ClaimsPrincipal user, string[] scopes, CancellationToken ct)
        => Task.FromResult(scopes);
}