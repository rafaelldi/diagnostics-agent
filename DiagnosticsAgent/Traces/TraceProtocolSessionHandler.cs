using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal static class TraceProtocolSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.TraceProtocolSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, TraceProtocolSession session)
    {
        var envelope = new TraceProtocolSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}