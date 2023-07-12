using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Counters;
using DiagnosticsAgent.Counters.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal sealed class ChartProtocolSessionEnvelope : ProtocolSessionEnvelope<ChartProtocolSession, ValueCounter>
{
    internal ChartProtocolSessionEnvelope(int pid, ChartProtocolSession session, Lifetime lifetime) :
        base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        ChartProtocolSession session,
        ChannelReader<ValueCounter> reader
    ) => new ChartProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        ChartProtocolSession session,
        ChannelWriter<ValueCounter> writer,
        Lifetime lifetime)
    {
        var configuration = new CounterProducerConfiguration(
            Guid.NewGuid().ToString(),
            "System.Runtime[cpu-usage,gc-heap-size,working-set]",
            null,
            1,
            1000,
            10
        );

        return new CounterProducer(pid, configuration, writer, lifetime);
    }
}