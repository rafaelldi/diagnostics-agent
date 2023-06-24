using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Counters;

internal static class LiveCounterSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.LiveCounterSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, LiveCounterSession session)
    {
        var envelope = new LiveCounterSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}