using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Counters;

internal static class CounterSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.CounterSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, CounterSession session)
    {
        var envelope = new CounterSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}