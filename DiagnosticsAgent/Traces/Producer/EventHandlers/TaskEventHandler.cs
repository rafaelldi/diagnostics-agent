using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Tpl;

namespace DiagnosticsAgent.Traces.Producer.EventHandlers;

internal sealed class TaskEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly ChannelWriter<ValueTrace> _writer;

    internal TaskEventHandler(int pid, ChannelWriter<ValueTrace> writer)
    {
        _pid = pid;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        var parser = new TplEtwProviderTraceEventParser(source);
        lifetime.Bracket(
            () => parser.TaskScheduledSend += HandleTaskScheduledSendEvent,
            () => parser.TaskScheduledSend -= HandleTaskScheduledSendEvent
        );
        lifetime.Bracket(
            () => parser.TaskExecuteStart += HandleTaskExecuteStartEvent,
            () => parser.TaskExecuteStart -= HandleTaskExecuteStartEvent
        );
        lifetime.Bracket(
            () => parser.TaskExecuteStop += HandleTaskExecuteStopEvent,
            () => parser.TaskExecuteStop -= HandleTaskExecuteStopEvent
        );
        lifetime.Bracket(
            () => parser.TaskWaitSend += HandleTaskWaitSendEvent,
            () => parser.TaskWaitSend -= HandleTaskWaitSendEvent
        );
    }

    private void HandleTaskScheduledSendEvent(TaskScheduledArgs evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Task Scheduled",
            PredefinedProvider.Tasks,
            evt.TimeStamp,
            $"Task ID: {evt.TaskID}"
        );
        _writer.TryWrite(trace);
    }

    private void HandleTaskExecuteStartEvent(TaskStartedArgs evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Task Started",
            PredefinedProvider.Tasks,
            evt.TimeStamp,
            $"Task ID: {evt.TaskID}"
        );
        _writer.TryWrite(trace);
    }

    private void HandleTaskExecuteStopEvent(TaskCompletedArgs evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Task Completed",
            PredefinedProvider.Tasks,
            evt.TimeStamp,
            $"Task ID: {evt.TaskID}"
        );
        _writer.TryWrite(trace);
    }

    private void HandleTaskWaitSendEvent(TaskWaitSendArgs evt)
    {
        if (evt.ProcessID != _pid) return;
        var trace = new ValueTrace(
            "Task Wait Begin",
            PredefinedProvider.Tasks,
            evt.TimeStamp,
            $"Task ID: {evt.TaskID}"
        );
        _writer.TryWrite(trace);
    }
}