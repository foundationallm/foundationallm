using System.CommandLine;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

var inputFileOption = new Option<string>("--input-file")
{
    Description = "Path to the input content file.",
    Required = true
};

var outputFileOption = new Option<string>("--output-file")
{
    Description = "Path to the output text file.",
    Required = true
};

var imageExportOption = new Option<bool>("--images")
{
    Description = "Export images from content.",
    Arity = ArgumentArity.Zero
};

var rootCommand = new RootCommand("FoundationaLLM Text Extraction Utility");

var extractCommand = new Command("extract", "Extract text from a content file.")
{
    inputFileOption,
    outputFileOption,
    imageExportOption
};
extractCommand.SetAction(async parseResult =>
{
    var inputFile = parseResult.GetValue(inputFileOption);
    var outputFile = parseResult.GetValue(outputFileOption);
    var exportImages = parseResult.GetValue(imageExportOption);
    await ExtractText(
        inputFile!,
        outputFile!,
        exportImages);
});

rootCommand.Subcommands.Add(extractCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

async Task ExtractText(
    string inputFilePath,
    string outputFilePath,
    bool exportImages)
{
    var startTime = DateTimeOffset.UtcNow;

    var binaryContent = BinaryData.FromBytes(
        await File.ReadAllBytesAsync(inputFilePath));

    StringBuilder sb = new();
    using var pdfDocument = PdfDocument.Open(binaryContent.ToStream());
    var imageCount = 0;
    foreach (var page in pdfDocument.GetPages())
    {
        var text = ContentOrderTextExtractor.GetText(page);
        sb.Append(text);

        if (exportImages)
        {
            foreach (var pdfImage in page.GetImages())
            {
                if (pdfImage.TryGetPng(out var bytes))
                    await File.WriteAllBytesAsync(
                        Path.Combine(
                            Path.GetDirectoryName(outputFilePath)!,
                            $"{Path.GetFileNameWithoutExtension(outputFilePath)}_image{++imageCount:D4}.png"),
                        bytes);
            }
        }
    }

    await File.WriteAllTextAsync(outputFilePath, sb.ToString());

    Console.WriteLine($"Text extraction completed in {(DateTimeOffset.UtcNow - startTime).TotalSeconds} seconds.");
}