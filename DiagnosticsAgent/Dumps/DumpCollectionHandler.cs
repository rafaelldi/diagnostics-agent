using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;
using JetBrains.Rd.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using DumpType = Microsoft.Diagnostics.NETCore.Client.DumpType;

namespace DiagnosticsAgent.Dumps;

internal static class DumpCollectionHandler
{
    internal static void Subscribe(DiagnosticsHostModel model)
    {
        model.CollectDump.SetAsync(async (lt, command) => await CollectAsync(command, lt));
    }

    private static async Task<DumpCollectionResult> CollectAsync(CollectDumpCommand command, Lifetime lifetime)
    {
        var client = new DiagnosticsClient(command.Pid);
        var type = Map(command.Type);
        var path = Path.Combine(command.OutFolder, command.Filename);
        await client.WriteDumpAsync(type, path, command.Diag, lifetime);

        return new DumpCollectionResult(path);
    }

    private static DumpType Map(Model.DumpType type) =>
        type switch
        {
            Model.DumpType.Full => DumpType.Full,
            Model.DumpType.Heap => DumpType.WithHeap,
            Model.DumpType.Triage => DumpType.Triage,
            Model.DumpType.Mini => DumpType.Normal,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
}