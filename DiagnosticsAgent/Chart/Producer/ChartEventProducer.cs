using System.Threading.Channels;
using DiagnosticsAgent.Chart.Producer.EventHandlers;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using static DiagnosticsAgent.Common.Providers;
using static DiagnosticsAgent.EventPipes.EventPipeProviderFactory;

namespace DiagnosticsAgent.Chart.Producer;

internal sealed class ChartEventProducer : IValueProducer
{
    private readonly EventPipeSessionProvider _sessionProvider;
    private readonly EventPipeSessionConfiguration _sessionConfiguration;
    private readonly ChartCounterEventHandler _counterEventHandler;
    private readonly ChartTraceEventHandler _traceEventHandler;

    internal ChartEventProducer(
        int pid,
        ChannelWriter<ValueChartEvent> writer,
        Lifetime lifetime)
    {
        _sessionProvider = new EventPipeSessionProvider(pid);

        var counterProvider = CreateCounterProvider(SystemRuntimeProvider);
        _sessionConfiguration = new EventPipeSessionConfiguration(
            new[] { counterProvider },
            false
        );

        _counterEventHandler = new ChartCounterEventHandler(pid, writer);
        _traceEventHandler = new ChartTraceEventHandler(pid, writer);

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
        _traceEventHandler.SubscribeToEvents(source, lifetime);
    }
}