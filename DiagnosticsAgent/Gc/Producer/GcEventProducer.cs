using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing.Analysis.GC;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace DiagnosticsAgent.Gc.Producer;

internal sealed class GcEventProducer : IValueProducer
{
    private readonly int _pid;
    private readonly EventPipeSessionProvider _sessionProvider;
    private readonly EventPipeSessionConfiguration _sessionConfiguration;
    private readonly ChannelWriter<ValueGcEvent> _writer;

    internal GcEventProducer(
        int pid,
        ChannelWriter<ValueGcEvent> writer,
        Lifetime lifetime)
    {
        _pid = pid;
        _sessionProvider = new EventPipeSessionProvider(pid);
        _writer = writer;

        _sessionConfiguration = new EventPipeSessionConfiguration(
            new[] { EventPipeProviderFactory.CreateGcProvider() },
            false
        );

        lifetime.OnTermination(() => _writer.Complete());
    }

    public async Task ProduceAsync()
    {
        await _sessionProvider.RunSessionAndSubscribeAsync(
            _sessionConfiguration,
            Lifetime.AsyncLocal.Value,
            (source, _) => SubscribeToEvents(source)
        );
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void SubscribeToEvents(EventPipeEventSource source)
    {
        source.NeedLoadedDotNetRuntimes();
        source.AddCallbackOnProcessStart(tp =>
        {
            tp.AddCallbackOnDotNetRuntimeLoad(runtime => runtime.GCEnd += HandleEvent);
        });
    }

    private void HandleEvent(TraceProcess process, TraceGC gc)
    {
        if (process.ProcessID != _pid)
        {
            return;
        }

        var gcEvent = new ValueGcEvent(
            gc.Number,
            gc.GCGenerationName?.ToString() ?? "",
            gc.Reason.ToValue(),
            Math.Round(gc.PauseDurationMSec, 3),
            Math.Round(gc.HeapSizePeakMB, 3),
            Math.Round(gc.HeapSizeAfterMB, 3),
            Math.Round(gc.RatioPeakAfter, 2),
            Math.Round(gc.PromotedMB, 3),
            Math.Round(gc.AllocedSinceLastGCMB, 3),
            Math.Round(gc.AllocRateMBSec, 2),
            Math.Round(gc.GenSizeAfterMB(Gens.Gen0), 3),
            Math.Round(gc.GenFragmentationPercent(Gens.Gen0), 3),
            Math.Round(gc.SurvivalPercent(Gens.Gen0), 2),
            Math.Round(gc.GenBudgetMB(Gens.Gen0), 2),
            Math.Round(gc.GenSizeAfterMB(Gens.Gen1), 3),
            Math.Round(gc.GenFragmentationPercent(Gens.Gen1), 3),
            Math.Round(gc.SurvivalPercent(Gens.Gen1), 2),
            Math.Round(gc.GenBudgetMB(Gens.Gen1), 2),
            Math.Round(gc.GenSizeAfterMB(Gens.Gen2), 3),
            Math.Round(gc.GenFragmentationPercent(Gens.Gen2), 3),
            Math.Round(gc.SurvivalPercent(Gens.Gen2), 2),
            Math.Round(gc.GenBudgetMB(Gens.Gen2), 2),
            Math.Round(gc.GenSizeAfterMB(Gens.GenLargeObj), 3),
            Math.Round(gc.GenFragmentationPercent(Gens.GenLargeObj), 3),
            Math.Round(gc.SurvivalPercent(Gens.GenLargeObj), 2),
            Math.Round(gc.GenBudgetMB(Gens.GenLargeObj), 2),
            gc.HeapStats.PinnedObjectCount
        );

        _writer.TryWrite(gcEvent);
    }
}