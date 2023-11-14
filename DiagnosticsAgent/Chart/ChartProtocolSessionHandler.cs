using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal static class ChartProtocolSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.ChartSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, ChartSession session)
    {
        var envelope = new ChartSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}