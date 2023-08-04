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
    private readonly TraceProducerConfiguration _configuration;
    private readonly List<IEventPipeEventHandler> _handlers;

    internal TraceProducer(
        int pid,
        TraceProducerConfiguration configuration,
        ChannelWriter<ValueTrace> writer,
        Lifetime lifetime)
    {
        _sessionProvider = new EventPipeSessionProvider(pid);
        _configuration = configuration;

        _handlers = new List<IEventPipeEventHandler>(8);

        if (_configuration.IsHttpEnabled)
        {
            _handlers.Add(new HttpEventHandler(pid, writer));
        }

        if (_configuration.IsAspNetEnabled)
        {
            _handlers.Add(new AspNetEventHandler(pid, writer));
        }

        if (_configuration.IsEfEnabled)
        {
            _handlers.Add(new EfEventHandler(pid, writer));
        }

        if (_configuration.IsExceptionsEnabled)
        {
            _handlers.Add(new ExceptionEventHandler(pid, writer));
        }

        if (_configuration.IsThreadsEnabled)
        {
            _handlers.Add(new ThreadEventHandler(pid, writer));
        }

        if (_configuration.IsContentionsEnabled)
        {
            _handlers.Add(new ContentionEventHandler(pid, writer));
        }

        if (_configuration.IsTasksEnabled)
        {
            _handlers.Add(new TaskEventHandler(pid, writer));
        }

        if (_configuration.IsLoaderEnabled)
        {
            _handlers.Add(new LoaderEventHandler(pid, writer));
        }

        lifetime.OnTermination(() => writer.Complete());
    }

    public async Task ProduceAsync()
    {
        var sessionConfiguration = _configuration.GetSessionConfiguration();
        await _sessionProvider.RunSessionAndSubscribeAsync(
            sessionConfiguration,
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