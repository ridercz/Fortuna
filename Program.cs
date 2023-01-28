global using McMaster.Extensions.CommandLineUtils;
using Fortuna;

var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
Console.WriteLine($"Fortuna/{version} Lottery Ticket Generator");
Console.WriteLine("Copyright (c) Michal Altair Valášek, 2023");
Console.WriteLine();
await CommandLineApplication.ExecuteAsync<RootCommand>(args);

[Command]
[Subcommand(typeof(PrepareCommand), typeof(GenerateCommand), typeof(MergeCommand))]
public class RootCommand {

    public int OnExecute(CommandLineApplication app) {
        app.ShowHelp();
        return 0;
    }

}

[Command("prepare", Description = "Prepare JSON file with ticket definitions.")]
[Subcommand(typeof(PrepareSingleCommand), typeof(PrepareMultiCommand))]
public class PrepareCommand {

    public int OnExecute(CommandLineApplication app) {
        app.ShowHelp();
        return 0;
    }

}
