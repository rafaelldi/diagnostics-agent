using System.Diagnostics.Tracing;
using System.Threading.Channels;
using DiagnosticsAgent.Chart.Producer.EventHandlers;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using static DiagnosticsAgent.Common.Providers;
using static DiagnosticsAgent.EventPipes.EventPipeProviderFactory;
using static Microsoft.Diagnostics.Tracing.Parsers.ClrTraceEventParser;

namespace DiagnosticsAgent.Chart.Producer;

internal sealed class ChartEventProducer : IValueProducer
{
    private readonly EventPipeSessionProvider _sessionProvider;
    private readonly EventPipeSessionConfiguration _sessionConfiguration;
    private readonly ChartCounterEventHandler _counterEventHandler;
    private readonly ChartTraceEventHandler _traceEventHandler;

    private static readonly EventPipeProvider CounterProvider = CreateCounterProvider(SystemRuntimeProvider);
    private static readonly EventPipeProvider TraceProvider = CreateTraceProvider(
        DotNetRuntimeProvider,
        EventLevel.Informational,
        (long)(Keywords.Exception | Keywords.GC)
    );

    private static readonly EventPipeProvider[] EventPipeProviders = { CounterProvider, TraceProvider };

    internal ChartEventProducer(
        int pid,
        ChannelWriter<ValueChartEvent> writer,
        Lifetime lifetime)
    {
        _sessionProvider = new EventPipeSessionProvider(pid);
        _sessionConfiguration = new EventPipeSessionConfiguration(EventPipeProviders, false);

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