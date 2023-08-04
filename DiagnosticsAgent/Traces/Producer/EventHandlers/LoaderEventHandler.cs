using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace DiagnosticsAgent.Traces.Producer.EventHandlers;

internal sealed class LoaderEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueTrace> _writer;

    internal LoaderEventHandler(int pid, ChannelWriter<ValueTrace> writer)
    {
        _pid = pid;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        lifetime.Bracket(
            () => source.Clr.LoaderAssemblyLoad += HandleLoaderAssemblyLoadEvent,
            () => source.Clr.LoaderAssemblyLoad -= HandleLoaderAssemblyLoadEvent
        );
        lifetime.Bracket(
            () => source.Clr.LoaderAssemblyUnload += HandleLoaderAssemblyUnloadEvent,
            () => source.Clr.LoaderAssemblyUnload -= HandleLoaderAssemblyUnloadEvent
        );
    }

    private void HandleLoaderAssemblyLoadEvent(AssemblyLoadUnloadTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Assembly Load",
            PredefinedProvider.Loader,
            evt.TimeStamp,
            $"Assembly name: {evt.FullyQualifiedAssemblyName}"
        );
        _writer.TryWrite(trace);
    }

    private void HandleLoaderAssemblyUnloadEvent(AssemblyLoadUnloadTraceData evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Assembly Unload",
            PredefinedProvider.Loader, 
            evt.TimeStamp,
            $"Assembly name: {evt.FullyQualifiedAssemblyName}"
        );
        _writer.TryWrite(trace);
    }
}