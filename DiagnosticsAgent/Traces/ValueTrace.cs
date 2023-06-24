using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Traces;

internal readonly record struct ValueTrace(
    string EventName,
    PredefinedProvider Provider,
    DateTime TimeStamp, 
    string Content
);