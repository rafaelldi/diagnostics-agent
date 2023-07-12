using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Counters.Exporter;
using DiagnosticsAgent.Counters.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Counters;

internal sealed class CounterProtocolSessionEnvelope : ProtocolSessionEnvelope<CounterProtocolSession, ValueCounter>
{
    public CounterProtocolSessionEnvelope(int pid, CounterProtocolSession session, Lifetime lifetime)
        : base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        CounterProtocolSession session,
        ChannelReader<ValueCounter> reader
    ) => new CounterProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        CounterProtocolSession session,
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