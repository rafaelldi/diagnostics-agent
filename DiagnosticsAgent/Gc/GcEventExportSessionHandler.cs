using System.Threading.Channels;
using DiagnosticsAgent.Gc.Exporter;
using DiagnosticsAgent.Gc.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal static class GcEventExportSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.GcEventExportSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, GcEventExportSession session)
    {
        var channel = Channel.CreateBounded<ValueGcEvent>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        var exporter = new GcEventCsvExporter(session.ExportFilePath, channel.Reader);
        var producer = new GcEventProducer(pid, channel.Writer, lt);

        lt.StartAttachedAsync(TaskScheduler.Default, async () => await exporter.ConsumeAsync());
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await producer.ProduceAsync());
    }
}