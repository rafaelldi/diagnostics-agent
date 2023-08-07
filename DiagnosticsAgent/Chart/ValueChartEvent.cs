using DiagnosticsAgent.Model;

namespace DiagnosticsAgent.Chart;

internal readonly record struct ValueChartEvent(
    DateTime TimeStamp,
    ChartValueType Type,
    double Value
);