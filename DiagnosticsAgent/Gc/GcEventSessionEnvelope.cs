using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Gc.Exporter;
using DiagnosticsAgent.Gc.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal sealed class GcEventSessionEnvelope : ProtocolSessionEnvelope<GcEventSession, ValueGcEvent>
{
    internal GcEventSessionEnvelope(int pid, GcEventSession session, Lifetime lifetime)
        : base(pid, session, lifetime)
    {
    }

    protected override IValueConsumer CreateConsumer(
        GcEventSession session,
        ChannelReader<ValueGcEvent> reader
    ) => new GcEventProtocolExporter(session, reader);

    protected override IValueProducer CreateProducer(
        int pid,
        GcEventSession session,
        ChannelWriter<ValueGcEvent> writer,
        Lifetime lifetime
    ) => new GcEventProducer(pid, writer, lifetime);
}