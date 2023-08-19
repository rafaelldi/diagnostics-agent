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
            Math.Round(value, 2),
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
        Math.Round(value, 2),
        CounterType.Metric,
        tags
    );
}