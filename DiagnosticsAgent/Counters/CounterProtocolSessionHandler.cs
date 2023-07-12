using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Counters;

internal static class CounterProtocolSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.CounterProtocolSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, CounterProtocolSession session)
    {
        var envelope = new CounterProtocolSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}