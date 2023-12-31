﻿namespace DiagnosticsAgent.Counters;

internal readonly record struct ValueCounter(
    DateTime TimeStamp,
    string Name,
    string DisplayName,
    string ProviderName,
    double Value,
    CounterType Type,
    string? Tags
);