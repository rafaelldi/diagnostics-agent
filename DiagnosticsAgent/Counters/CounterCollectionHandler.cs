using System.Threading.Channels;
using DiagnosticsAgent.Counters.Exporter;
using DiagnosticsAgent.Counters.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using JetBrains.Rd.Tasks;

// ReSharper disable SuggestBaseTypeForParameter

namespace DiagnosticsAgent.Counters;

internal static class CounterCollectionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model)
    {
        model.CollectCounters.SetAsync(async (lt, command) => await CollectAsync(command, lt));
    }

    private static async Task<CounterCollectionResult> CollectAsync(CollectCounterCommand command, Lifetime lifetime)
    {
        var collectionLifetime = command.Duration.HasValue
            ? lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(command.Duration.Value))
            : lifetime;

        var channel = Channel.CreateBounded<ValueCounter>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        var exporter = CreateExporter(command, channel);
        var producer = CreateProducer(command, channel, collectionLifetime);

        var consuming =
            collectionLifetime.StartAttachedAsync(TaskScheduler.Default, async () => await exporter.ConsumeAsync());
        var producing =
            collectionLifetime.StartAttachedAsync(TaskScheduler.Default, async () => await producer.ProduceAsync());

        await Task.WhenAll(consuming, producing);

        return new CounterCollectionResult(command.ExportFilePath);
    }

    private static FileCounterExporter CreateExporter(
        CollectCounterCommand command,
        Channel<ValueCounter> channel) =>
        FileCounterExporter.Create(command.ExportFilePath, command.Format, channel.Reader);

    private static CounterProducer CreateProducer(
        CollectCounterCommand command,
        Channel<ValueCounter> channel,
        Lifetime lt)
    {
        var configuration = new CounterProducerConfiguration(
            Guid.NewGuid().ToString(),
            command.Providers,
            command.Metrics,
            command.RefreshInterval,
            command.MaxTimeSeries,
            command.MaxHistograms
        );
        return new CounterProducer(command.Pid, configuration, channel.Writer, lt);
    }
}