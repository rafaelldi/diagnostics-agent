using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Chart;

internal readonly record struct ValueChartEvent(
    DateTime TimeStamp,
    ChartEventType Type,
    double Value,
    string? Label
);