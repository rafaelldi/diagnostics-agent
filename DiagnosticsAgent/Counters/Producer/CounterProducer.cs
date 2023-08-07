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
    private readonly EventPipeSessionConfiguration _sessionConfiguration;
    private readonly CounterEventHandler _counterEventHandler;
    private readonly MetricEventHandler? _metricEventHandler;

    internal CounterProducer(
        int pid,
        CounterProducerConfiguration configuration,
        ChannelWriter<ValueCounter> writer,
        Lifetime lifetime)
    {
        _sessionProvider = new EventPipeSessionProvider(pid);
        _sessionConfiguration = configuration.GetSessionConfiguration();

        _counterEventHandler = new CounterEventHandler(
            pid,
            configuration.RefreshInterval,
            configuration.IsCounterEnabled,
            writer
        );
        _metricEventHandler = configuration.IsMetricsEnabled
            ? new MetricEventHandler(
                pid,
                configuration.RefreshInterval,
                configuration.SessionId,
                configuration.IsMetricEnabled,
                writer
            )
            : null;

        lifetime.OnTermination(() => writer.Complete());
    }

    public async Task ProduceAsync()
    {
        await _sessionProvider.RunSessionAndSubscribeAsync(
            _sessionConfiguration,
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