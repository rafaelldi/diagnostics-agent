using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgent.Counters.Producer;

internal sealed class CounterCollection
{
    private readonly Dictionary<string, string[]?> _counters;

    internal CounterCollection()
    {
        _counters = new Dictionary<string, string[]?>
        {
            [SystemRuntimeProvider] = null
        };
    }

    internal CounterCollection(string counterProviderCollectionString)
    {
        _counters = CounterCollectionParser.Parse(counterProviderCollectionString.AsSpan());
    }

    internal IReadOnlyCollection<string> Providers() => _counters.Keys;

    internal int Count() => _counters.Count;

    internal bool Contains(string provider, string counter)
    {
        if (!_counters.TryGetValue(provider, out var counters))
        {
            return false;
        }

        return counters is null || counters.Contains(counter);
    }
}