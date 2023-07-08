﻿using System.Threading.Channels;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Traces;

internal sealed class TraceProtocolExporter
{
    private readonly LiveTraceSession _session;
    private readonly ChannelReader<ValueTrace> _reader;

    internal TraceProtocolExporter(LiveTraceSession session, ChannelReader<ValueTrace> reader)
    {
        _session = session;
        _reader = reader;
    }

    internal async Task ConsumeAsync()
    {
        try
        {
            while (await _reader.WaitToReadAsync(Lifetime.AsyncLocal.Value))
            {
                if (_reader.TryRead(out var trace))
                {
                    _session.TraceReceived.Fire(
                        new Trace(
                            trace.EventName,
                            trace.Provider,
                            trace.TimeStamp,
                            trace.Content
                        )
                    );
                }
            }
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
    }
}