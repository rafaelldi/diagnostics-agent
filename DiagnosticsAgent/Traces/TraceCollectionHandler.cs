using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using JetBrains.Rd.Tasks;

namespace DiagnosticsAgent.Traces;

internal static class TraceCollectionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model)
    {
        model.CollectTraces.SetAsync(async (lt, command) => await CollectAsync(command, lt));
    }

    private static async Task<TraceCollectionResult> CollectAsync(CollectTraceCommand command, Lifetime lifetime)
    {
        var collectionLifetime = command.Duration.HasValue
            ? lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(command.Duration.Value))
            : lifetime;

        var sessionProvider = new EventPipeSessionProvider(command.Pid);
        var providers = new TraceProviderCollection(command.Providers, command.Profile, command.PredefinedProviders);
        var sessionConfiguration = new EventPipeSessionConfiguration(providers.EventPipeProviders);

        await collectionLifetime.StartAttachedAsync(
            TaskScheduler.Default,
            async () => await sessionProvider.RunSessionAndCopyToFileAsync(
                sessionConfiguration,
                Lifetime.AsyncLocal.Value,
                command.ExportFilePath
            )
        );

        return new TraceCollectionResult(command.ExportFilePath);
    }
}