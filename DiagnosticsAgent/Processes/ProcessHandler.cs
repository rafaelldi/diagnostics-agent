using System.Diagnostics;
using System.Globalization;
using DiagnosticsAgent.Common;
using DiagnosticsAgent.Model;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.NETCore.Client;

namespace DiagnosticsAgent.Processes;

internal static class ProcessHandler
{
    internal static void Subscribe(DiagnosticsHostModel model, Lifetime lifetime)
    {
        model.ProcessList.Active.WhenTrue(lifetime, lt => Handle(model.ProcessList, lt));
    }

    private static void Handle(ProcessList processList, Lifetime lt)
    {
        lt.StartAttachedAsync(TaskScheduler.Default, async () => await RefreshAsync(processList));
    }

    private static async Task RefreshAsync(ProcessList processList)
    {
        try
        {
            while (true)
            {
                RefreshProcessList(processList);
                await Task.Delay(TimeSpan.FromSeconds(3), Lifetime.AsyncLocal.Value);
            }
        }
        catch (OperationCanceledException)
        {
            processList.Items.Clear();
        }
    }

    private static void RefreshProcessList(ProcessList processList)
    {
        var processes = DiagnosticsClient.GetPublishedProcesses().ToList();

        var existingProcesses = processList.Items.Keys.ToArray();
        var newProcesses = new Dictionary<int, ProcessInfo>(processes.Count);
        foreach (var pid in processes.Where(pid => !existingProcesses.Contains(pid)))
        {
            try
            {
                var process = Process.GetProcessById(pid);
                var client = new DiagnosticsClient(pid);
                var additionalProcessInfo = client.GetProcessInfo();
                var filename = process.MainModule?.FileName;
                var startTime = process.StartTime.ToString(CultureInfo.CurrentCulture);
                var environment = client
                    .GetProcessEnvironment()
                    .Select(it => new ProcessEnvironmentVariable(it.Key, it.Value))
                    .ToArray();

                var processInfo = new ProcessInfo(
                    process.ProcessName,
                    filename,
                    startTime,
                    additionalProcessInfo.CommandLine,
                    additionalProcessInfo.OperatingSystem,
                    additionalProcessInfo.ProcessArchitecture,
                    environment
                );

                newProcesses[pid] = processInfo;
            }
            catch (ArgumentException)
            {
                //The identifier might be expired.
            }
        }

        foreach (var createdProcess in newProcesses)
        {
            processList.Items.Add(createdProcess);
        }

        foreach (var removedProcess in existingProcesses.Except(processes))
        {
            processList.Items.Remove(removedProcess);
        }
    }
}