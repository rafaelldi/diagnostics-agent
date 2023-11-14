using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;
using DiagnosticsAgent.Traces.Exporter;
using DiagnosticsAgent.Traces.Producer;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal sealed class TraceSessionEnvelope : ProtocolSessionEnvelope<TraceSession, ValueTrace>
{
    internal TraceSessionEnvelope(int pid, TraceSession session, Lifetime lifetime)
        : base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        TraceSession session,
        ChannelReader<ValueTrace> reader
    ) => new TraceProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        TraceSession session,
        ChannelWriter<ValueTrace> writer,
        Lifetime lifetime)
    {
        var traceProducerConfiguration = new TraceProducerConfiguration(session.PredefinedProviders);
        return new TraceProducer(pid, traceProducerConfiguration, writer, lifetime);
    }
}