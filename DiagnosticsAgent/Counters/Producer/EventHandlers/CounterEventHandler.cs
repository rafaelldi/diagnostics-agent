using System.Threading.Channels;
using DiagnosticsAgent.EventPipes;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.Tracing;
using static DiagnosticsAgent.Counters.Producer.ValueCounterMappers;

namespace DiagnosticsAgent.Counters.Producer.EventHandlers;

internal sealed class CounterEventHandler(
    int pid,
    int refreshInterval,
    Func<string, string, bool> isCounterEnabled,
    ChannelWriter<ValueCounter> writer)
    : IEventPipeEventHandler
{
    private const string EventName = "EventCounters";

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

        if (evt.EventName != EventName)
        {
            return;
        }

        var payloadVal = (IDictionary<string, object>)evt.PayloadValue(0);
        var payloadFields = (IDictionary<string, object>)payloadVal["Payload"];

        var name = payloadFields["Name"].ToString();
        if (name is null || !isCounterEnabled(evt.ProviderName, name))
        {
            return;
        }

        var counter = MapCounterEvent(evt.ProviderName, name, evt.TimeStamp, refreshInterval, payloadFields);
        writer.TryWrite(counter);
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