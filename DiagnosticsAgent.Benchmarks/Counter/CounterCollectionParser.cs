using BenchmarkDotNet.Attributes;

namespace DiagnosticsAgent.Benchmarks.Counter;

[MemoryDiagnoser]
public class CounterCollectionParser
{
    private const string CounterProviderCollection =
        "System.Runtime[cpu-usage,working-set,gc-heap-size],Microsoft.AspNetCore.Hosting[requests-per-second,total-requests]";

    [Benchmark]
    public Dictionary<string, string[]?> ParseByRegex()
    {
        return CounterCollectionParserRegex.Parse(CounterProviderCollection);
    }

    [Benchmark]
    public Dictionary<string, string[]?> ParseByFirst()
    {
        return CounterCollectionParserSpans.Parse(CounterProviderCollection);
    }
}