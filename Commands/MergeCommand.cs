using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Fortuna.Commands;

[Command("merge", "Merge multiple images into single file")]
internal class MergeCommand {

    [Argument(0, "source-folder", "Folder containing images to merge, created by `generate` command.")]
    [Required, DirectoryExists]
    public required string SourceFolder { get; set; }

    [Argument(1, "target-folder", "Folder where the merged files will be stored.")]
    [Required]
    public required string TargetFolder { get; set; }

    [Argument(2, "columns", "How many images is to be merged horizontally.")]
    [Required, Range(1, 100)]
    public int Columns { get; set; }

    [Argument(3, "rows", "How many images is to be merged horizontally.")]
    [Required, Range(1, 100)]
    public int Rows { get; set; }

    [Option("--dpi <number>", Description = "Set the DPI resolution of the generated images.")]
    public int Dpi { get; set; } = 300;

    public async Task<int> OnExecuteAsync() {
        var imagesPerPage = this.Columns * this.Rows;
        if (imagesPerPage == 1) {
            Console.WriteLine("Error: There must be more than one image on page!");
            return 1;
        }

        Console.WriteLine($"Analyzing images in {this.SourceFolder}...");
        var sfi = new DirectoryInfo(this.SourceFolder).GetFiles();
        var sourceImageCount = sfi.Length;
        var pageCount = (int)Math.Ceiling((float)sourceImageCount / imagesPerPage);
        Console.WriteLine($"  There is {sourceImageCount} images, resulting in {pageCount} pages");
        var testImage = await Image.IdentifyAsync(sfi[0].FullName);
        var pageImageWidth = testImage.Width * this.Columns;
        var pageImageHeight = testImage.Height * this.Rows;
        Console.WriteLine($"  Page will be {pageImageWidth} x {pageImageHeight} px");

        Console.Write("Creating target folder...");
        Directory.CreateDirectory(this.TargetFolder);
        Console.WriteLine("OK");

        Console.WriteLine("Processing images...");
        var i = 0;
        for (var p = 0; p < pageCount; p++) {
            using var pageImage = new Image<Rgb24>(pageImageWidth, pageImageHeight, Color.White);
            pageImage.Metadata.ResolutionUnits = SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerInch;
            pageImage.Metadata.VerticalResolution = this.Dpi;
            pageImage.Metadata.HorizontalResolution = this.Dpi;
            for (var r = 0; r < this.Rows; r++)                 for (var c = 0; c < this.Columns; c++) {
                    Console.Write($"  #{i,4} [{p,3},{c,3},{r,3}] ");
                    if (i >= sfi.Length) {
                        Console.WriteLine("skipped");
                        i++;
                        continue;
                    }
                    Console.WriteLine(sfi[i].Name);

                    using var img = await Image.LoadAsync(sfi[i].FullName);
                    pageImage.Mutate(x => x.DrawImage(img, new Point(c * testImage.Width, r * testImage.Height), 1));
                    i++;
                }
            var pageFileName = Path.Combine(this.TargetFolder, $"page-{p:0000}{sfi[0].Extension}");
            Console.Write($"Saving {pageFileName}...");
            pageImage.Save(pageFileName);
            Console.WriteLine("OK");
        }

        return 0;
    }

}
