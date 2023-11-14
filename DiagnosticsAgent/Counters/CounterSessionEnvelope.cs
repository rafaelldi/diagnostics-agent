using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Counters.Exporter;
using DiagnosticsAgent.Counters.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Counters;

internal sealed class CounterSessionEnvelope : ProtocolSessionEnvelope<CounterSession, ValueCounter>
{
    public CounterSessionEnvelope(int pid, CounterSession session, Lifetime lifetime)
        : base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        CounterSession session,
        ChannelReader<ValueCounter> reader
    ) => new CounterProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        CounterSession session,
        ChannelWriter<ValueCounter> writer,
        Lifetime lifetime)
    {
        var configuration = new CounterProducerConfiguration(
            Guid.NewGuid().ToString(),
            session.Providers,
            session.Metrics,
            session.RefreshInterval,
            session.MaxTimeSeries,
            session.MaxHistograms
        );

        return new CounterProducer(pid, configuration, writer, lifetime);
    }
}