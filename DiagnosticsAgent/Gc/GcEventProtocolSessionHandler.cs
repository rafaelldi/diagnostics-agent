using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Gc;

internal static class GcEventProtocolSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.GcEventProtocolSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, GcEventProtocolSession session)
    {
        var envelope = new GcEventProtocolSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}