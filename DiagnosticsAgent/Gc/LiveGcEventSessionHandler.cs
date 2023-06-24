using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal static class LiveGcEventSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.LiveGcEventSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, LiveGcEventSession session)
    {
        var envelope = new LiveGcEventSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}