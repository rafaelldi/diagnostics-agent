﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.NETCore.Client;
using static DiagnosticsAgent.Common.DiagnosticsClientExtensions;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace DiagnosticsAgent.EventPipes;

internal sealed class EventPipeSessionManager : IAsyncDisposable
{
    private const string DotnetDiagnosticPort = "DOTNET_DiagnosticPorts";

    private readonly DiagnosticsClient _client;
    private readonly object? _server;

    internal EventPipeSessionManager(int pid)
    {
        _client = new DiagnosticsClient(pid);
    }

    // ReSharper disable once UnusedMember.Global
    internal EventPipeSessionManager(string fileName, string arguments)
    {
        var transportName = GetTransportName();
        _server = NewReversedDiagnosticsServer(transportName);
        StartDiagnosticsServer(_server);
        
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = false
        };
        startInfo.Environment.Add(DotnetDiagnosticPort, transportName);

        using var process = Process.Start(startInfo) 
                            ?? throw new ApplicationException($"Cannot start process {fileName}");

        _client = WaitForProcessToConnect(_server, process.Id, TimeSpan.FromSeconds(15));
    }

    private static string GetTransportName()
    {
        var name = $"diagnostics-client-{Process.GetCurrentProcess().Id}-{DateTime.Now:yyyyMMdd_HHmmss}.socket";
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? name
            : Path.Combine(Path.GetTempPath(), name);
    }

    internal EventPipeSession StartSession(IReadOnlyCollection<EventPipeProvider> providers, bool requestRundown = true)
    {
        return _client.StartEventPipeSession(providers, requestRundown);
    }

    internal static void StopSession(EventPipeSession session)
    {
        try
        {
            session.Stop();
        }
        catch (EndOfStreamException)
        {
        }
        catch (TimeoutException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (ServerNotAvailableException)
        {
        }
    }

    public ValueTask DisposeAsync()
    {
        return _server is not null ? DisposeDiagnosticsServerAsync(_server) : new ValueTask();
    }
}