using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace DiagnosticsAgent.Chart.Producer.EventHandlers;

internal sealed class ChartTraceEventHandler(int pid, ChannelWriter<ValueChartEvent> writer) : IEventPipeEventHandler
{
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
        if (evt.ProcessID != pid) return;
        var label = evt.ExceptionType;
        writer.TryWrite(new ValueChartEvent(evt.TimeStamp, ChartEventType.Exception, 0.0, label));
    }

    private void HandleGcStopEvent(GCEndTraceData evt)
    {
        if (evt.ProcessID != pid) return;
        var label = $"GC generation {evt.Depth}";
        writer.TryWrite(new ValueChartEvent(evt.TimeStamp, ChartEventType.Gc, 0.0, label));
    }
}