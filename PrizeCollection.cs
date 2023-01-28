using System.Collections.ObjectModel;

namespace Fortuna;

internal class PrizeCollection : Collection<PrizeInfo> {

    public static async Task<PrizeCollection> Read(TextReader reader, string csvSeparator, string commentIndicator) {
        var result = new PrizeCollection();
        var lineNumber = 0;
        while (true) {
            var line = await reader.ReadLineAsync();
            lineNumber++;
            if (line is null) break;
            if (string.IsNullOrEmpty(line) || line.StartsWith(commentIndicator)) {
                Console.WriteLine($"  Ignoring line {lineNumber}: Comment or empty");
                continue;
            }
            var data = line.Split(csvSeparator);
            if (data.Length != 2 || string.IsNullOrWhiteSpace(data[0]) || !int.TryParse(data[1], out var count) || count < 1) {
                Console.WriteLine($"  Ignoring line {lineNumber}: Syntax error");
                continue;
            }

            var name = data[0].Replace("\\n", "\n");
            result.Add(new(name, count));
        }
        return result;
    }

    public int PrizeCount => this.Sum(x => x.Count);

}

internal record struct PrizeInfo(string Name, int Count);
