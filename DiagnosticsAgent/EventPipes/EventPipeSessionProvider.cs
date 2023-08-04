using JetBrains.Lifetimes;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using static DiagnosticsAgent.Common.DiagnosticsClientExtensions;
using static JetBrains.Lifetimes.Lifetime;

namespace DiagnosticsAgent.EventPipes;

internal sealed class EventPipeSessionProvider
{
    private readonly DiagnosticsClient _client;

    internal EventPipeSessionProvider(int pid)
    {
        _client = new DiagnosticsClient(pid);
    }

    internal async Task RunSessionAndSubscribeAsync(
        EventPipeSessionConfiguration configuration,
        Lifetime sessionLifetime,
        Action<EventPipeEventSource, Lifetime> subscribeAction)
    {
        Task sessionTask;

        using (var cookie = sessionLifetime.UsingExecuteIfAlive())
        {
            if (!cookie.Succeed) return;

            var session = await _client.StartEventPipeSessionAsync(
                configuration.EventPipeProviders,
                configuration.RequestRundown,
                configuration.CircularBufferMb,
                sessionLifetime
            );
            sessionLifetime.AddDispose(session);

            var source = new EventPipeEventSource(session.EventStream);
            sessionLifetime.AddDispose(source);

            subscribeAction(source, sessionLifetime);

            var taskLifetime = sessionLifetime.CreateNested().Lifetime;

            var processingTask = taskLifetime.StartAttached(
                TaskScheduler.Default,
                () => source.Process()
            );

            var stoppingTask = taskLifetime.StartAttachedAsync(
                TaskScheduler.Default,
                async () => await StopSessionAsync(session)
            );

            sessionTask = Task.WhenAll(processingTask, stoppingTask);
        }

        await sessionTask;
    }

    internal async Task RunSessionAndCopyToFileAsync(
        EventPipeSessionConfiguration configuration,
        Lifetime sessionLifetime,
        string filePath,
        TimeSpan? duration = null)
    {
        Task sessionTask;

        using (var cookie = sessionLifetime.UsingExecuteIfAlive())
        {
            if (!cookie.Succeed) return;

            var session = await _client.StartEventPipeSessionAsync(
                configuration.EventPipeProviders,
                configuration.RequestRundown,
                configuration.CircularBufferMb,
                sessionLifetime
            );
            sessionLifetime.AddDispose(session);

            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            sessionLifetime.AddDispose(fileStream);

            var taskLifetime = sessionLifetime.CreateNested().Lifetime;

            var copyingTask = taskLifetime.StartAttachedAsync(
                TaskScheduler.Default,
                async () => await session.EventStream.CopyToAsync(fileStream, 81920)
            );

            var stoppingLifetime = duration.HasValue
                ? taskLifetime.CreateTerminatedAfter(duration.Value)
                : taskLifetime;

            var stoppingTask = stoppingLifetime.StartAttachedAsync(
                TaskScheduler.Default,
                async () => await StopSessionAsync(session)
            );

            sessionTask = Task.WhenAll(copyingTask, stoppingTask);
        }

        await sessionTask;
    }

    private static async Task StopSessionAsync(EventPipeSession session)
    {
        try
        {
            await Task.Delay(-1, AsyncLocal.Value.ToCancellationToken());
        }
        catch (TaskCanceledException)
        {
            //do nothing
        }

        await UsingAsync(async lifetime => await StopSessionAsync(session, lifetime));
    }

    private static async Task StopSessionAsync(EventPipeSession session, Lifetime lifetime)
    {
        try
        {
            await session.StopAsync(lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(30)));
        }
        catch (EndOfStreamException)
        {
        }
        catch (TimeoutException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (ServerNotAvailableException)
        {
        }
    }
}