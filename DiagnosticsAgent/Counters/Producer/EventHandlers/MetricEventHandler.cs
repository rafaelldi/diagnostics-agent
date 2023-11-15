using System.Globalization;
using System.Threading.Channels;
using DiagnosticsAgent.Common;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using static DiagnosticsAgent.Counters.Producer.ValueCounterMappers;

namespace DiagnosticsAgent.Counters.Producer.EventHandlers;

internal sealed class MetricEventHandler(
    int pid,
    int refreshInterval,
    string id,
    Func<string, string, bool> isMetricEnabled,
    ChannelWriter<ValueCounter> writer)
    : IEventPipeEventHandler
{
    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        lifetime.Bracket(
            () => source.Dynamic.All += HandleEvent,
            () => source.Dynamic.All -= HandleEvent
        );
    }

    private void HandleEvent(TraceEvent evt)
    {
        if (evt.ProcessID != pid)
        {
            return;
        }

        if (evt.ProviderName != Providers.SystemDiagnosticsMetricsProvider)
        {
            return;
        }

        switch (evt.EventName)
        {
            case "CounterRateValuePublished":
                HandleCounterRateEvent(evt);
                break;
            case "GaugeValuePublished":
                HandleGaugeEvent(evt);
                break;
            case "HistogramValuePublished":
                HandleHistogramEvent(evt);
                break;
            case "UpDownCounterRateValuePublished":
                HandleUpDownCounterRateEvent(evt);
                break;
        }
    }

    private void HandleCounterRateEvent(TraceEvent evt)
    {
        var sessionId = (string)evt.PayloadValue(0);
        if (sessionId != id)
        {
            return;
        }

        var rateValue = (string)evt.PayloadValue(6);
        if (!double.TryParse(rateValue, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture,
                out var rate))
        {
            return;
        }

        var meterName = (string)evt.PayloadValue(1);
        var instrumentName = (string)evt.PayloadValue(3);
        if (meterName is null || instrumentName is null || !isMetricEnabled(meterName, instrumentName))
        {
            return;
        }

        var unit = (string)evt.PayloadValue(4);
        var tags = (string)evt.PayloadValue(5);
        var counter = MapToRateCounter(evt.TimeStamp, instrumentName, unit, meterName, rate, tags,
            refreshInterval);
        writer.TryWrite(counter);
    }

    private void HandleGaugeEvent(TraceEvent evt)
    {
        var sessionId = (string)evt.PayloadValue(0);
        if (sessionId != id)
        {
            return;
        }

        var gaugeValue = (string)evt.PayloadValue(6);
        if (!double.TryParse(gaugeValue, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture,
                out var lastValue))
        {
            return;
        }

        var meterName = (string)evt.PayloadValue(1);
        var instrumentName = (string)evt.PayloadValue(3);
        if (meterName is null || instrumentName is null || !isMetricEnabled(meterName, instrumentName))
        {
            return;
        }

        var unit = (string)evt.PayloadValue(4);
        var tags = (string)evt.PayloadValue(5);
        var counter = MapToMetricCounter(evt.TimeStamp, instrumentName, unit, meterName, lastValue, tags);
        writer.TryWrite(counter);
    }

    private void HandleHistogramEvent(TraceEvent evt)
    {
        var sessionId = (string)evt.PayloadValue(0);
        if (sessionId != id)
        {
            return;
        }

        var meterName = (string)evt.PayloadValue(1);
        var instrumentName = (string)evt.PayloadValue(3);
        if (meterName is null || instrumentName is null || !isMetricEnabled(meterName, instrumentName))
        {
            return;
        }

        var unit = (string)evt.PayloadValue(4);
        var tags = (string)evt.PayloadValue(5);
        var quantiles = (string)evt.PayloadValue(6);
        if (string.IsNullOrEmpty(quantiles)) return;
        var quantileValues = ParseQuantiles(quantiles.AsSpan());

        var counter = MapToMetricCounter(evt.TimeStamp, instrumentName, unit, meterName, quantileValues.Value50,
            CombineTagsAndQuantiles(tags, "Percentile=50"));
        writer.TryWrite(counter);

        counter = MapToMetricCounter(evt.TimeStamp, instrumentName, unit, meterName, quantileValues.Value95,
            CombineTagsAndQuantiles(tags, "Percentile=95"));
        writer.TryWrite(counter);

        counter = MapToMetricCounter(evt.TimeStamp, instrumentName, unit, meterName, quantileValues.Value99,
            CombineTagsAndQuantiles(tags, "Percentile=99"));
        writer.TryWrite(counter);

        static string CombineTagsAndQuantiles(string tagString, string quantileString) =>
            string.IsNullOrEmpty(tagString) ? quantileString : $"{tagString},{quantileString}";
    }

    private void HandleUpDownCounterRateEvent(TraceEvent evt)
    {
        var sessionId = (string)evt.PayloadValue(0);
        if (sessionId != id)
        {
            return;
        }

        var counterValue = (string)evt.PayloadValue(7);
        if (!double.TryParse(counterValue, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture,
                out var value))
        {
            return;
        }

        var meterName = (string)evt.PayloadValue(1);
        var instrumentName = (string)evt.PayloadValue(3);
        if (meterName is null || instrumentName is null || !isMetricEnabled(meterName, instrumentName))
        {
            return;
        }

        var unit = (string)evt.PayloadValue(4);
        var tags = (string)evt.PayloadValue(5);
        var counter = MapToMetricCounter(evt.TimeStamp, instrumentName, unit, meterName, value, tags);
        writer.TryWrite(counter);
    }

    private static Quantiles ParseQuantiles(ReadOnlySpan<char> quantiles)
    {
        var firstDelimiterIndex = quantiles.IndexOf(';');
        var value50 = ParsePair(quantiles.Slice(0, firstDelimiterIndex).Trim());

        quantiles = quantiles.Slice(firstDelimiterIndex + 1);
        var secondDelimiterIndex = quantiles.IndexOf(';');
        var value95 = ParsePair(quantiles.Slice(0, secondDelimiterIndex).Trim());

        quantiles = quantiles.Slice(secondDelimiterIndex + 1);
        var value99 = ParsePair(quantiles);

        return new Quantiles(value50, value95, value99);
    }

    private static double ParsePair(ReadOnlySpan<char> pair)
    {
        var pairDelimiter = pair.IndexOf('=');
        var valueSlice = pair.Slice(pairDelimiter + 1).Trim();
        return double.TryParse(valueSlice.ToString(), NumberStyles.Number | NumberStyles.Float,
            CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private readonly record struct Quantiles(double Value50, double Value95, double Value99);
}