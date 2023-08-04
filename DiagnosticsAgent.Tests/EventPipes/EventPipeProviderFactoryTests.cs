using System.Diagnostics.Tracing;
using AutoFixture.Xunit2;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Traces;
using FluentAssertions;
using Microsoft.Diagnostics.Tracing.Parsers;
using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgentTests.EventPipes;

public class EventPipeProviderFactoryTests
{
    [Theory]
    [AutoData]
    public void Create_counter_providers(int interval)
    {
        var counterProvider = "CounterProvider";

        var eventPipeProviders = EventPipeProviderFactory.CreateCounterProviders(new[] { counterProvider }, interval);

        eventPipeProviders.Length.Should().Be(1);
        var eventPipeProvider = eventPipeProviders.Single();
        eventPipeProvider.Name.Should().Be(counterProvider);
        eventPipeProvider.EventLevel.Should().Be(EventLevel.Informational);
        eventPipeProvider.Keywords.Should().Be((long)EventKeywords.None);
        eventPipeProvider.Arguments["EventCounterIntervalSec"].Should().Be(interval.ToString());
    }

    [Theory]
    [AutoData]
    public void Create_metric_provider(string sessionId, int interval, int maxTimeSeries, int maxHistograms)
    {
        var meter = "Meter";

        var eventPipeProvider =
            EventPipeProviderFactory.CreateMetricProvider(sessionId, meter, interval, maxTimeSeries, maxHistograms);

        eventPipeProvider.Should().NotBeNull();
        eventPipeProvider.Name.Should().Be(SystemDiagnosticsMetricsProvider);
        eventPipeProvider.EventLevel.Should().Be(EventLevel.Informational);
        eventPipeProvider.Keywords.Should().Be(2L);
        eventPipeProvider.Arguments["SessionId"].Should().Be(sessionId);
        eventPipeProvider.Arguments["Metrics"].Should().Be(meter);
        eventPipeProvider.Arguments["RefreshInterval"].Should().Be(interval.ToString());
        eventPipeProvider.Arguments["MaxTimeSeries"].Should().Be(maxTimeSeries.ToString());
        eventPipeProvider.Arguments["MaxHistograms"].Should().Be(maxHistograms.ToString());
    }

    [Theory]
    [AutoData]
    public void Create_trace_providers(string name, EventLevel level, long flags, Dictionary<string, string> arguments)
    {
        var traceProviders = new[] { new TraceProvider(name, level, flags, arguments) };

        var eventPipeProviders = EventPipeProviderFactory.CreateTraceProviders(traceProviders);

        eventPipeProviders.Length.Should().Be(1);
        var eventPipeProvider = eventPipeProviders.Single();
        eventPipeProvider.Name.Should().Be(name);
        eventPipeProvider.EventLevel.Should().Be(level);
        eventPipeProvider.Keywords.Should().Be(flags);
        eventPipeProvider.Arguments.Should().Equal(arguments);
    }

    [Fact]
    public void Create_gc_provider()
    {
        var eventPipeProvider = EventPipeProviderFactory.CreateGcProvider();

        eventPipeProvider.Name.Should().Be(DotNetRuntimeProvider);
        eventPipeProvider.EventLevel.Should().Be(EventLevel.Informational);
        eventPipeProvider.Keywords.Should().Be((long)ClrTraceEventParser.Keywords.GC);
    }
}