using DiagnosticsAgent.Counters.Producer;
using FluentAssertions;

namespace DiagnosticsAgentTests.Counters;

public class CounterCollectionParserTests
{
    [Fact]
    public void Parse_empty_string()
    {
        var countersString = string.Empty;
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        counters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_empty_string_with_delimiter()
    {
        var countersString = ",";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        counters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_one_provider()
    {
        var countersString = "System.Runtime";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());

        counters.Keys.Should().Contain("System.Runtime");
        counters["System.Runtime"].Should().BeNull();
    }

    [Fact]
    public void Parse_one_provider_with_empty_metrics()
    {
        var countersString = "System.Runtime[]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());

        counters.Keys.Should().Contain("System.Runtime");
        counters["System.Runtime"].Should().BeNull();
    }

    [Fact]
    public void Parse_one_provider_with_empty_metrics_and_delimiter()
    {
        var countersString = "System.Runtime[,]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());

        counters.Keys.Should().Contain("System.Runtime");
        counters["System.Runtime"].Should().BeNull();
    }

    [Fact]
    public void Parse_one_provider_with_metrics()
    {
        var countersString = "System.Runtime[cpu-usage,alloc-rate,exception-count]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());

        counters.Keys.Should().Contain("System.Runtime");
        var provider = counters["System.Runtime"];
        provider.Should().NotBeNull();
        provider.Should().NotBeEmpty();
        provider.Should().Contain("cpu-usage");
        provider.Should().Contain("alloc-rate");
        provider.Should().Contain("exception-count");
    }

    [Fact]
    public void Parse_one_provider_with_empty_second_metric()
    {
        var countersString = "System.Runtime[cpu-usage,]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        var provider = counters["System.Runtime"];

        provider.Should().NotBeNull();
        provider.Should().NotBeEmpty();
        provider.Should().Contain("cpu-usage");
        provider!.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_one_provider_with_empty_first_metric()
    {
        var countersString = "System.Runtime[,cpu-usage]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        var provider = counters["System.Runtime"];

        provider.Should().NotBeNull();
        provider.Should().NotBeEmpty();
        provider.Should().Contain("cpu-usage");
        provider!.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_multiple_providers_with_empty_second_one()
    {
        var countersString = "System.Runtime[cpu-usage],";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        var provider = counters["System.Runtime"];

        provider.Should().NotBeNull();
        provider.Should().NotBeEmpty();
        provider.Should().Contain("cpu-usage");
        counters.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_multiple_providers_with_empty_first_one()
    {
        var countersString = ",System.Runtime[cpu-usage]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        var provider = counters["System.Runtime"];

        provider.Should().NotBeNull();
        provider.Should().NotBeEmpty();
        provider.Should().Contain("cpu-usage");
        counters.Count.Should().Be(1);
    }

    [Fact]
    public void Parse_multiple_providers()
    {
        var countersString = "System.Runtime[cpu-usage],MyEventCounterSource";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        var provider = counters["System.Runtime"];

        provider.Should().NotBeNull();
        provider.Should().NotBeEmpty();
        provider.Should().Contain("cpu-usage");
        counters.Keys.Should().Contain("MyEventCounterSource");
        counters["MyEventCounterSource"].Should().BeNull();
    }

    [Fact]
    public void Parse_multiple_providers_with_metrics()
    {
        var countersString = "System.Runtime[cpu-usage],MyEventCounterSource[my-counter]";
        var counters = CounterCollectionParser.Parse(countersString.AsSpan());
        var runtimeProvider = counters["System.Runtime"];

        runtimeProvider.Should().NotBeNull();
        runtimeProvider.Should().NotBeEmpty();
        runtimeProvider.Should().Contain("cpu-usage");
        var customProvider = counters["MyEventCounterSource"];
        customProvider.Should().NotBeNull();
        customProvider.Should().NotBeEmpty();
        customProvider.Should().Contain("my-counter");
    }
}