﻿using System.Text;
using System.Threading.Channels;
using DiagnosticsAgent.Common;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Traces.EventHandlers;

internal sealed class EfEventHandler : IEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueTrace> _writer;
    private const string Source = "Microsoft.EntityFrameworkCore";

    internal EfEventHandler(int pid, ChannelWriter<ValueTrace> writer)
    {
        _pid = pid;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source)
    {
        Lifetime.AsyncLocal.Value.Bracket(
            () => source.Dynamic.All += HandleEvent,
            () => source.Dynamic.All -= HandleEvent
        );
    }

    private void HandleEvent(TraceEvent evt)
    {
        if (evt.ProviderName != Providers.MicrosoftDiagnosticSource)
        {
            return;
        }

        if (evt.ProcessID != _pid)
        {
            return;
        }

        if (evt.PayloadByName("SourceName")?.ToString() != Source)
        {
            return;
        }

        if (evt.EventName is not ("Activity2/Start" or "Activity2/Stop"))
        {
            return;
        }

        if (evt.PayloadByName("Arguments") is not IDictionary<string, object>[] arguments)
        {
            return;
        }

        var sb = new StringBuilder();
        foreach (var argument in arguments)
        {
            if (!argument.TryGetValue("Key", out var key) || !argument.TryGetValue("Value", out var value)) continue;

            var keyString = key?.ToString();
            var valueString = value?.ToString();

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(valueString))
            {
                continue;
            }

            sb.Append($"{keyString} = {valueString}; ");
        }

        var trace = new ValueTrace(
            GetEventName(evt),
            PredefinedProvider.EF,
            evt.TimeStamp,
            sb.ToString()
        );
        _writer.TryWrite(trace);
    }

    private static string GetEventName(TraceEvent evt) => evt.EventName switch
    {
        "Activity2/Start" => "Command Started",
        "Activity2/Stop" => "Command Executed",
        _ => evt.EventName
    };
}