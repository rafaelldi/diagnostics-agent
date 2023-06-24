using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal static class PersistentTraceSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.PersistentTraceSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, PersistentTraceSession session)
    {
        var providers = new TraceProviderCollection(session.Providers, session.Profile, session.PredefinedProviders);
        var sessionManager = new EventPipeSessionManager(pid);
        var eventPipeSession = sessionManager.StartSession(providers.EventPipeProviders);
        lt.AddDispose(eventPipeSession);
        
        var fileStream = new FileStream(session.FilePath, FileMode.Create, FileAccess.Write);
        lt.AddDispose(fileStream);

        var cancellationToken = lt.ToCancellationToken();
        cancellationToken.Register(() => EventPipeSessionManager.StopSession(eventPipeSession));

        // ReSharper disable once MethodSupportsCancellation
        var copyTask = eventPipeSession.EventStream.CopyToAsync(fileStream, 81920);
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await copyTask);
    }
}