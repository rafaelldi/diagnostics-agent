using AutoFixture.Xunit2;
using DiagnosticsAgent.Counters.Producer;
using FluentAssertions;
using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgentTests.Counters;

public class CounterProducerConfigurationTests
{
    [Theory]
    [AutoData]
    public void Default_configuration(string sessionId, int refreshInterval)
    {
        var configuration = new CounterProducerConfiguration(sessionId, string.Empty, null, refreshInterval, 0, 0);

        configuration.SessionId.Should().Be(sessionId);
        configuration.RefreshInterval.Should().Be(refreshInterval);
        configuration.EventPipeProviders.Count.Should().Be(1);
        var provider = configuration.EventPipeProviders.Single();
        provider.Name.Should().Be(SystemRuntimeProvider);
    }

    [Theory]
    [AutoData]
    public void Counters_only_configuration(string sessionId, int refreshInterval)
    {
        var counterProvider = "CounterProvider";
        var configuration = new CounterProducerConfiguration(sessionId, counterProvider, null, refreshInterval, 0, 0);

        configuration.EventPipeProviders.Count.Should().Be(1);
        var provider = configuration.EventPipeProviders.Single();
        provider.Name.Should().Be(counterProvider);
    }

    [Theory]
    [AutoData]
    public void Metrics_only_configuration(string sessionId, int refreshInterval)
    {
        var meter = "Meter";
        var configuration = new CounterProducerConfiguration(sessionId, string.Empty, meter, refreshInterval, 0, 0);

        configuration.EventPipeProviders.Count.Should().Be(1);
        var provider = configuration.EventPipeProviders.Single();
        provider.Name.Should().Be(SystemDiagnosticsMetricsProvider);
    }

    [Theory]
    [AutoData]
    public void Counters_and_metrics_configuration(string sessionId, int refreshInterval)
    {
        var counterProvider = "CounterProvider";
        var meter = "Meter";
        var configuration = new CounterProducerConfiguration(sessionId, counterProvider, meter, refreshInterval, 0, 0);

        configuration.EventPipeProviders.Count.Should().Be(2);
        var providerNames = configuration.EventPipeProviders.Select(it => it.Name).ToList();
        providerNames.Should().Contain(counterProvider);
        providerNames.Should().Contain(SystemDiagnosticsMetricsProvider);
    }
}