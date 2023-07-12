using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;
using DiagnosticsAgent.Traces.Exporter;
using DiagnosticsAgent.Traces.Producer;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal sealed class TraceProtocolSessionEnvelope : ProtocolSessionEnvelope<TraceProtocolSession, ValueTrace>
{
    internal TraceProtocolSessionEnvelope(int pid, TraceProtocolSession session, Lifetime lifetime)
        : base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        TraceProtocolSession session,
        ChannelReader<ValueTrace> reader
    ) => new TraceProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        TraceProtocolSession session,
        ChannelWriter<ValueTrace> writer,
        Lifetime lifetime)
    {
        var traceProducerConfiguration = new TraceProducerConfiguration(session.PredefinedProviders);
        return new TraceProducer(pid, traceProducerConfiguration, writer, lifetime);
    }
}