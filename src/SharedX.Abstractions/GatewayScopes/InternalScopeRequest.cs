namespace SharedX.Abstractions.GatewayScopes;

public sealed record InternalScopeRequest(
    string RouteId,
    string Audience, // target service: service-name
    string HttpMethod, // GET/POST/...
    string Path, // downstream path (no query)
    string? TenantId)
{
    public string HttpMethod { get; init; } = HttpMethod.ToUpperInvariant();
}