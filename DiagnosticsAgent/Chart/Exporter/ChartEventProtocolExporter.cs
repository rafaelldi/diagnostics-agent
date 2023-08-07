using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Chart.Exporter;

internal sealed class ChartEventProtocolExporter : ProtocolExporter<ChartProtocolSession, ValueChartEvent>
{
    internal ChartEventProtocolExporter(ChartProtocolSession session, ChannelReader<ValueChartEvent> reader)
        : base(session, reader)
    {
    }

    protected override void ExportToProtocol(ValueChartEvent value)
    {
        var offset = new DateTimeOffset(value.TimeStamp);
        var timestamp = offset.ToUnixTimeSeconds();
        Session.ValueReceived.Fire(new ChartValue(timestamp, value.Value, value.Type));
    }
}