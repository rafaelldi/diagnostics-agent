// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using DiagnosticsAgent.Benchmarks.Counter;

var summary = BenchmarkRunner.Run<CounterCollectionParser>();