using System.Threading.Channels;
using DiagnosticsAgent.Common.Session;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Traces.Producer.EventHandlers;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;

namespace DiagnosticsAgent.Traces.Producer;

internal sealed class TraceProducer : IValueProducer
{
    private readonly EventPipeSessionProvider _sessionProvider;
    private readonly EventPipeSessionConfiguration _sessionConfiguration;
    private readonly List<IEventPipeEventHandler> _handlers;

    internal TraceProducer(
        int pid,
        TraceProducerConfiguration configuration,
        ChannelWriter<ValueTrace> writer,
        Lifetime lifetime)
    {
        _sessionProvider = new EventPipeSessionProvider(pid);
        _sessionConfiguration = configuration.GetSessionConfiguration();

        _handlers = new List<IEventPipeEventHandler>(8);

        if (configuration.IsHttpEnabled)
        {
            _handlers.Add(new HttpEventHandler(pid, writer));
        }

        if (configuration.IsAspNetEnabled)
        {
            _handlers.Add(new AspNetEventHandler(pid, writer));
        }

        if (configuration.IsEfEnabled)
        {
            _handlers.Add(new EfEventHandler(pid, writer));
        }

        if (configuration.IsExceptionsEnabled)
        {
            _handlers.Add(new ExceptionEventHandler(pid, writer));
        }

        if (configuration.IsThreadsEnabled)
        {
            _handlers.Add(new ThreadEventHandler(pid, writer));
        }

        if (configuration.IsContentionsEnabled)
        {
            _handlers.Add(new ContentionEventHandler(pid, writer));
        }

        if (configuration.IsTasksEnabled)
        {
            _handlers.Add(new TaskEventHandler(pid, writer));
        }

        if (configuration.IsLoaderEnabled)
        {
            _handlers.Add(new LoaderEventHandler(pid, writer));
        }

        lifetime.OnTermination(() => writer.Complete());
    }

    public async Task ProduceAsync()
    {
        await _sessionProvider.RunSessionAndSubscribeAsync(
            _sessionConfiguration,
            Lifetime.AsyncLocal.Value,
            SubscribeToEvents
        );
    }

    private void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        foreach (var handler in _handlers)
        {
            handler.SubscribeToEvents(source, lifetime);
        }
    }
}