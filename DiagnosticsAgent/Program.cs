using System.CommandLine;
using DiagnosticsAgent;
using JetBrains.Lifetimes;

var rootCommand = new RootCommand();

var runCommand = RunCommand();
rootCommand.Add(runCommand);

await rootCommand.InvokeAsync(args);


static Command RunCommand()
{
    var portOption = new Option<int>("--port", "Port on which the RD server will run")
    {
        IsRequired = true,
    };
    portOption.AddAlias("-p");

    var command = new Command("run", "Run RD server with diagnostics tools")
    {
        portOption
    };

    command.SetHandler(async context =>
    {
        var port = context.ParseResult.GetValueForOption(portOption);
        var token = context.GetCancellationToken();
        var lifetimeDefinition = Lifetime.Define();
        token.Register(() => lifetimeDefinition.Terminate());

        await DiagnosticsHost.Run(port, lifetimeDefinition.Lifetime);
    });

    return command;
}