using System.Text.RegularExpressions;

namespace SharedX.Abstractions.GatewayScopes;

public sealed partial class DefaultPathNormalizer : IPathNormalizer
{
    private const string GuidRxPattern = @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";

    private const string NumSegRxPattern = @"/\d+(?=/|$)";

    [GeneratedRegex(GuidRxPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GuidRx();

    [GeneratedRegex(NumSegRxPattern, RegexOptions.Compiled)]
    private static partial Regex NumSegRx();

    public string Normalize(string path)
    {
        var p = (path ?? "/").Trim();
        if (!p.StartsWith('/')) p = $"/{p}";

        p = p.Split('?', '#')[0];
        p = p.TrimEnd('/');
        if (p.Length == 0) p = "/";

        p = p.ToLowerInvariant();
        p = GuidRx().Replace(p, "{id}");
        p = NumSegRx().Replace(p, "/{id}");

        return p;
    }
}