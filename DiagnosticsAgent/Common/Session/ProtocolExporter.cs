using System.Threading.Channels;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Common.Session;

internal abstract class ProtocolExporter<TSession, TValue> : IValueConsumer where TSession : ProtocolSession
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
                ExportToProtocol(value);
            }
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
    }

    protected abstract void ExportToProtocol(TValue value);
}