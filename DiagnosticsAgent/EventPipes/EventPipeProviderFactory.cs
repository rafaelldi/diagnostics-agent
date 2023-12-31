﻿using System.Diagnostics.Tracing;
using DiagnosticsAgent.Traces;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgent.EventPipes;

internal static class EventPipeProviderFactory
{
    private const string IntervalArgument = "EventCounterIntervalSec";
    private const string SessionIdArgument = "SessionId";
    private const string MetricsArgument = "Metrics";
    private const string RefreshIntervalArgument = "RefreshInterval";
    private const string MaxTimeSeriesArgument = "MaxTimeSeries";
    private const string MaxHistogramsArgument = "MaxHistograms";

    internal static EventPipeProvider CreateCounterProvider(string provider, int interval = 1) =>
        new(
            provider,
            EventLevel.Informational,
            (long)EventKeywords.None,
            new Dictionary<string, string> { [IntervalArgument] = interval.ToString() }
        );

    internal static EventPipeProvider[] CreateCounterProviders(IReadOnlyCollection<string> providers, int interval = 1)
    {
        var providerArguments = new Dictionary<string, string>
        {
            [IntervalArgument] = interval.ToString()
        };

        return providers.Select(it => new EventPipeProvider(
            it,
            EventLevel.Informational,
            (long)EventKeywords.None,
            providerArguments
        )).ToArray();
    }

    internal static EventPipeProvider CreateMetricProvider(
        string sessionId,
        string metrics,
        int interval,
        int maxTimeSeries,
        int maxHistograms
    ) =>
        new(
            SystemDiagnosticsMetricsProvider,
            EventLevel.Informational,
            2L,
            new Dictionary<string, string>
            {
                [SessionIdArgument] = sessionId,
                [MetricsArgument] = metrics,
                [RefreshIntervalArgument] = interval.ToString(),
                [MaxTimeSeriesArgument] = maxTimeSeries.ToString(),
                [MaxHistogramsArgument] = maxHistograms.ToString()
            }
        );

    internal static EventPipeProvider CreateTraceProvider(
        string name,
        EventLevel level,
        long flags,
        Dictionary<string, string>? arguments = null
    ) => new(
        name,
        level,
        flags,
        arguments
    );

    internal static EventPipeProvider[] CreateTraceProviders(IReadOnlyCollection<TraceProvider> providers) =>
        providers.Select(it => new EventPipeProvider(
                it.Name,
                it.Level,
                it.Flags,
                it.Arguments
            ))
            .ToArray();

    internal static EventPipeProvider CreateSampleProvider() => new(
        SampleProfilerProvider,
        EventLevel.Informational
    );

    internal static EventPipeProvider CreateGcProvider() => new(
        DotNetRuntimeProvider,
        EventLevel.Informational,
        (long)ClrTraceEventParser.Keywords.GC
    );
}