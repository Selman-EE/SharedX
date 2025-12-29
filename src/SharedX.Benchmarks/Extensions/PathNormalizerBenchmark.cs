using BenchmarkDotNet.Attributes;
using System.Buffers;
using System.Text.RegularExpressions;

namespace SharedX.Benchmarks.Extensions;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class PathNormalizerBenchmark
{
    private readonly OriginalPathNormalizer _original = new();
    private readonly OptimizedPathNormalizer _optimized = new();

    private const string ShortPath = "/api/users";
    private const string PathWithQuery = "/api/users?page=1&size=10";
    private const string PathWithGuid = "/api/users/550e8400-e29b-41d4-a716-446655440000";
    private const string PathWithNumber = "/api/orders/12345/items";
    private const string LongPath = "/api/v1/organizations/550e8400-e29b-41d4-a716-446655440000/departments/123/employees/456/details";
    private const string PathMixed = "/API/Users/550e8400-e29b-41d4-a716-446655440000/Orders/999?filter=active#top";

    [Benchmark(Baseline = true)]
    public string Original_ShortPath() => _original.Normalize(ShortPath);

    [Benchmark]
    public string Optimized_ShortPath() => _optimized.Normalize(ShortPath);

    [Benchmark]
    public string Original_WithQuery() => _original.Normalize(PathWithQuery);

    [Benchmark]
    public string Optimized_WithQuery() => _optimized.Normalize(PathWithQuery);

    [Benchmark]
    public string Original_WithGuid() => _original.Normalize(PathWithGuid);

    [Benchmark]
    public string Optimized_WithGuid() => _optimized.Normalize(PathWithGuid);

    [Benchmark]
    public string Original_WithNumber() => _original.Normalize(PathWithNumber);

    [Benchmark]
    public string Optimized_WithNumber() => _optimized.Normalize(PathWithNumber);

    [Benchmark]
    public string Original_LongPath() => _original.Normalize(LongPath);

    [Benchmark]
    public string Optimized_LongPath() => _optimized.Normalize(LongPath);

    [Benchmark]
    public string Original_MixedCase() => _original.Normalize(PathMixed);

    [Benchmark]
    public string Optimized_MixedCase() => _optimized.Normalize(PathMixed);
}

// ===== ORIGINAL IMPLEMENTATION =====
public sealed partial class OriginalPathNormalizer
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

// ===== OPTIMIZED IMPLEMENTATION =====
public sealed partial class OptimizedPathNormalizer
{
    private const string GuidRxPattern = @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";
    private const string NumSegRxPattern = @"/\d+(?=/|$)";

    [GeneratedRegex(GuidRxPattern, RegexOptions.IgnoreCase)]
    private static partial Regex GuidRx();

    [GeneratedRegex(NumSegRxPattern)]
    private static partial Regex NumSegRx();

    public string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        ReadOnlySpan<char> span = path.AsSpan().Trim();
        
        int queryIndex = span.IndexOfAny('?', '#');
        if (queryIndex >= 0)
            span = span[..queryIndex];

        if (span.IsEmpty)
            return "/";

        int maxLength = span.Length + 1;
        char[]? rentedBuffer = null;
        Span<char> buffer = maxLength <= 256 
            ? stackalloc char[maxLength] 
            : (rentedBuffer = ArrayPool<char>.Shared.Rent(maxLength));

        try
        {
            int pos = 0;

            // Add leading slash if needed
            if (span[0] != '/')
            {
                buffer[pos++] = '/';
            }

            // Copy and lowercase
            for (int i = 0; i < span.Length; i++)
            {
                buffer[pos++] = char.ToLowerInvariant(span[i]);
            }

            // Remove trailing slash (except root)
            if (pos > 1 && buffer[pos - 1] == '/')
                pos--;

            string result = new string(buffer[..pos]);

            // Apply regex replacements
            result = GuidRx().Replace(result, "{id}");
            result = NumSegRx().Replace(result, "/{id}");

            return result;
        }
        finally
        {
            if (rentedBuffer != null)
                ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }
}