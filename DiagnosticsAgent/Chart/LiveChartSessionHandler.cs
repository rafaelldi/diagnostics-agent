using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal static class LiveChartSessionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.LiveChartSessions.View(lifetime, Handle);
    }

    private static void Handle(Lifetime lt, int pid, LiveChartSession session)
    {
        var envelope = new LiveChartSessionEnvelope(pid, session, lt);
        lt.KeepAlive(envelope);
    }
}