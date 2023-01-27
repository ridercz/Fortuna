using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Fortuna;

[Command("multi", "Prepare multi-field tickets.")]
internal class PrepareMultiCommand {

    [Argument(0, "input-file", "Name of CSV file containing list of prizes.")]
    public required string InputFileName { get; set; }

    [Argument(1, "output-file", "Name of JSON file to generate.")]
    public required string OutputFileName { get; set; }

    [Argument(2, "ticket-count", "Number of tickets to generate.")]
    public int TicketCount { get; set; }

    [Option("--fields <number>", Description = "Number of fields to generate.")]
    [Range(3, 50)]
    public int Fields { get; set; } = 3;

    [Option("--fields-to-win <number>", Description = "Number of fields with same content required to win.")]
    [Range(2, 50)]
    public int FieldsToWin { get; set; } = 3;

    [Option("--csv-separator <string>", Description = "Character sequence used for CSV field separator, ie `,` or `TAB`.")]
    public string CsvSeparator { get; set; } = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

    [Option("--csv-comment <string>", Description = "Character sequence used as CSV comment line indicator.")]
    public string CsvComment { get; set; } = "#";

    [Option("--serial-length <length>", Description = "Length of randomly generated serial numbers.")]
    [Range(5, 20)]
    public int SerialNumberLength { get; set; } = 10;

    [Option("--serial-characters <string>", Description = "Characters permitted in randomly generated serial number.")]
    public string SerialNumberCharacters { get; set; } = "0123456789ABCDEFGHKLMNPSTUWXYZ";

    public int OnExecute(CommandLineApplication app) {
        return 0;
    }

}
