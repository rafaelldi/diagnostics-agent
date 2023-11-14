using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal static class GcEventSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.GcEventSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, GcEventSession session)
    {
        var envelope = new GcEventSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}