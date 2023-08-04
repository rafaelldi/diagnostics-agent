using System.Text;
using DiagnosticsAgent.EventPipes;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using JetBrains.Rd.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Stacks;
using static JetBrains.Lifetimes.Lifetime;

namespace DiagnosticsAgent.StackTrace;

internal static class StackTraceCollectionHandler
{
    private static readonly EventPipeProvider[] Providers = { EventPipeProviderFactory.CreateSampleProvider() };
    private static readonly EventPipeSessionConfiguration SessionConfiguration = new(Providers);

    internal static void Subscribe(DiagnosticsHostModel model)
    {
        model.CollectStackTrace.SetAsync(async (lt, command) => await CollectAsync(command, lt));
    }

    private static async Task<string> CollectAsync(CollectStackTraceCommand command, Lifetime lifetime)
    {
        var sessionFilename = $"{Path.GetRandomFileName()}.nettrace";
        var sessionFilePath = Path.Combine(Path.GetTempPath(), sessionFilename);
        lifetime.OnTermination(() =>
        {
            if (File.Exists(sessionFilePath))
            {
                File.Delete(sessionFilePath);
            }
        });
        await CollectTracesAsync(command, sessionFilePath, lifetime);

        var traceLogFilePath = TraceLog.CreateFromEventPipeDataFile(sessionFilePath);
        lifetime.OnTermination(() =>
        {
            if (File.Exists(traceLogFilePath))
            {
                File.Delete(traceLogFilePath);
            }
        });
        var stackTraces = ParseSessionFile(traceLogFilePath);
        return stackTraces;
    }

    private static async Task CollectTracesAsync(
        CollectStackTraceCommand command,
        string sessionFilePath,
        Lifetime lifetime)
    {
        await UsingAsync(lifetime, async lt =>
        {
            var sessionProvider = new EventPipeSessionProvider(command.Pid);
            await sessionProvider.RunSessionAndCopyToFileAsync(
                SessionConfiguration,
                lt,
                sessionFilePath,
                TimeSpan.FromMilliseconds(10)
            );
        });
    }

    private static string ParseSessionFile(string traceLogFilePath)
    {
        using var symbolReader = new SymbolReader(TextWriter.Null)
        {
            SymbolPath = SymbolPath.MicrosoftSymbolServerPath
        };
        using var traceLog = new TraceLog(traceLogFilePath);
        var stackSource = new MutableTraceEventStackSource(traceLog)
        {
            OnlyManagedCodeStacks = true
        };

        var computer = new SampleProfilerThreadTimeComputer(traceLog, symbolReader);
        computer.GenerateThreadTimeStacks(stackSource);

        var samplesByThread = new Dictionary<int, StackSourceSample>();
        stackSource.ForEach(it =>
        {
            var stackIndex = it.StackIndex;
            var frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            while (!frameName.StartsWith("Thread"))
            {
                stackIndex = stackSource.GetCallerIndex(stackIndex);
                frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            }

            var threadId = int.Parse(frameName[8..^1]);
            samplesByThread.TryAdd(threadId, it);
        });

        var stackTraces = SerializeStackTraces(samplesByThread, stackSource);

        return stackTraces;
    }

    private static string SerializeStackTraces(Dictionary<int, StackSourceSample> samplesByThread,
        MutableTraceEventStackSource stackSource)
    {
        var sb = new StringBuilder();

        foreach (var threadSamples in samplesByThread)
        {
            sb.AppendLine($"Thread (0x{threadSamples.Key:X}):");

            var stackIndex = threadSamples.Value.StackIndex;
            var frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            while (!frameName.StartsWith("Thread"))
            {
                sb.AppendLine(frameName != "UNMANAGED_CODE_TIME" ? $"    {frameName}" : "    [Native Frames]");
                stackIndex = stackSource.GetCallerIndex(stackIndex);
                frameName = stackSource.GetFrameName(stackSource.GetFrameIndex(stackIndex), false);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}