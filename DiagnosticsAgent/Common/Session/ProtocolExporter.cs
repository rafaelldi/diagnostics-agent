using System.Threading.Channels;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Common.Session;

internal abstract class ProtocolExporter<TSession, TValue>(TSession session, ChannelReader<TValue> reader)
    : IValueConsumer where TSession : Model.Session
{
    protected readonly TSession Session = session;

    public async Task Consume()
    {
        try
        {
            await foreach (var value in reader.ReadAllAsync(Lifetime.AsyncLocal.Value))
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