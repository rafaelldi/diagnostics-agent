﻿using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace DiagnosticsAgent.Traces.Producer.EventHandlers;

internal sealed class ContentionEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueTrace> _writer;

    internal ContentionEventHandler(int pid, ChannelWriter<ValueTrace> writer)
    {
        _pid = pid;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        lifetime.Bracket(
            () => source.Clr.ContentionStart += HandleContentionStartEvent,
            () => source.Clr.ContentionStart -= HandleContentionStartEvent
        );
        lifetime.Bracket(
            () => source.Clr.ContentionStop += HandleContentionStopEvent,
            () => source.Clr.ContentionStop -= HandleContentionStopEvent
        );
    }

    private void HandleContentionStartEvent(ContentionStartTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Contention Start",
            PredefinedProvider.Contentions,
            evt.TimeStamp,
            string.Empty
        );
        _writer.TryWrite(trace);
    }

    private void HandleContentionStopEvent(ContentionStopTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Contention Stop",
            PredefinedProvider.Contentions,
            evt.TimeStamp,
            $"Duration(ns): {evt.DurationNs}"
        );
        _writer.TryWrite(trace);
    }
}