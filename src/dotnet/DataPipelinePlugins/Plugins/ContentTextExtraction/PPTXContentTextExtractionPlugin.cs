using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using System.Text;
using OpenXml = DocumentFormat.OpenXml.Drawing;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextExtraction
{
    /// <summary>
    /// Implements the PPTX Content Text Extraction Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class PPTXContentTextExtractionPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IContentTextExtractionPlugin
    {
        protected override string Name => PluginNames.PPTX_CONTENTTEXTEXTRACTION;

        /// <inheritdoc/>
        public async Task<PluginResult<string>> ExtractText(BinaryData rawContent)
        {
            try
            {
                StringBuilder sb = new();

                using var stream = rawContent.ToStream();
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

                return
                    await Task.FromResult(new PluginResult<string>(
                        sb.ToString().Trim(), true, false));
            }
            catch (Exception ex)
            {
                return
                    await Task.FromResult(new PluginResult<string>(
                        null, false, false, ex.Message));
            }
        }
    }
}
