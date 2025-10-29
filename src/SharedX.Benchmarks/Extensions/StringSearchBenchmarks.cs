
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.Globalization;
using System.Text;

namespace SharedX.Benchmarks.Extensions;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringSearchBenchmarks
{
    private const string PolishText = "Łódź Kraków Gdańsk Wrocław Poznań";
    private const string MixedText = "Product: Łódź Wooden Chair #42 - 50% OFF!";
    private const string EnglishText = "London Berlin Paris Rome Madrid";
        
    [Benchmark(Baseline = true)]
    public string Baseline_ToLower_Polish()
    {
        // What developers typically do (WRONG for search)
        return PolishText.ToLowerInvariant();
    }
        
    [Benchmark]
    public string Naive_RemoveDiacritics_Polish()
    {
        // Naive implementation - creates many string objects
        return RemoveDiacriticsNaive(PolishText).ToLowerInvariant();
    }
        
    [Benchmark]
    public string Optimized_ToSearchable_Polish()
    {
        // Our optimized implementation (to be implemented)
        return ToSearchableOptimized(PolishText);
    }
        
    [Benchmark]
    public string Baseline_ToLower_Mixed()
    {
        return MixedText.ToLowerInvariant();
    }
        
    [Benchmark]
    public string Optimized_ToSearchable_Mixed()
    {
        return ToSearchableOptimized(MixedText);
    }
        
    [Benchmark]
    public string Baseline_ToLower_English()
    {
        return EnglishText.ToLowerInvariant();
    }
        
    [Benchmark]
    public string Optimized_ToSearchable_English()
    {
        return ToSearchableOptimized(EnglishText);
    }

    // Naive implementation (allocates many strings)
    private static string RemoveDiacriticsNaive(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    // Optimized implementation (Span-based, fewer allocations)
    private static string ToSearchableOptimized(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Allocate buffer once
        Span<char> buffer = stackalloc char[text.Length];
        var length = 0;
        var lastWasSpace = false;

        foreach (var c in text)
        {
            // Skip punctuation and special chars
            if (char.IsPunctuation(c) || char.IsSymbol(c))
            {
                if (!lastWasSpace && length > 0)
                {
                    buffer[length++] = ' ';
                    lastWasSpace = true;
                }
                continue;
            }

            // Handle whitespace
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && length > 0)
                {
                    buffer[length++] = ' ';
                    lastWasSpace = true;
                }
                continue;
            }

            // Normalize diacritics and lowercase
            var normalized = RemoveDiacriticChar(char.ToLowerInvariant(c));
            buffer[length++] = normalized;
            lastWasSpace = false;
        }

        // Trim trailing space
        if (length > 0 && buffer[length - 1] == ' ')
            length--;

        return new string(buffer.Slice(0, length));
    }

    // Fast character-level diacritic removal (Polish-focused)
    private static char RemoveDiacriticChar(char c)
    {
        return c switch
        {
            // Polish lowercase
            'ą' => 'a',
            'ć' => 'c',
            'ę' => 'e',
            'ł' => 'l',
            'ń' => 'n',
            'ó' => 'o',
            'ś' => 's',
            'ź' or 'ż' => 'z',
                
            // Polish uppercase (shouldn't hit after ToLower, but just in case)
            'Ą' => 'a',
            'Ć' => 'c',
            'Ę' => 'e',
            'Ł' => 'l',
            'Ń' => 'n',
            'Ó' => 'o',
            'Ś' => 's',
            'Ź' or 'Ż' => 'z',
                
            // Common European diacritics (for completeness)
            'à' or 'á' or 'â' or 'ã' or 'ä' or 'å' => 'a',
            'è' or 'é' or 'ê' or 'ë' => 'e',
            'ì' or 'í' or 'î' or 'ï' => 'i',
            'ò' or 'ó' or 'ô' or 'õ' or 'ö' => 'o',
            'ù' or 'ú' or 'û' or 'ü' => 'u',
            'ý' or 'ÿ' => 'y',
            'ñ' => 'n',
            'ç' => 'c',
                
            _ => c
        };
    }
}