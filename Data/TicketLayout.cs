namespace Fortuna.Data;

internal class TicketLayout {

    public record struct Rectangle(int X, int Y, int Width, int Height);

    public record struct TextStyle(string Color, int Size, string Font, bool Bold, bool Italic);

    public required string BaseImage { get; set; }

    public required Rectangle SerialNumberPosition { get; set; }

    public required Rectangle BarcodePosition { get; set; }

    public required TextStyle SerialNumberStyle { get; set; }

    public required Rectangle[] Fields { get; set; }

    public required TextStyle FieldStyle { get; set; }

}
