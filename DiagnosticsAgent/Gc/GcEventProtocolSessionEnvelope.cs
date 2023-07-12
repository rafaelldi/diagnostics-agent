using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Gc.Exporter;
using DiagnosticsAgent.Gc.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal sealed class GcEventProtocolSessionEnvelope : ProtocolSessionEnvelope<GcEventProtocolSession, ValueGcEvent>
{
    internal GcEventProtocolSessionEnvelope(int pid, GcEventProtocolSession session, Lifetime lifetime)
        : base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        GcEventProtocolSession session,
        ChannelReader<ValueGcEvent> reader
    ) => new GcEventProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        GcEventProtocolSession session,
        ChannelWriter<ValueGcEvent> writer,
        Lifetime lifetime
    ) => new GcEventProducer(pid, writer, lifetime);
}