using System.Threading.Channels;
using DiagnosticsAgent.Counters.Exporter;
using DiagnosticsAgent.Counters.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

// ReSharper disable SuggestBaseTypeForParameter

namespace DiagnosticsAgent.Counters;

internal static class CounterExportSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.CounterExportSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, CounterExportSession session)
    {
        var channel = Channel.CreateBounded<ValueCounter>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });
        
        var exporter = CreateExporter(session, channel);
        var producer = CreateProducer(pid, session, channel, lt);

        lt.StartAttachedAsync(TaskScheduler.Default, async () => await exporter.ConsumeAsync());
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await producer.ProduceAsync());
    }

    private static FileCounterExporter CreateExporter(
        CounterExportSession session,
        Channel<ValueCounter> channel) =>
        FileCounterExporter.Create(session.ExportFilePath, session.Format, channel.Reader);

    private static CounterProducer CreateProducer(
        int pid,
        CounterExportSession session,
        Channel<ValueCounter> channel,
        Lifetime lt)
    {
        var configuration = new CounterProducerConfiguration(
            Guid.NewGuid().ToString(),
            session.Providers,
            session.Metrics,
            session.RefreshInterval,
            session.MaxTimeSeries,
            session.MaxHistograms
        );
        return new CounterProducer(pid, configuration, channel.Writer, lt);
    }
}