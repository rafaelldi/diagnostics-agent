using System.Text.RegularExpressions;

namespace DiagnosticsAgent.Benchmarks.Counter;

public static class CounterCollectionParserRegex
{
    private const string CounterPattern = @"^([\w\-.]+)(?:\[([\w-]+(?:,[\w-]+)*)])?(?:,([\w\-.]+)(?:\[([\w-]+(?:,[\w-]+)*)])?)*$";
    private static readonly Regex CounterRegex = new(CounterPattern);

    public static Dictionary<string, string[]?> Parse(string value)
    {
        var match = CounterRegex.Match(value);

        var result = new Dictionary<string, string[]?>();
        for (var i = 1; i < match.Groups.Count; i++)
        {
            var provider = match.Groups[i];
            var counters = match.Groups[i + 1];
            result[provider.Value] = counters.Value.Split(',');
            i++;
        }

        return result;
    }
}