using System.Threading.Channels;
using DiagnosticsAgent.Gc.Exporter;
using DiagnosticsAgent.Gc.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using JetBrains.Rd.Tasks;

namespace DiagnosticsAgent.Gc;

internal static class GcEventCollectionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model)
    {
        model.CollectGcEvents.SetAsync(async (lt, command) => await CollectAsync(command, lt));
    }

    private static async Task<GcEventCollectionResult> CollectAsync(CollectGcEventCommand command, Lifetime lifetime)
    {
        var collectionLifetime = command.Duration.HasValue
            ? lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(command.Duration.Value))
            : lifetime;

        var channel = Channel.CreateBounded<ValueGcEvent>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        var exporter = new GcEventCsvExporter(command.ExportFilePath, channel.Reader);
        var producer = new GcEventProducer(command.Pid, channel.Writer, collectionLifetime);

        var consuming =
            collectionLifetime.StartAttachedAsync(TaskScheduler.Default, async () => await exporter.ConsumeAsync());
        var producing =
            collectionLifetime.StartAttachedAsync(TaskScheduler.Default, async () => await producer.ProduceAsync());

        await Task.WhenAll(consuming, producing);

        return new GcEventCollectionResult(command.ExportFilePath);
    }
}