using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgent.Counters.Producer;

internal sealed class CounterProviderCollection
{
    private readonly Dictionary<string, List<string>?> _counterProviders;

    internal CounterProviderCollection()
    {
        _counterProviders = new Dictionary<string, List<string>?>
        {
            [SystemRuntimeProvider] = null
        };
    }

    internal CounterProviderCollection(string counterProviderCollectionString)
    {
        _counterProviders = CounterCollectionParser.Parse(counterProviderCollectionString.AsSpan());
    }

    internal IReadOnlyCollection<string> Providers() => _counterProviders.Keys;

    internal int Count() => _counterProviders.Count;

    internal bool Contains(string provider, string counter)
    {
        if (!_counterProviders.TryGetValue(provider, out var counters))
        {
            return false;
        }

        return counters is null || counters.Contains(counter);
    }
}