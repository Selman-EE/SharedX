namespace SharedX.Abstractions.GatewayScopes;

public interface IPathNormalizer
{
    string Normalize(string path);
}