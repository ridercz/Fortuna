using System.Security.Cryptography;

namespace Fortuna;

internal class TicketData {

    public int Version { get; set; } = 1;

    public required string Strategy { get; set; }

    public required Guid BatchId { get; set; }

    public required DateTimeOffset DateCreated { get; set; }

    public ICollection<TicketInfo> Tickets { get; set; }= new HashSet<TicketInfo>();

    public string GenerateUniqueSerialNumber(int length, string chars) {
        var snChars = new char[length];
        while (true) {
            // Generate random serial number
            for (var i = 0; i < length; i++) {
                snChars[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            var snString = new string(snChars);

            // Check if it's unique
            if (!this.Tickets.Any(x => x.SerialNumber.Equals(snString, StringComparison.OrdinalIgnoreCase))) return snString;
        }
    }

}

internal record struct TicketInfo(string SerialNumber, string? Result, params string[] Fields);