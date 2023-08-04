using System.Reflection;
using Microsoft.Diagnostics.NETCore.Client;

namespace DiagnosticsAgent.Common;

internal static class DiagnosticsClientExtensions
{
    private static readonly Type DiagnosticsClientType = typeof(DiagnosticsClient);

    private static readonly MethodInfo? GetProcessInfoMethod =
        DiagnosticsClientType.GetMethod("GetProcessInfo", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly Type ProcessInfoType =
        DiagnosticsClientType.Assembly.GetType("Microsoft.Diagnostics.NETCore.Client.ProcessInfo")!;

    private static readonly PropertyInfo? ProcessInfoCommandLineProperty =
        ProcessInfoType.GetProperty("CommandLine");

    private static readonly PropertyInfo? ProcessInfoOperatingSystemProperty =
        ProcessInfoType.GetProperty("OperatingSystem");

    private static readonly PropertyInfo? ProcessInfoProcessArchitectureProperty =
        ProcessInfoType.GetProperty("ProcessArchitecture");

    private static readonly MethodInfo? StartEventPipeSessionAsyncMethod =
        DiagnosticsClientType.GetMethod(
            "StartEventPipeSessionAsync",
            BindingFlags.NonPublic | BindingFlags.Instance,
            new[] { typeof(IEnumerable<EventPipeProvider>), typeof(bool), typeof(int), typeof(CancellationToken) });

    internal static (string? CommandLine, string? OperatingSystem, string? ProcessArchitecture) GetProcessInfo(
        this DiagnosticsClient client)
    {
        var processInfo = GetProcessInfoMethod?.Invoke(client, Array.Empty<object>());
        if (processInfo is null)
        {
            return (null, null, null);
        }

        var commandLine = (string?)ProcessInfoCommandLineProperty?.GetValue(processInfo);
        var operatingSystem = (string?)ProcessInfoOperatingSystemProperty?.GetValue(processInfo);
        var processArchitecture = (string?)ProcessInfoProcessArchitectureProperty?.GetValue(processInfo);

        return (commandLine, operatingSystem, processArchitecture);
    }

    internal static Task<EventPipeSession> StartEventPipeSessionAsync(this DiagnosticsClient client,
        IEnumerable<EventPipeProvider> providers, bool requestRundown, int circularBufferMb, CancellationToken token)
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        return (Task<EventPipeSession>)StartEventPipeSessionAsyncMethod?.Invoke(
            client,
            new object[] { providers, requestRundown, circularBufferMb, token }
        )!;
    }
}