using System.Net;
using DiagnosticsAgent.Chart;
using DiagnosticsAgent.Counters;
using DiagnosticsAgent.Dumps;
using DiagnosticsAgent.Gc;
using DiagnosticsAgent.Model;
using DiagnosticsAgent.Processes;
using DiagnosticsAgent.StackTrace;
using DiagnosticsAgent.Traces;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using JetBrains.Rd;
using JetBrains.Rd.Impl;

// ReSharper disable SuggestBaseTypeForParameter

namespace DiagnosticsAgent;

internal static class DiagnosticsHost
{
    internal static async Task Run(int port, Lifetime lifetime)
    {
        var scheduler = SingleThreadScheduler.RunOnSeparateThread(lifetime, "Diagnostics Agent to Host Scheduler");
        var wire = new SocketWire.Server(lifetime, scheduler, new IPEndPoint(IPAddress.Loopback, port));
        var protocol = new Protocol(
            "Diagnostics Agent to Host",
            new Serializers(),
            new Identities(IdKind.Server),
            scheduler,
            wire,
            lifetime
        );

        scheduler.Queue(() => RunRdHost(protocol, lifetime));

        await lifetime.ExecuteAsync(async () =>
            await Task.Delay(Timeout.InfiniteTimeSpan, lifetime.ToCancellationToken())
        );
    }

    private static void RunRdHost(Protocol protocol, Lifetime lifetime)
    {
        var model = new DiagnosticsHostModel(lifetime, protocol);
        var handlerLifetimeDefinition = lifetime.CreateNested();

        ProcessHandler.Subscribe(model, handlerLifetimeDefinition.Lifetime);

        DumpCollectionHandler.Subscribe(model);
        StackTraceCollectionHandler.Subscribe(model);

        CounterSessionHandler.Subscribe(model, lifetime);
        CounterExportSessionHandler.Subscribe(model, lifetime);

        GcEventSessionHandler.Subscribe(model, lifetime);
        GcEventExportSessionHandler.Subscribe(model, lifetime);

        TraceSessionHandler.Subscribe(model, lifetime);
        TraceExportSessionHandler.Subscribe(model, lifetime);

        ChartProtocolSessionHandler.Subscribe(model, lifetime);
    }
}