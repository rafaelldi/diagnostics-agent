namespace DiagnosticsAgent.Counters.Producer;

internal static class ValueCounterMappers
{
    internal static ValueCounter MapToRateCounter(
        DateTime timeStamp,
        string name,
        string? units,
        string providerName,
        double value,
        string? tags,
        int refreshInterval
    )
    {
        var displayUnits = string.IsNullOrEmpty(units) ? "Count" : units;
        return new ValueCounter(
            timeStamp,
            name,
            $"{name} ({displayUnits} / {refreshInterval} sec)",
            providerName,
            value,
            CounterType.Rate,
            tags
        );
    }

    internal static ValueCounter MapToMetricCounter(
        DateTime timeStamp,
        string name,
        string? units,
        string providerName,
        double value,
        string? tags
    ) => new(
        timeStamp,
        name,
        string.IsNullOrEmpty(units) ? name : $"{name} ({units})",
        providerName,
        value,
        CounterType.Metric,
        tags
    );
}