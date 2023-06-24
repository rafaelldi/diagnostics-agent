using System.Diagnostics.Tracing;
using DiagnosticsAgent.Model;
using DiagnosticsAgent.Traces;
using FluentAssertions;
using static Microsoft.Diagnostics.Tracing.Parsers.ClrTraceEventParser;
using static DiagnosticsAgent.Common.Providers;

namespace DiagnosticsAgentTests.Traces;

public class TraceProfileParserTests
{
    [Fact]
    public void Parse_none_profile()
    {
        var providers = TraceProfileConverter.Convert(TracingProfile.None);

        providers.Should().BeEmpty();
    }

    [Fact]
    public void Parse_cpu_sampling_profile()
    {
        var providers = TraceProfileConverter.Convert(TracingProfile.CpuSampling);

        providers.Length.Should().Be(2);
        var sampleProvider = providers.Single(it => it.Name == SampleProfilerProvider);
        sampleProvider.Level.Should().Be(EventLevel.Informational);
        sampleProvider.Flags.Should().Be(0xF00000000000);
        var dotNetRuntimeProvider = providers.Single(it => it.Name == DotNetRuntimeProvider);
        dotNetRuntimeProvider.Level.Should().Be(EventLevel.Informational);
        dotNetRuntimeProvider.Flags.Should().Be((long)Keywords.Default);
    }

    [Fact]
    public void Parse_gc_verbose_profile()
    {
        var providers = TraceProfileConverter.Convert(TracingProfile.GcVerbose);

        providers.Length.Should().Be(1);
        var provider = providers.Single();
        provider.Name.Should().Be(DotNetRuntimeProvider);
        provider.Level.Should().Be(EventLevel.Verbose);
        provider.Flags.Should().Be((long)Keywords.GC | (long)Keywords.GCHandle | (long)Keywords.Exception);
    }

    [Fact]
    public void Parse_gc_collect_profile()
    {
        var providers = TraceProfileConverter.Convert(TracingProfile.GcCollect);

        providers.Length.Should().Be(1);
        var provider = providers.Single();
        provider.Name.Should().Be(DotNetRuntimeProvider);
        provider.Level.Should().Be(EventLevel.Informational);
        provider.Flags.Should().Be((long)Keywords.GC);
    }
}