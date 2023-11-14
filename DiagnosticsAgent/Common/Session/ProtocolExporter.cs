using System.Threading.Channels;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Common.Session;

internal abstract class ProtocolExporter<TSession, TValue> : IValueConsumer where TSession : Model.Session
{
    private readonly ChannelReader<TValue> _reader;
    protected readonly TSession Session;

    protected ProtocolExporter(TSession session, ChannelReader<TValue> reader)
    {
        _reader = reader;
        Session = session;
    }

    public async Task Consume()
    {
        try
        {
            await foreach (var value in _reader.ReadAllAsync(Lifetime.AsyncLocal.Value))
            {
                Export(value);
            }
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
    }

    protected abstract void Export(TValue value);
}