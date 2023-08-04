using DiagnosticsAgent.Counters.Producer;
using FluentAssertions;

namespace DiagnosticsAgentTests.Counters;

public class MetricCollectionTests
{
    [Fact]
    public void Empty_meter_list()
    {
        var collection = new MetricCollection(string.Empty);
        collection.Metrics.Should().BeEmpty();
    }

    [Fact]
    public void One_meter_without_metrics()
    {
        var customMeter = "CustomMeter";
        var collection = new MetricCollection(customMeter);
        collection.Metrics.Should().Be(customMeter);
    }

    [Fact]
    public void One_meter_with_metric()
    {
        var customMeter = "CustomMeter";
        var customMetric = "custom-metric";
        var collection = new MetricCollection($"{customMeter}[{customMetric}]");
        collection.Metrics.Should().Be($"{customMeter}\\{customMetric}");
    }

    [Fact]
    public void One_meter_with_multiple_metrics()
    {
        var customMeter = "CustomMeter";
        var customMetric = "custom-metric";
        var anotherCustomMetric = "another-custom-metric";
        var collection = new MetricCollection($"{customMeter}[{customMetric},{anotherCustomMetric}]");
        collection.Metrics.Should().Be($"{customMeter}\\{customMetric},{customMeter}\\{anotherCustomMetric}");
    }

    [Fact]
    public void Multiple_meters_with_metrics()
    {
        var customMeter = "CustomMeter";
        var customMetric = "custom-metric";
        var anotherCustomMeter = "AnotherCustomMeter";
        var anotherCustomMetric = "another-custom-metric";
        var collection = new MetricCollection(
            $"{customMeter}[{customMetric}],{anotherCustomMeter}[{anotherCustomMetric}]");
        collection.Metrics.Should().Be($"{customMeter}\\{customMetric},{anotherCustomMeter}\\{anotherCustomMetric}");
    }
}