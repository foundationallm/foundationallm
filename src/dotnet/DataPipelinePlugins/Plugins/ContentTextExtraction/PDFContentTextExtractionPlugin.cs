using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextExtraction
{
    /// <summary>
    /// Implements the PDF Content Text Extraction Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class PDFContentTextExtractionPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IContentTextExtractionPlugin
    {
        protected override string Name => PluginNames.PDF_CONTENTTEXTEXTRACTION;

        /// <inheritdoc/>
        public async Task<PluginResult<string>> ExtractText(BinaryData rawContent)
        {
            try
            {
                StringBuilder sb = new();
                using var pdfDocument = PdfDocument.Open(rawContent.ToStream());
                foreach (var page in pdfDocument.GetPages())
                {
                    var text = ContentOrderTextExtractor.GetText(page);
                    sb.Append(text);
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
