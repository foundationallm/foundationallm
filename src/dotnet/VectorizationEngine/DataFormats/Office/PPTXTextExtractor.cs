using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Text;
using OpenXml = DocumentFormat.OpenXml.Drawing;

namespace FoundationaLLM.Vectorization.DataFormats.Office
{
    /// <summary>
    /// Extracts text from PPTX files.
    /// </summary>
    public class PPTXTextExtractor
    {
        /// <summary>
        /// Extracts the text content from a PPTX document.
        /// </summary>
        /// <param name="binaryContent">The binary content of the PPTX document.</param>
        /// <returns>The text content of the PPTX document.</returns>
        public static string GetText(BinaryData binaryContent)
        {
            StringBuilder sb = new();

            using var stream = binaryContent.ToStream();
            using var presentationDocument = PresentationDocument.Open(stream, false);

            if (presentationDocument.PresentationPart is PresentationPart presentationPart
                && presentationPart.Presentation is Presentation presentation
                && presentation.SlideIdList is SlideIdList slideIdList
                && slideIdList.Elements<SlideId>().ToList() is List<SlideId> slideIds and { Count: > 0 })
            {
                var slideNumber = 0;
                foreach (SlideId slideId in slideIds)
                {
                    slideNumber++;

                    if ((string?)slideId.RelationshipId is string relationshipId
                        && presentationPart.GetPartById(relationshipId) is SlidePart slidePart
                        && slidePart != null
                        && slidePart.Slide?.Descendants<OpenXml.Text>().ToList() is List<OpenXml.Text> texts and { Count: > 0 })
                    {
                        // Skip the slide if it is hidden
                        bool isVisible = slidePart.Slide.Show ?? true;
                        if (!isVisible) { continue; }

                        var slideContent = new StringBuilder();
                        for (var i = 0; i < texts.Count; i++)
                        {
                            var text = texts[i];
                            slideContent.Append(text.Text);
                            if (i < texts.Count - 1)
                            {
                                slideContent.Append(' ');
                            }
                        }

                        // Skip the slide if there is no text
                        if (slideContent.Length < 1) { continue; }

                        sb.Append(slideContent);
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}
