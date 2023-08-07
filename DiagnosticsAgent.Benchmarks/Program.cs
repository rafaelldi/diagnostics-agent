// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using DiagnosticsAgent.Benchmarks.Counter;

// ReSharper disable once UnusedVariable
var summary = BenchmarkRunner.Run<CounterCollectionParser>();