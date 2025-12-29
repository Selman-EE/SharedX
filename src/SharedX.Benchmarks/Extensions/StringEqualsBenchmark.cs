using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace SharedX.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringEqualsBenchmark
{
    private const string ShortString1 = "test";
    private const string ShortString2 = "TEST";

    private const string MediumString1 = "Łódź Wooden Chair";
    private const string MediumString2 = "ŁÓDŹ WOODEN CHAIR";

    private const string LongString1 =
        "This is a much longer string with more content to compare including Polish characters like Łódź and Kraków";

    private const string LongString2 =
        "THIS IS A MUCH LONGER STRING WITH MORE CONTENT TO COMPARE INCLUDING POLISH CHARACTERS LIKE ŁÓDŹ AND KRAKÓW";

    #region Short Strings (4 chars)

    [Benchmark(Baseline = true)]
    public bool Short_StringEquals()
    {
        return string.Equals(ShortString1, ShortString2, StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool Short_SpanEquals()
    {
        if (ShortString1 == null && ShortString2 == null) return true;
        if (ShortString1 == null || ShortString2 == null) return false;
        return ShortString1.AsSpan().Equals(ShortString2.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool Short_MemoryEquals()
    {
        if (ShortString1 == null && ShortString2 == null) return true;
        if (ShortString1 == null || ShortString2 == null) return false;
        return ShortString1.AsSpan().Equals(ShortString2.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Medium Strings (17 chars)

    [Benchmark]
    public bool Medium_StringEquals()
    {
        return string.Equals(MediumString1, MediumString2, StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool Medium_SpanEquals()
    {
        if (MediumString1 == null && MediumString2 == null) return true;
        if (MediumString1 == null || MediumString2 == null) return false;
        return MediumString1.AsSpan().Equals(MediumString2.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool Medium_MemoryEquals()
    {
        if (MediumString1 == null && MediumString2 == null) return true;
        if (MediumString1 == null || MediumString2 == null) return false;
        return MediumString1.AsSpan().Equals(MediumString2.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Long Strings (100+ chars)

    [Benchmark]
    public bool Long_StringEquals()
    {
        return string.Equals(LongString1, LongString2, StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool Long_SpanEquals()
    {
        if (LongString1 == null && LongString2 == null) return true;
        if (LongString1 == null || LongString2 == null) return false;
        return LongString1.AsSpan().Equals(LongString2.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool Long_MemoryEquals()
    {
        if (LongString1 == null && LongString2 == null) return true;
        if (LongString1 == null || LongString2 == null) return false;
        return LongString1.AsSpan().Equals(LongString2.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Different Lengths (Early Exit Test)

    [Benchmark]
    public bool DifferentLength_StringEquals()
    {
        return string.Equals("short", "much longer string", StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool DifferentLength_SpanEquals()
    {
        return "short".AsSpan().Equals("much longer string".AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region With Nulls

    [Benchmark]
    public bool WithNull_StringEquals()
    {
        string? a = null;
        var b = "test";
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    [Benchmark]
    public bool WithNull_SpanEquals()
    {
        string? a = null;
        var b = "test";

        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.AsSpan().Equals(b.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}