using System.Diagnostics.Tracing;
using DiagnosticsAgent.Model;
using static Microsoft.Diagnostics.Tracing.Parsers.ClrTraceEventParser;
using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgent.Traces;

internal static class TraceProfileConverter
{
    internal static TraceProvider[] Convert(TracingProfile profile) =>
        profile switch
        {
            TracingProfile.None => Array.Empty<TraceProvider>(),
            TracingProfile.CpuSampling => new[]
            {
                new TraceProvider(SampleProfilerProvider, EventLevel.Informational, 0xF00000000000),
                new TraceProvider(DotNetRuntimeProvider, EventLevel.Informational, (long)Keywords.Default)
            },
            TracingProfile.GcVerbose => new[]
            {
                new TraceProvider(
                    DotNetRuntimeProvider,
                    EventLevel.Verbose,
                    (long)Keywords.GC | (long)Keywords.GCHandle | (long)Keywords.Exception
                )
            },
            TracingProfile.GcCollect => new[]
            {
                new TraceProvider(DotNetRuntimeProvider, EventLevel.Informational, (long)Keywords.GC)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
        };
}