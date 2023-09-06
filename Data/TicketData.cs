using System.Security.Cryptography;

namespace Fortuna.Data;

internal class TicketData {

    // Properties

    public int Version { get; set; } = 1;

    public required DateTimeOffset DateCreated { get; set; }

    public ICollection<TicketInfo> Tickets { get; set; } = new HashSet<TicketInfo>();

    // Methods

    public string GenerateUniqueSerialNumber(int length, string chars, string prefix = "") {
        var snChars = new char[length];
        while (true) {
            // Generate random serial number
            for (var i = 0; i < length; i++) {
                snChars[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            var snString = prefix + new string(snChars);

            // Check if it's unique
            if (!this.Tickets.Any(x => x.SerialNumber.Equals(snString, StringComparison.OrdinalIgnoreCase))) return snString;
        }
    }

    public bool Validate() {
        if (!this.Tickets.Any()) return false;
        var firstFieldCount = this.Tickets.First().Fields.Length;
        return this.Tickets.All(x => x.Fields.Length == firstFieldCount);
    }

}

internal record struct TicketInfo(string SerialNumber, string? Result, params string[] Fields);