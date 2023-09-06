using Fortuna.Commands;

[Command("prepare", Description = "Prepare JSON file with ticket definitions.")]
[Subcommand(typeof(PrepareSingleCommand), typeof(PrepareMultiCommand))]
public class PrepareCommand {

    public int OnExecute(CommandLineApplication app) {
        app.ShowHelp();
        return 0;
    }

}
