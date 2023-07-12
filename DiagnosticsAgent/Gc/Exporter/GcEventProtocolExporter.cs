using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Gc.Exporter;

internal sealed class GcEventProtocolExporter : ProtocolExporter<GcEventProtocolSession, ValueGcEvent>
{
    internal GcEventProtocolExporter(GcEventProtocolSession session, ChannelReader<ValueGcEvent> reader)
        : base(session, reader)
    {
    }

    protected override void ExportToProtocol(ValueGcEvent value)
    {
        Session.GcHappened.Fire(new GcEvent(
            value.Number,
            value.Generation,
            value.Reason,
            value.PauseDuration,
            value.Peak,
            value.After,
            value.Ratio,
            value.Promoted,
            value.Allocated,
            value.AllocationRate,
            value.SizeGen0,
            value.SizeGen1,
            value.SizeGen2,
            value.SizeLoh,
            value.PinnedObjects
        ));
    }
}