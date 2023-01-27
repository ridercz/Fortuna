using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Fortuna;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

[Command("generate", Description = "Generate image files with tickets.")]
public class GenerateCommand {

    [Argument(0, "ticket-data-file", "JSON file with ticket data, prepared with the `prepare` command.")]
    [FileExists]
    public required string TicketDataFile { get; set; }

    [Argument(1, "ticket-layout-file", "JSON file containing layout.")]
    [FileExists]
    public required string TicketLayoutFile { get; set; }

    [Argument(2, "output-folder", "Folder where generated output images are to be stored.")]
    public required string OutputFolder { get; set; }

    public async Task<int> OnExecuteAsync(CommandLineApplication app) {
        Console.Write("Reading ticket data...");
        TicketData data;
        try {
            var ticketDataJson = File.OpenRead(this.TicketDataFile);
            data = await JsonSerializer.DeserializeAsync<TicketData>(ticketDataJson) ?? throw new Exception("Empty data in file.");
            if (!data.Validate()) throw new Exception("Invalid number of columns in data file.");
            Console.WriteLine("OK");
        } catch (Exception ex) {
            Console.WriteLine("Failed!");
            Console.WriteLine(ex.Message);
            return 1;
        }

        Console.Write("Reading layout data...");
        TicketLayout layout;
        try {
            var ticketLayoutJson = File.OpenRead(this.TicketLayoutFile);
            layout = await JsonSerializer.DeserializeAsync<TicketLayout>(ticketLayoutJson) ?? throw new Exception("Empty data in file.");
            if (layout.Fields.Length != data.Tickets.First().Fields.Length) throw new Exception("Number of fields in data file does not equal number of fields in layout file.");
            Console.WriteLine("OK");
        } catch (Exception ex) {
            Console.WriteLine("Failed!");
            Console.WriteLine(ex.Message);
            return 1;
        }

        // Create target path
        Console.Write("Creating output path...");
        Directory.CreateDirectory(this.OutputFolder);
        Console.WriteLine("OK");

        // Prepare items for image generation
        var snColor = GetColorFromHtmlString(layout.SerialNumberStyle.Color);
        var snFontStyle = FontStyle.Regular
            | (layout.SerialNumberStyle.Bold ? FontStyle.Bold : FontStyle.Regular)
            | (layout.SerialNumberStyle.Italic ? FontStyle.Italic : FontStyle.Regular);
        var snFont = SystemFonts.Get(layout.SerialNumberStyle.Font).CreateFont(layout.SerialNumberStyle.Size, snFontStyle);
        var fColor = GetColorFromHtmlString(layout.FieldStyle.Color);
        var fFontStyle = FontStyle.Regular
            | (layout.FieldStyle.Bold ? FontStyle.Bold : FontStyle.Regular)
            | (layout.FieldStyle.Italic ? FontStyle.Italic : FontStyle.Regular);
        var fFontFamily = SystemFonts.Get(layout.FieldStyle.Font);

        foreach (var ticket in data.Tickets) {
            var targetImagePath = Path.Combine(this.OutputFolder, ticket.SerialNumber + Path.GetExtension(layout.BaseImage));
            Console.Write($"Generating {targetImagePath}...");

            // Load base image
            using var image = await Image.LoadAsync(layout.BaseImage);

            // Add serial number
            image.Mutate(x => x.DrawText(ticket.SerialNumber, snFont, snColor, new PointF(layout.SerialNumberPosition.X, layout.SerialNumberPosition.Y)));

            // Add fields
            for (var i = 0; i < ticket.Fields.Length; i++) {
                var rect = new Rectangle(layout.Fields[i].X, layout.Fields[i].Y, layout.Fields[i].Width, layout.Fields[i].Height);
                var fontSize = layout.FieldStyle.Size;

                while (true) {
                    // Create font of appropriate size
                    var font = fFontFamily.CreateFont(fontSize, FontStyle.Bold);
                    var options = new TextOptions(font) {
                        Origin = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2),
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        WrappingLength = rect.Width
                    };

                    // Check if font fits into the box
                    var fr = TextMeasurer.Measure(ticket.Fields[i], options);
                    if (fr.Width > rect.Width || fr.Height > rect.Height) {
                        // It doesn't, try smaller size
                        fontSize -= 2;
                        continue;
                    }

                    // It fits, write it
                    image.Mutate(x => x.DrawText(options, ticket.Fields[i], fColor));
                    break;
                }
            }

            // Save file
            image.Save(targetImagePath);

            Console.WriteLine("OK");
        }

        return 0;
    }


    private static Color GetColorFromHtmlString(string s) {
        s = s.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(s));
        if (s.StartsWith('#')) s = s[1..];
        if (!Regex.IsMatch(s, "^[0-9a-f]{6}")) throw new ArgumentException("Value must be valid HTML color string.", nameof(s));

        var r = Convert.ToByte(s[0..2], 16);
        var g = Convert.ToByte(s[2..4], 16);
        var b = Convert.ToByte(s[4..6], 16);

        return Color.FromRgb(r, g, b);
    }

}