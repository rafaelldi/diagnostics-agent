using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal static class LiveTraceSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.LiveTraceSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, LiveTraceSession session)
    {
        var envelope = new LiveTraceSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}