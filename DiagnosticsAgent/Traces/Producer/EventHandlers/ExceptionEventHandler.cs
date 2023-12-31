﻿using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace DiagnosticsAgent.Traces.Producer.EventHandlers;

internal sealed class ExceptionEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueTrace> _writer;

    internal ExceptionEventHandler(int pid, ChannelWriter<ValueTrace> writer)
    {
        _pid = pid;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        lifetime.Bracket(
            () => source.Clr.ExceptionStart += HandleExceptionStartEvent,
            () => source.Clr.ExceptionStart -= HandleExceptionStartEvent
        );
    }

    private void HandleExceptionStartEvent(ExceptionTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Exception Thrown",
            PredefinedProvider.Exceptions,
            evt.TimeStamp,
            $"{evt.ExceptionType}: {evt.ExceptionMessage}"
        );
        _writer.TryWrite(trace);
    }
}