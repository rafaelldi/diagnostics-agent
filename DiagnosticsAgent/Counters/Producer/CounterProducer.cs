using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Counters.Producer.EventHandlers;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Counters.Producer;

internal sealed class CounterProducer : IValueProducer
{
    private readonly EventPipeSessionProvider _sessionProvider;
    private readonly CounterProducerConfiguration _configuration;
    private readonly CounterEventHandler _counterEventHandler;
    private readonly MetricEventHandler? _metricEventHandler;

    internal CounterProducer(
        int pid,
        CounterProducerConfiguration configuration,
        ChannelWriter<ValueCounter> writer,
        Lifetime lifetime)
    {
        _sessionProvider = new EventPipeSessionProvider(pid);
        _configuration = configuration;

        _counterEventHandler = new CounterEventHandler(
            pid,
            configuration.RefreshInterval,
            (provider, counter) => _configuration.IsCounterEnabled(provider, counter),
            writer
        );
        _metricEventHandler = _configuration.IsMetricsEnabled
            ? new MetricEventHandler(
                pid,
                configuration.RefreshInterval,
                configuration.SessionId,
                (meter, instrument) => _configuration.IsMetricEnabled(meter, instrument),
                writer
            )
            : null;

        lifetime.OnTermination(() => writer.Complete());
    }

    public async Task ProduceAsync()
    {
        var sessionConfiguration = _configuration.GetSessionConfiguration();
        await _sessionProvider.RunSessionAndSubscribeAsync(
            sessionConfiguration,
            Lifetime.AsyncLocal.Value,
            SubscribeToEvents
        );
    }

    private void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        _counterEventHandler.SubscribeToEvents(source, lifetime);
        _metricEventHandler?.SubscribeToEvents(source, lifetime);
    }
}