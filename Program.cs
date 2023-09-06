global using McMaster.Extensions.CommandLineUtils;

var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
Console.WriteLine($"Fortuna/{version} Lottery Ticket Generator");
Console.WriteLine("Copyright (c) Michal Altair Valášek, 2023");
Console.WriteLine();
await CommandLineApplication.ExecuteAsync<RootCommand>(args);