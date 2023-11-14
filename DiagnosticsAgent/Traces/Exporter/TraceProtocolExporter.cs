using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Traces.Exporter;

internal sealed class TraceProtocolExporter : ProtocolExporter<TraceSession, ValueTrace>
{
    internal TraceProtocolExporter(TraceSession session, ChannelReader<ValueTrace> reader)
        : base(session, reader)
    {
    }

    protected override void Export(ValueTrace value)
    {
        Session.TraceReceived.Fire(new Trace(
            value.EventName,
            value.Provider,
            value.TimeStamp,
            value.Content
        ));
    }
}