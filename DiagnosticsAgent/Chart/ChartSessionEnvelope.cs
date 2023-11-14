using System.Threading.Channels;
using DiagnosticsAgent.Chart.Exporter;
using DiagnosticsAgent.Chart.Producer;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal sealed class ChartSessionEnvelope : ProtocolSessionEnvelope<ChartSession, ValueChartEvent>
{
    internal ChartSessionEnvelope(int pid, ChartSession session, Lifetime lifetime) :
        base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        ChartSession session,
        ChannelReader<ValueChartEvent> reader
    ) => new ChartEventProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        ChartSession session,
        ChannelWriter<ValueChartEvent> writer,
        Lifetime lifetime
    ) => new ChartEventProducer(pid, writer, lifetime);
}