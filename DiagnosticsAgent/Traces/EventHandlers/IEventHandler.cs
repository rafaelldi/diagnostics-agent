using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Traces.EventHandlers;

internal interface IEventHandler
{
    void SubscribeToEvents(EventPipeEventSource source);
}