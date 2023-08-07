using System.Threading.Channels;
using DiagnosticsAgent.Chart.Exporter;
using DiagnosticsAgent.Chart.Producer;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal sealed class ChartProtocolSessionEnvelope : ProtocolSessionEnvelope<ChartProtocolSession, ValueChartEvent>
{
    internal ChartProtocolSessionEnvelope(int pid, ChartProtocolSession session, Lifetime lifetime) :
        base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        ChartProtocolSession session,
        ChannelReader<ValueChartEvent> reader
    ) => new ChartEventProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        ChartProtocolSession session,
        ChannelWriter<ValueChartEvent> writer,
        Lifetime lifetime
    ) => new ChartEventProducer(pid, writer, lifetime);
}