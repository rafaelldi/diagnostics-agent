using System.Text;

namespace DiagnosticsAgent.Counters.Producer;

internal sealed class MetricCollection
{
    private readonly Dictionary<string, string[]?> _metrics;
    internal string Metrics { get; }

    internal MetricCollection(string metricCollectionString)
    {
        _metrics = CounterCollectionParser.Parse(metricCollectionString.AsSpan());

        var sb = new StringBuilder();
        foreach (var meter in _metrics)
        {
            if (meter.Value is not null)
            {
                foreach (var metric in meter.Value)
                {
                    sb.Append($"{meter.Key}\\{metric},");
                }
            }
            else
            {
                sb.Append($"{meter.Key},");
            }
        }

        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }

        Metrics = sb.ToString();
    }

    internal bool Contains(string meter, string instrument)
    {
        if (!_metrics.TryGetValue(meter, out var metrics))
        {
            return false;
        }

        return metrics is null || metrics.Contains(instrument);
    }
}