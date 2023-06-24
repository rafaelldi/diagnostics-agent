using System.Threading.Channels;
using DiagnosticsAgent.Counters;
using DiagnosticsAgent.Counters.Producer;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Chart;

internal sealed class LiveChartSessionEnvelope
{
    private readonly ChartProtocolExporter _exporter;
    private readonly CounterProducer _producer;

    internal LiveChartSessionEnvelope(int pid, LiveChartSession session, Lifetime lifetime)
    {
        var channel = Channel.CreateBounded<ValueCounter>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        _exporter = new ChartProtocolExporter(session, channel.Reader);
        var configuration = new CounterProducerConfiguration(
            Guid.NewGuid().ToString(),
            "System.Runtime[cpu-usage,gc-heap-size,working-set]",
            null,
            1,
            1000,
            10
        );
        _producer = new CounterProducer(pid, configuration, channel.Writer, lifetime);

        session.Active.WhenTrue(lifetime, Handle);
    }

    private void Handle(Lifetime lt)
    {
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await _exporter.ConsumeAsync());
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await _producer.Produce());
    }
}