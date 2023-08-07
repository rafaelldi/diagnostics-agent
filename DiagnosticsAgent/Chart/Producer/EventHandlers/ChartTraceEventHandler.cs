using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Chart.Producer.EventHandlers;

internal sealed class ChartTraceEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueChartEvent> _writer;

    public ChartTraceEventHandler(int pid, ChannelWriter<ValueChartEvent> writer)
    {
        _pid = pid;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        lifetime.Bracket(
            () => source.Dynamic.All += HandleEvent,
            () => source.Dynamic.All -= HandleEvent
        );
    }

    private void HandleEvent(TraceEvent evt)
    {

    }
}