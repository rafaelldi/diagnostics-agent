using System.Diagnostics.Tracing;

namespace DiagnosticsAgent.Traces;

internal sealed record TraceProvider(
    string Name,
    EventLevel Level,
    long Flags,
    Dictionary<string, string>? Arguments = null
);