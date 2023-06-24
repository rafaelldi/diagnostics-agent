# Diagnostics Agent

Diagnostics Agent is a .NET global tool designed to collect various diagnostics information such as traces, metrics,
and dumps about a process running on a host, and send it to a server for analysis.
It utilizes the [Reactive Distributed communication framework](https://github.com/JetBrains/rd), to establish a
connection with the server.

## Installation

To install the Diagnostics Agent as a global tool, you can use the .NET CLI:

```bash
dotnet tool install --global diagnostics-agent
```

Make sure you have the .NET SDK installed on your machine.

## Usage

The Diagnostics Agent is primarily intended to be used in conjunction with the Rider IDE plugin
called [Diagnostics Client](https://plugins.jetbrains.com/plugin/19141-diagnostics-client). 

For more information about available commands and options, you can use the `--help` flag:

```bash
diagnostics-agent --help
```

## Supported Diagnostics

The Diagnostics Agent supports various types of diagnostics information that can be collected and sent to the server:

* Traces: Detailed logs of events and actions occurring during the execution of a process.
* Metrics: Statistical data about the performance and behavior of a process.
* Dumps: Snapshots of a process memory and execution state, useful for debugging purposes.