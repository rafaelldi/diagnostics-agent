using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Counters.Exporter;

internal sealed class CounterProtocolExporter : ProtocolExporter<CounterProtocolSession, ValueCounter>
{
    internal CounterProtocolExporter(CounterProtocolSession session, ChannelReader<ValueCounter> reader)
        : base(session, reader)
    {
    }

    protected override void ExportToProtocol(ValueCounter value)
    {
        Session.CounterReceived.Fire(new Counter(value.DisplayName, value.Tags, value.Value));
    }
}