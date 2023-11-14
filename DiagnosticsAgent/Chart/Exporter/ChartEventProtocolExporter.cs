using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Chart.Exporter;

internal sealed class ChartEventProtocolExporter : ProtocolExporter<ChartSession, ValueChartEvent>
{
    internal ChartEventProtocolExporter(ChartSession session, ChannelReader<ValueChartEvent> reader)
        : base(session, reader)
    {
    }

    protected override void Export(ValueChartEvent value)
    {
        var offset = new DateTimeOffset(value.TimeStamp);
        var timestamp = offset.ToUnixTimeSeconds();
        Session.EventReceived.Fire(new ChartEvent(timestamp, value.Value, value.Type, value.Label));
    }
}