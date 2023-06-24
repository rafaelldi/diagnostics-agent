using System.Diagnostics.Tracing;
using DiagnosticsAgent.Model;
using static Microsoft.Diagnostics.Tracing.Parsers.ClrTraceEventParser;
using static DiagnosticsAgent.Common.Providers;
using static DiagnosticsAgent.Common.DiagnosticSourceFilterAndPayloadSpecs;
using static DiagnosticsAgent.Common.DiagnosticSourceKeywords;

namespace DiagnosticsAgent.Traces;

internal static class PredefinedProviderConverter
{
    internal static List<TraceProvider> Convert(List<PredefinedProvider> predefinedProviders)
    {
        var providers = predefinedProviders
            .Select(ConvertPredefinedProvider)
            .Where(it => it != null)
            .ToList();

        var filterAndPayloadSpecs = GetFilterAndPayloadSpecs(predefinedProviders);
        if (!filterAndPayloadSpecs.Any()) return providers!;

        var filterAndPayloadString = string.Join("\n", filterAndPayloadSpecs);
        providers.Add(new TraceProvider(MicrosoftDiagnosticSource, EventLevel.Verbose, (long)Events,
            new Dictionary<string, string> { { FilterAndPayloadSpecs, filterAndPayloadString } })
        );

        return providers!;
    }

    private static TraceProvider? ConvertPredefinedProvider(PredefinedProvider provider) =>
        provider switch
        {
            PredefinedProvider.Exceptions =>
                new TraceProvider(DotNetRuntimeProvider, EventLevel.Informational, (long)Keywords.Exception),
            PredefinedProvider.Threads =>
                new TraceProvider(DotNetRuntimeProvider, EventLevel.Informational, (long)Keywords.Threading),
            PredefinedProvider.Contentions =>
                new TraceProvider(DotNetRuntimeProvider, EventLevel.Informational, (long)Keywords.Contention),
            PredefinedProvider.Tasks =>
                new TraceProvider(TplEventSourceProvider, EventLevel.Informational, (long)EventKeywords.All),
            PredefinedProvider.Loader =>
                new TraceProvider(DotNetRuntimeProvider, EventLevel.Informational, (long)Keywords.Loader),
            _ => null
        };

    private static List<string> GetFilterAndPayloadSpecs(ICollection<PredefinedProvider> predefinedProviders)
    {
        var filterAndPayloadSpecs = new List<string>();

        if (predefinedProviders.Contains(PredefinedProvider.Http))
        {
            filterAndPayloadSpecs.AddRange(Http);
        }

        if (predefinedProviders.Contains(PredefinedProvider.AspNet))
        {
            filterAndPayloadSpecs.AddRange(AspNetCore);
        }

        if (predefinedProviders.Contains(PredefinedProvider.EF))
        {
            filterAndPayloadSpecs.AddRange(EntityFrameworkCore);
        }

        return filterAndPayloadSpecs;
    }
}