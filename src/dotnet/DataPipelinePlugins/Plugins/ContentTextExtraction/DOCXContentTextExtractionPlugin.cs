using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using System.Text;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextExtraction
{
    /// <summary>
    /// Implements the DOCX Content Text Extraction Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class DOCXContentTextExtractionPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IContentTextExtractionPlugin
    {
        protected override string Name => PluginNames.DOCX_CONTENTTEXTEXTRACTION;

        /// <inheritdoc/>
        public async Task<PluginResult<string>> ExtractText(BinaryData rawContent)
        {
            try
            {
                StringBuilder sb = new();

                using var stream = rawContent.ToStream();
                var wordprocessingDocument = WordprocessingDocument.Open(stream, false);

                var mainPart = wordprocessingDocument.MainDocumentPart ?? throw new VectorizationException("The main document part is missing.");
                var body = mainPart.Document.Body ?? throw new VectorizationException("The document body is missing.");

                var paragraphs = body.Descendants<Paragraph>();
                if (paragraphs != null)
                    foreach (Paragraph p in paragraphs)
                        sb.AppendLine(p.InnerText);

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
