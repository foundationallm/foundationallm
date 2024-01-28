using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace FoundationaLLM.Vectorization.DataFormats.PDF
{
    /// <summary>
    /// Extracts text from PDF files.
    /// </summary>
    public class PDFTextExtractor
    {
        /// <summary>
        /// Extracts the text content from a PDF document.
        /// </summary>
        /// <param name="binaryContent">The binary content of the PDF document.</param>
        /// <returns>The text content of the PDF document.</returns>
        public static string GetText(BinaryData binaryContent)
        {
            StringBuilder sb = new();
            using var pdfDocument = PdfDocument.Open(binaryContent.ToStream());
            foreach (var page in pdfDocument.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page);
                sb.Append(text);
            }

            return sb.ToString().Trim();
        }
    }
}
