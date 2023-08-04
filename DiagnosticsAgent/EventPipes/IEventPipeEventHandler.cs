using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.EventPipes;

internal interface IEventPipeEventHandler
{
    void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime);
}