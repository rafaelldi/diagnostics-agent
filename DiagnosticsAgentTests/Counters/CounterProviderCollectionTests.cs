using DiagnosticsAgent.Counters.Producer;
using FluentAssertions;
using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgentTests.Counters;

public class CounterProviderCollectionTests
{
    [Fact]
    public void Default_provider()
    {
        var collection = new CounterProviderCollection();
        var providers = collection.Providers();

        providers.Count.Should().Be(1);
        providers.Should().Contain(SystemRuntimeProvider);
        collection.Count().Should().Be(1);
        collection.Contains(SystemRuntimeProvider, "custom-counter").Should().BeTrue();
        collection.Contains("CustomProvider", "custom-counter").Should().BeFalse();
    }

    [Fact]
    public void One_provider_without_counters()
    {
        var customProvider = "CustomProvider";
        var collection = new CounterProviderCollection(customProvider);
        var providers = collection.Providers();

        providers.Count.Should().Be(1);
        providers.Should().Contain(customProvider);
        collection.Count().Should().Be(1);
        collection.Contains(customProvider, "custom-counter").Should().BeTrue();
        collection.Contains("AnotherCustomProvider", "custom-counter").Should().BeFalse();
    }

    [Fact]
    public void One_provider_with_counter()
    {
        var customProvider = "CustomProvider";
        var customCounter = "custom-counter";
        var collection = new CounterProviderCollection($"{customProvider}[{customCounter}]");
        var providers = collection.Providers();

        providers.Count.Should().Be(1);
        providers.Should().Contain(customProvider);
        collection.Count().Should().Be(1);
        collection.Contains(customProvider, customCounter).Should().BeTrue();
        collection.Contains(customProvider, "another-custom-counter").Should().BeFalse();
    }

    [Fact]
    public void One_provider_with_multiple_counters()
    {
        var customProvider = "CustomProvider";
        var customCounter = "custom-counter";
        var anotherCustomCounter = "another-custom-counter";
        var collection = new CounterProviderCollection($"{customProvider}[{customCounter},{anotherCustomCounter}]");
        var providers = collection.Providers();

        providers.Count.Should().Be(1);
        providers.Should().Contain(customProvider);
        collection.Count().Should().Be(1);
        collection.Contains(customProvider, customCounter).Should().BeTrue();
        collection.Contains(customProvider, anotherCustomCounter).Should().BeTrue();
    }

    [Fact]
    public void Multiple_providers_with_counters()
    {
        var customProvider = "CustomProvider";
        var customCounter = "custom-counter";
        var anotherCustomProvider = "AnotherCustomProvider";
        var anotherCustomCounter = "another-custom-counter";
        var collection = new CounterProviderCollection(
            $"{customProvider}[{customCounter}],{anotherCustomProvider}[{anotherCustomCounter}]");
        var providers = collection.Providers();

        providers.Count.Should().Be(2);
        providers.Should().Contain(customProvider);
        providers.Should().Contain(anotherCustomProvider);
        collection.Count().Should().Be(2);
        collection.Contains(customProvider, customCounter).Should().BeTrue();
        collection.Contains(anotherCustomProvider, anotherCustomCounter).Should().BeTrue();
    }
}