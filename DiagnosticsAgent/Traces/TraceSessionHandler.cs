using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal static class TraceSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.TraceSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, TraceSession session)
    {
        var envelope = new TraceSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}