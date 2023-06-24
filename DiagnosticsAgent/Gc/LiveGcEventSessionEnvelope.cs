using System.Threading.Channels;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal sealed class LiveGcEventSessionEnvelope
{
    private readonly GcEventProducer _producer;
    private readonly GcEventProtocolExporter _exporter;

    internal LiveGcEventSessionEnvelope(int pid, LiveGcEventSession session, Lifetime lifetime)
    {
        var channel = Channel.CreateBounded<ValueGcEvent>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        _exporter = new GcEventProtocolExporter(session, channel.Reader);
        _producer = new GcEventProducer(pid, channel.Writer, lifetime);

        session.Active.WhenTrue(lifetime, Handle);
    }

    private void Handle(Lifetime lt)
    {
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await _exporter.ConsumeAsync());
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await _producer.Produce());
    }
}