﻿using System.Net;
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
        var scheduler =
            SingleThreadScheduler.RunOnSeparateThread(lifetime, "Diagnostics Toolbox to Rider Frontend Scheduler");
        var wire = new SocketWire.Server(lifetime, scheduler, new IPEndPoint(IPAddress.Loopback, port));
        var protocol = new Protocol(
            "Diagnostics Toolbox to Rider Frontend",
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

        LiveCounterSessionHandler.Subscribe(model, lifetime);
        PersistentCounterSessionHandler.Subscribe(model, lifetime);
        
        LiveGcEventSessionHandler.Subscribe(model, lifetime);
        PersistentGcEventSessionHandler.Subscribe(model, lifetime);
        TriggerGcCollectionHandler.Subscribe(model, lifetime);
        
        LiveChartSessionHandler.Subscribe(model, lifetime);
        
        LiveTraceSessionHandler.Subscribe(model, lifetime);
        PersistentTraceSessionHandler.Subscribe(model, lifetime);
    }
}