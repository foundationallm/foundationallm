using FoundationaLLM.Common.Utils;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Context
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Context_ContentType(
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(1, output, fixture, new DependencyInjectionContainerInitializer()), IClassFixture<TestFixture>
    {
        private const string ROOT_PATH = "D:/Repos/FoundationaLLM-Customer/evaluation/files/content-type-testing";

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task Context_ContentType_Validate(
            string filePath,
            bool matchesExtension)
        {
            WriteLine("============ FoundationaLLM Context - Content Type Tests ============");

            var fileContent = await File.ReadAllBytesAsync(
                Path.Combine(ROOT_PATH, filePath));
            var fileContentStream = BinaryData.FromBytes(fileContent);

            var contentTypeResult = FileUtils.GetFileContentType(
               Path.GetFileName(filePath),
               fileContentStream);

            Assert.True(contentTypeResult.IsSupported);
            if (matchesExtension)
                Assert.True(contentTypeResult.MatchesExtension);
            else
                Assert.False(contentTypeResult.MatchesExtension);
        }

        public static TheoryData<string, bool> TestData =>
        new()
        {
            { "archive/TAR File.tar", true},
            { "archive/ZIP File.zip", true },
            { "binary-document/DOCX File.docx", true },
            { "binary-document/PDF File.pdf", true },
            { "binary-document/PPTX File.pptx", true },
            { "binary-document/XLSX File.xlsx", true },
            { "media/BMP File.bmp", true },
            { "media/GIF File.gif", true },
            { "media/JPEG File.jpeg", true },
            { "media/JPG File.jpg", true },
            { "media/PNG File.png", true },
            { "media/TIFF File.tiff", true },
            { "media/WAV File.wav", true },
            { "microsoft-office-legacy/DOC File.doc", true },
            { "microsoft-office-legacy/PPT File.ppt", true },
            { "microsoft-office-legacy/XLS File.xls", true },
            { "plain-text/C File.c", true },
            { "plain-text/CPP File.cpp", true },
            { "plain-text/CS File.cs", true },
            { "plain-text/CSS File.css", true },
            { "plain-text/HTML File.html", true },
            { "plain-text/INI File.ini", true },
            { "plain-text/JAVA File.java", true },
            { "plain-text/JS File.js", true },
            { "plain-text/JSON File.json", true },
            { "plain-text/JSONL File.jsonl", true },
            { "plain-text/MD File.md", true },
            { "plain-text/PHP File.php", true },
            { "plain-text/PY File.py", true },
            { "plain-text/RB File.rb", true },
            { "plain-text/RTF File.rtf", true },
            { "plain-text/SH File.sh", true },
            { "plain-text/TEX File.tex", true },
            { "plain-text/TOML File.toml", true },
            { "plain-text/TS File.ts", true },
            { "plain-text/TXT File.txt", true },
            { "plain-text/XML File.xml", true },
            { "plain-text/YAML File.yaml", true },
            { "plain-text/YML File.yml", true },
            { "plain-text-data/CSV File.csv", true },
            { "plain-text-data/TSV File.tsv", true },

            { "mismatch/CS File.zip", false },
            { "mismatch/JPEG File.pdf", false },
            { "mismatch/PDF File.png", false },
            { "mismatch/PPTX File.pdf", false },
            { "mismatch/PNG File.txt", false },
            { "mismatch/PY File.jpeg", false },
            { "mismatch/TXT File.pdf", false }
        };
    }
}