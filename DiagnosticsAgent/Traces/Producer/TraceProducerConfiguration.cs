﻿using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using Microsoft.Diagnostics.NETCore.Client;

namespace DiagnosticsAgent.Traces.Producer;

internal sealed class TraceProducerConfiguration
{
    internal bool IsHttpEnabled { get; }
    internal bool IsAspNetEnabled { get; }
    internal bool IsEfEnabled { get; }
    internal bool IsExceptionsEnabled { get; }
    internal bool IsThreadsEnabled { get; }
    internal bool IsContentionsEnabled { get; }
    internal bool IsTasksEnabled { get; }
    internal bool IsLoaderEnabled { get; }
    private IReadOnlyCollection<EventPipeProvider> EventPipeProviders { get; }

    internal TraceProducerConfiguration(List<PredefinedProvider> providers)
    {
        var traceProviders = PredefinedProviderConverter.Convert(providers);
        EventPipeProviders = EventPipeProviderFactory.CreateTraceProviders(traceProviders);
        IsHttpEnabled = providers.Contains(PredefinedProvider.Http);
        IsAspNetEnabled = providers.Contains(PredefinedProvider.AspNet);
        IsEfEnabled = providers.Contains(PredefinedProvider.EF);
        IsExceptionsEnabled = providers.Contains(PredefinedProvider.Exceptions);
        IsThreadsEnabled = providers.Contains(PredefinedProvider.Threads);
        IsContentionsEnabled = providers.Contains(PredefinedProvider.Contentions);
        IsTasksEnabled = providers.Contains(PredefinedProvider.Tasks);
        IsLoaderEnabled = providers.Contains(PredefinedProvider.Loader);
    }

    internal EventPipeSessionConfiguration GetSessionConfiguration() => new(EventPipeProviders, false);
}