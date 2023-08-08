using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Chart.Producer.EventHandlers;

internal sealed class ChartCounterEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueChartEvent> _writer;
    private const string EventName = "EventCounters";
    private const string CpuCounterName = "cpu-usage";
    private const string GcHeapSizeCounterName = "gc-heap-size";
    private const string WorkingSetCounterName = "working-set";

    public ChartCounterEventHandler(int pid, ChannelWriter<ValueChartEvent> writer)
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
        if (evt.ProcessID != _pid)
        {
            return;
        }

        if (evt.EventName != EventName)
        {
            return;
        }

        var payloadVal = (IDictionary<string, object>)evt.PayloadValue(0);
        var payloadFields = (IDictionary<string, object>)payloadVal["Payload"];
        var name = payloadFields["Name"].ToString();

        ChartEventType? type = name switch
        {
            CpuCounterName => ChartEventType.Cpu,
            GcHeapSizeCounterName => ChartEventType.GcHeapSize,
            WorkingSetCounterName => ChartEventType.WorkingSet,
            _ => null
        };
        if (type is null) return;

        var value = payloadFields["CounterType"].ToString() == "Sum"
            ? (double)payloadFields["Increment"]
            : (double)payloadFields["Mean"];

        _writer.TryWrite(new ValueChartEvent(evt.TimeStamp, type.Value, value, null));
    }
}