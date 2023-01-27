using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Fortuna;

[Command("single", "Prepare single-field tickets.")]
internal class PrepareSingleCommand {

    [Argument(0, "input-file", "Name of CSV file containing list of prizes.")]
    [Required]
    public required string InputFileName { get; set; }

    [Argument(1, "output-file", "Name of JSON file to generate.")]
    [Required]
    public required string OutputFileName { get; set; }

    [Option("--csv-separator <string>", ShortName = "cs", Description = "Character sequence used for CSV field separator, ie `,` or `TAB`.")]
    public string CsvSeparator { get; set; } = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

    [Option("--csv-comment <string>", ShortName = "cc", Description = "Character sequence used as CSV comment line indicator.")]
    public string CsvComment { get; set; } = "#";

    [Option("--serial-length <length>", ShortName = "sl", Description = "Length of randomly generated serial numbers.")]
    [Range(5, 20)]
    public int SerialNumberLength { get; set; } = 10;

    [Option("--serial-characters <string>", ShortName = "sc", Description = "Characters permitted in randomly generated serial number.")]
    public string SerialNumberCharacters { get; set; } = "0123456789ABCDEFGHKLMNPSTUWXYZ";

    [Option("--serial-number-prefix <string>", ShortName = "sp", Description = "Prefix for all serial numbers (ie. batch number)")]
    public string SerialNumberPrefix { get; set; } = string.Empty;

    public async Task<int> OnExecute(CommandLineApplication app) {
        // Read prizes
        Console.WriteLine($"Reading prizes from {this.InputFileName}...");
        using var prizeFileReader = File.OpenText(this.InputFileName);
        var prizes = await PrizeCollection.Read(prizeFileReader, this.CsvSeparator, this.CsvComment);
        Console.WriteLine($"Read {prizes.Count} prizes in {prizes.PrizeCount} instances.");
        if (prizes.Count == 0) {
            Console.WriteLine("Error: No prizes defined.");
            return 1;
        }

        // Generate ticket data
        Console.Write("Generating tickets");
        var ticketData = new TicketData() {
            DateCreated = DateTime.Now
        };
        foreach (var prize in prizes) {
            for (var i = 0; i < prize.Count; i++) {
                var serialNumber = ticketData.GenerateUniqueSerialNumber(this.SerialNumberLength, this.SerialNumberCharacters, this.SerialNumberPrefix);
                ticketData.Tickets.Add(new(serialNumber, prize.Name, prize.Name));
                Console.Write(".");
            }
        }
        Console.WriteLine("OK");

        // Sort tickets
        Console.Write("Sorting tickets by serial number...");
        ticketData.Tickets = ticketData.Tickets.OrderBy(x => x.SerialNumber).ToList();
        Console.WriteLine("OK");

        // Save to JSON file
        Console.Write($"Saving to {this.OutputFileName}...");
        using var jsonFile = File.Create(this.OutputFileName);
        await JsonSerializer.SerializeAsync(jsonFile, ticketData, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) });
        Console.WriteLine("OK");

        return 0;
    }

}
