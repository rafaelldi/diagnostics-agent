using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Traces.Exporter;

internal sealed class TraceProtocolExporter : ProtocolExporter<TraceProtocolSession, ValueTrace>
{
    internal TraceProtocolExporter(TraceProtocolSession session, ChannelReader<ValueTrace> reader)
        : base(session, reader)
    {
    }

    protected override void ExportToProtocol(ValueTrace value)
    {
        Session.TraceReceived.Fire(new Trace(
            value.EventName,
            value.Provider,
            value.TimeStamp,
            value.Content
        ));
    }
}