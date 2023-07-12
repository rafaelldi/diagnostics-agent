using System.Threading.Channels;
using DiagnosticsAgent.Model;
using JetBrains.Annotations;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Common.Session;

internal abstract class ProtocolSessionEnvelope<TSession, TValue> where TSession : ProtocolSession
{
    private readonly IValueConsumer _consumer;
    private readonly IValueProducer _producer;

    protected ProtocolSessionEnvelope(int pid, TSession session, Lifetime lifetime)
    {
        var channel = Channel.CreateBounded<TValue>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        // ReSharper disable once VirtualMemberCallInConstructor
        _consumer = CreateConsumer(session, channel.Reader);
        // ReSharper disable once VirtualMemberCallInConstructor
        _producer = CreateProducer(pid, session, channel.Writer, lifetime);

        session.Active.WhenTrue(lifetime, Handle);
    }

    [Pure]
    protected abstract IValueConsumer CreateConsumer(TSession session, ChannelReader<TValue> reader);

    [Pure]
    protected abstract IValueProducer CreateProducer(int pid, TSession session, ChannelWriter<TValue> writer, Lifetime lifetime);

    private void Handle(Lifetime lt)
    {
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await _consumer.Consume());
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await _producer.Produce());
    }
}