using DiagnosticsAgent.EventPipes;
using Microsoft.Diagnostics.NETCore.Client;

namespace DiagnosticsAgent.Counters.Producer;

internal sealed class CounterProducerConfiguration
{
    internal string SessionId { get; }
    internal int RefreshInterval { get; }
    internal bool IsMetricsEnabled { get; }
    private IReadOnlyCollection<EventPipeProvider> EventPipeProviders { get; }
    private readonly CounterCollection _counters;
    private readonly MetricCollection? _metrics;

    internal CounterProducerConfiguration(
        string sessionId,
        string listOfCounters,
        string? listOfMetrics,
        int refreshInterval,
        int maxTimeSeries,
        int maxHistograms)
    {
        SessionId = sessionId;
        RefreshInterval = refreshInterval;

        if (string.IsNullOrEmpty(listOfCounters) && string.IsNullOrEmpty(listOfMetrics))
        {
            _counters = new CounterCollection();
            EventPipeProviders = EventPipeProviderFactory.CreateCounterProviders(
                _counters.Providers(),
                refreshInterval
            );
        }
        else
        {
            _counters = new CounterCollection(listOfCounters);

            var eventPipeProviderCount = listOfMetrics is not null
                ? _counters.Count() + 1
                : _counters.Count();
            var eventPipeProviders = new List<EventPipeProvider>(eventPipeProviderCount);

            var counterProviders = EventPipeProviderFactory.CreateCounterProviders(
                _counters.Providers(),
                refreshInterval
            );
            eventPipeProviders.AddRange(counterProviders);

            if (listOfMetrics is not null)
            {
                _metrics = new MetricCollection(listOfMetrics);
                var metricProvider = EventPipeProviderFactory.CreateMetricProvider(
                    sessionId,
                    _metrics.Metrics,
                    refreshInterval,
                    maxTimeSeries,
                    maxHistograms
                );
                eventPipeProviders.Add(metricProvider);
                IsMetricsEnabled = true;
            }

            EventPipeProviders = eventPipeProviders.AsReadOnly();
        }
    }

    internal EventPipeSessionConfiguration GetSessionConfiguration() => new(EventPipeProviders, false);

    internal bool IsCounterEnabled(string provider, string counter) => _counters.Contains(provider, counter);

    internal bool IsMetricEnabled(string meter, string instrument) => _metrics?.Contains(meter, instrument) ?? false;
}