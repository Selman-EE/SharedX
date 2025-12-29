using System.Security.Claims;

namespace SharedX.Abstractions.GatewayScopes;

public interface IInternalScopeResolver
{
    ValueTask<string[]> ResolveAsync(InternalScopeRequest request, ClaimsPrincipal user, CancellationToken ct);
}