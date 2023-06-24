using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Gc;

internal static class TriggerGcCollectionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.TriggerGc.Advise(lifetime, Handle);
    }

    private static void Handle(int pid)
    {
        var providers = new[] { EventPipeProviderFactory.CreateGcHeapCollect() };
        var sessionManager = new EventPipeSessionManager(pid);
        using var session = sessionManager.StartSession(providers);
        var source = new EventPipeEventSource(session.EventStream);

        Task.Run(() => source.Process());

        EventPipeSessionManager.StopSession(session);
    }
}