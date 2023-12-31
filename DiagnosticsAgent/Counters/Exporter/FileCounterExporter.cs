﻿using System.Threading.Channels;
using DiagnosticsAgent.Model;
using JetBrains.Lifetimes;

namespace DiagnosticsAgent.Counters.Exporter;

internal abstract class FileCounterExporter
{
    private readonly string _filePath;
    private readonly ChannelReader<ValueCounter> _reader;

    protected FileCounterExporter(string filePath, ChannelReader<ValueCounter> reader)
    {
        _filePath = filePath;
        _reader = reader;
    }

    internal async Task ConsumeAsync()
    {
        if (Lifetime.AsyncLocal.Value.IsNotAlive)
        {
            return;
        }

        await using var streamWriter = File.CreateText(_filePath);

        var header = GetFileHeader();
        if (header != null)
        {
            await streamWriter.WriteLineAsync(header);
        }

        try
        {
            await foreach (var counter in _reader.ReadAllAsync(Lifetime.AsyncLocal.Value))
            {
                await streamWriter.WriteLineAsync(GetCounterString(in counter));
            }
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }

        var footer = GetFileFooter();
        if (footer != null)
        {
            await streamWriter.WriteLineAsync(footer);
        }
    }

    protected abstract string? GetFileHeader();

    protected abstract string GetCounterString(in ValueCounter counter);

    protected abstract string? GetFileFooter();
    
    internal static FileCounterExporter Create(
        string filePath,
        CounterFileFormat format,
        ChannelReader<ValueCounter> reader) =>
        format switch
        {
            CounterFileFormat.Csv => new CounterCsvExporter(filePath, reader),
            CounterFileFormat.Json => new CounterJsonExporter(filePath, reader),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
}