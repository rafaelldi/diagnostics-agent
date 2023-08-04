using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using static DiagnosticsAgent.Counters.Producer.ValueCounterMappers;

namespace DiagnosticsAgent.Counters.Producer.EventHandlers;

internal sealed class CounterEventHandler : IEventPipeEventHandler
{
    private readonly int _pid;
    private readonly int _refreshInterval;
    private readonly Func<string, string, bool> _isCounterEnabled;
    private readonly ChannelWriter<ValueCounter> _writer;
    private const string EventName = "EventCounters";

    public CounterEventHandler(
        int pid,
        int refreshInterval,
        Func<string, string, bool> isCounterEnabled,
        ChannelWriter<ValueCounter> writer)
    {
        _pid = pid;
        _refreshInterval = refreshInterval;
        _isCounterEnabled = isCounterEnabled;
        _writer = writer;
    }

    public void SubscribeToEvents(EventPipeEventSource source, Lifetime lifetime)
    {
        lifetime.Bracket(
            () => source.Dynamic.All += HandleEvent,
            () => source.Dynamic.All -= HandleEvent
        );
    }

    private void HandleEvent(TraceEvent evt)
    {
        if (evt.ProcessID != _pid)
        {
            return;
        }

        if (evt.EventName != EventName)
        {
            return;
        }

        var payloadVal = (IDictionary<string, object>)evt.PayloadValue(0);
        var payloadFields = (IDictionary<string, object>)payloadVal["Payload"];

        var name = payloadFields["Name"].ToString();
        if (name is null || !_isCounterEnabled(evt.ProviderName, name))
        {
            return;
        }

        var counter = MapCounterEvent(evt.ProviderName, name, evt.TimeStamp, _refreshInterval, payloadFields);
        _writer.TryWrite(counter);
    }

    private static ValueCounter MapCounterEvent(string providerName, string name, DateTime timeStamp,
        int refreshInterval, IDictionary<string, object> payloadFields)
    {
        var displayName = payloadFields["DisplayName"].ToString();
        displayName = string.IsNullOrEmpty(displayName) ? name : displayName;
        var displayUnits = payloadFields["DisplayUnits"].ToString();

        if (payloadFields["CounterType"].ToString() == "Sum")
        {
            var value = (double)payloadFields["Increment"];
            return MapToRateCounter(timeStamp, displayName, displayUnits, providerName, value, null, refreshInterval);
        }
        else
        {
            var value = (double)payloadFields["Mean"];
            return MapToMetricCounter(timeStamp, displayName, displayUnits, providerName, value, null);
        }
    }
}