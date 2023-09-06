using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Fortuna.Data;

namespace Fortuna.Commands;

[Command("multi", "Prepare multi-field tickets.")]
internal class PrepareMultiCommand {
    private PrizeCollection? prizes;

    [Argument(0, "input-file", "Name of CSV file containing list of prizes.")]
    [Required]
    public required string InputFileName { get; set; }

    [Argument(1, "output-file", "Name of JSON file to generate.")]
    [Required]
    public required string OutputFileName { get; set; }

    [Argument(2, "ticket-count", "Number of tickets to generate.")]
    [Required, Range(1, int.MaxValue)]
    public int TicketCount { get; set; }

    [Option("--fields <number>", ShortName = "fn", Description = "Number of fields to generate.")]
    [Range(3, 50)]
    public int Fields { get; set; } = 3;

    [Option("--fields-to-win <number>", ShortName = "fw", Description = "Number of fields with same content required to win.")]
    [Range(2, 50)]
    public int FieldsToWin { get; set; } = 3;

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

    public async Task<int> OnExecuteAsync(CommandLineApplication app) {
        if (this.FieldsToWin > this.Fields) {
            Console.WriteLine($"Warning: {this.FieldsToWin} equal fields is required to win, but only {this.Fields} is defined");
            Console.WriteLine($"         Setting value to {this.Fields}");
            this.FieldsToWin = this.Fields;
        }

        // Read prizes
        Console.WriteLine($"Reading prizes from {this.InputFileName}...");
        using var prizeFileReader = File.OpenText(this.InputFileName);
        this.prizes = await PrizeCollection.Read(prizeFileReader, this.CsvSeparator, this.CsvComment);
        Console.WriteLine($"Read {this.prizes.Count} prizes in {this.prizes.PrizeCount} instances.");
        if (this.prizes.Count == 0) {
            Console.WriteLine("Error: No prizes defined.");
            return 1;
        } else if (this.prizes.PrizeCount > this.TicketCount) {
            Console.WriteLine($"Warning: There is more prizes ({this.prizes.PrizeCount}) than tickets ({this.TicketCount}).");
            Console.WriteLine($"         Setting value to {this.prizes.PrizeCount}");
            this.TicketCount = this.prizes.PrizeCount;
        }

        // Generate winning ticket data
        Console.Write("Generating winning tickets");
        var ticketData = new TicketData() {
            DateCreated = DateTime.Now
        };
        foreach (var prize in this.prizes)             for (var i = 0; i < prize.Count; i++) {
                var serialNumber = ticketData.GenerateUniqueSerialNumber(this.SerialNumberLength, this.SerialNumberCharacters, this.SerialNumberPrefix);
                var fields = new Collection<FieldInfo>();

                for (var j = 0; j < this.FieldsToWin; j++)                     fields.Add(new(RandomNumberGenerator.GetInt32(int.MaxValue), prize.Name));

                var fieldStrings = this.CompleteFields(fields, prize.Name).ToArray();
                ticketData.Tickets.Add(new(serialNumber, prize.Name, fieldStrings));

                Console.Write('.');
            }
        Console.WriteLine("OK");

        // Generate non-winning ticket data
        Console.Write("Generate non-winning tickets");
        while (ticketData.Tickets.Count < this.TicketCount) {
            var serialNumber = ticketData.GenerateUniqueSerialNumber(this.SerialNumberLength, this.SerialNumberCharacters, this.SerialNumberPrefix);
            var fields = this.CompleteFields(new Collection<FieldInfo>()).ToArray();
            ticketData.Tickets.Add(new(serialNumber, null, fields));
            Console.Write('.');
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

    private IEnumerable<string> CompleteFields(ICollection<FieldInfo> fields, string? designatedPrice = null) {
        if (prizes == null) throw new InvalidOperationException();

        while (fields.Count < this.Fields) {
            var newField = new FieldInfo(RandomNumberGenerator.GetInt32(int.MaxValue), this.prizes[RandomNumberGenerator.GetInt32(this.prizes.Count)].Name);
            if (newField.Value.Equals(designatedPrice)) continue;
            fields.Add(newField);
            var winningPrizes = fields.GroupBy(x => x.Value).Where(x => x.Count() >= this.FieldsToWin);
            if (designatedPrice == null && winningPrizes.Any() || designatedPrice != null && winningPrizes.Count() > 1) fields.Remove(newField);
        }

        return fields.OrderBy(x => x.Order).Select(x => x.Value);
    }

    private record struct FieldInfo(int Order, string Value);

}
