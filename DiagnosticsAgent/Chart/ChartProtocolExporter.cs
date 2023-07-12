using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Counters;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Chart;

internal sealed class ChartProtocolExporter : ProtocolExporter<ChartProtocolSession, ValueCounter>
{
    private const string CpuCounterName = "CPU Usage";
    private const string GcHeapSizeCounterName = "GC Heap Size";
    private const string WorkingSetCounterName = "Working Set";

    internal ChartProtocolExporter(ChartProtocolSession session, ChannelReader<ValueCounter> reader) 
        : base(session, reader)
    {
    }

    protected override void ExportToProtocol(ValueCounter value)
    {
        var chartValue = Map(value);
        if (chartValue is not null)
        {
            Session.ValueReceived.Fire(chartValue);
        }
    }

    private static ChartValue? Map(ValueCounter counter)
    {
        var offset = new DateTimeOffset(counter.TimeStamp);
        var timestamp = offset.ToUnixTimeSeconds();

        return counter.Name switch
        {
            CpuCounterName => new ChartValue(timestamp, counter.Value, ChartValueType.Cpu),
            GcHeapSizeCounterName => new ChartValue(timestamp, counter.Value, ChartValueType.GcHeapSize),
            WorkingSetCounterName => new ChartValue(timestamp, counter.Value, ChartValueType.WorkingSet),
            _ => null
        };
    }
}