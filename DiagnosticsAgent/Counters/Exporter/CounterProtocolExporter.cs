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
        var key = string.IsNullOrEmpty(value.Tags) ? value.Name : $"{value.Name}-{value.Tags}";
        Session.Counters[key] = new Counter(value.DisplayName, value.Tags, value.Value);
    }
}