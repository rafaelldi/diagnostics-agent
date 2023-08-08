using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

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
            () => source.Clr.ExceptionStart += HandleExceptionStartEvent,
            () => source.Clr.ExceptionStart -= HandleExceptionStartEvent
        );

        lifetime.Bracket(
            () => source.Clr.GCStop += HandleGcStopEvent,
            () => source.Clr.GCStop -= HandleGcStopEvent
        );
    }

    private void HandleExceptionStartEvent(ExceptionTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var label = evt.ExceptionType;
        _writer.TryWrite(new ValueChartEvent(evt.TimeStamp, ChartEventType.Exception, 0.0, label));
    }

    private void HandleGcStopEvent(GCEndTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var label = $"GC generation {evt.Depth}";
        _writer.TryWrite(new ValueChartEvent(evt.TimeStamp, ChartEventType.Gc, 0.0, label));
    }
}