using System.Threading.Channels;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal static class PersistentGcEventSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.PersistentGcEventSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, PersistentGcEventSession session)
    {
        var channel = Channel.CreateBounded<ValueGcEvent>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });
        
        var exporter = new GcEventCsvExporter(session.FilePath, channel.Reader);
        var producer = new GcEventProducer(pid, channel.Writer, lt);

        lt.StartAttachedAsync(TaskScheduler.Default, async () => await exporter.ConsumeAsync());
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await producer.Produce());
    }
}