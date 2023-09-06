using Fortuna.Commands;

[Command]
[Subcommand(typeof(PrepareCommand), typeof(GenerateCommand), typeof(MergeCommand))]
public class RootCommand {

    public int OnExecute(CommandLineApplication app) {
        app.ShowHelp();
        return 0;
    }

}
