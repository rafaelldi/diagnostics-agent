using Microsoft.Diagnostics.NETCore.Client;

namespace DiagnosticsAgent.EventPipes;

internal sealed record EventPipeSessionConfiguration(
    IReadOnlyCollection<EventPipeProvider> EventPipeProviders,
    bool RequestRundown = true,
    int CircularBufferMb = 256
);