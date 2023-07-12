using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal static class ChartProtocolSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.ChartProtocolSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, ChartProtocolSession session)
    {
        var envelope = new ChartProtocolSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}