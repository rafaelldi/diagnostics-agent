using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal static class TraceExportSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.TraceExportSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, TraceExportSession session)
    {
        var sessionProvider = new EventPipeSessionProvider(pid);
        var providers = new TraceProviderCollection(session.Providers, session.Profile, session.PredefinedProviders);
        var sessionConfiguration = new EventPipeSessionConfiguration(providers.EventPipeProviders);

        lt.StartAttachedAsync(
            TaskScheduler.Default,
            async () => await sessionProvider.RunSessionAndCopyToFileAsync(
                sessionConfiguration,
                Lifetime.AsyncLocal.Value,
                session.ExportFilePath
            )
        );
    }
}