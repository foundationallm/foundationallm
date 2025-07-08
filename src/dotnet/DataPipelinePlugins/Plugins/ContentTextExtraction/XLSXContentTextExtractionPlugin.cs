using ClosedXML.Excel;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using System.Text;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextExtraction
{
    /// <summary>
    /// Implements the XLSX Content Text Extraction Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class XLSXContentTextExtractionPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IContentTextExtractionPlugin
    {
        protected override string Name => PluginNames.XLSX_CONTENTTEXTEXTRACTION;

        private readonly bool _withWorksheetNumber = true;
        private readonly bool _withEndOfWorksheetMarker = false;
        private readonly bool _withQuotes = true;
        private readonly string _worksheetNumberTemplate = "\n# Worksheet {number}\n";
        private readonly string _endOfWorksheetMarkerTemplate = "\n# End of worksheet {number}";
        private readonly string _rowPrefix = string.Empty;
        private readonly string _columnSeparator = ", ";
        private readonly string _rowSuffix = string.Empty;

        /// <inheritdoc/>
        public async Task<PluginResult<string>> ExtractText(BinaryData rawContent)
        {
            try
            {
                var sb = new StringBuilder();

                using var stream = rawContent.ToStream();
                using var workbook = new XLWorkbook(stream);

                var worksheetNumber = 0;
                foreach (var worksheet in workbook.Worksheets)
                {
                    worksheetNumber++;
                    if (this._withWorksheetNumber)
                    {
                        sb.AppendLine(this._worksheetNumberTemplate.Replace("{number}", $"{worksheetNumber}", StringComparison.OrdinalIgnoreCase));
                    }

                    if (worksheet.RangeUsed() is not null)
                    {
                        foreach (IXLRangeRow? row in worksheet.RangeUsed()!.RowsUsed())
                        {
                            if (row == null) { continue; }

                            var cells = row.CellsUsed().ToList();

                            sb.Append(this._rowPrefix);
                            for (var i = 0; i < cells.Count; i++)
                            {
                                IXLCell? cell = cells[i];

                                if (this._withQuotes && cell is { CachedValue.IsText: true })
                                {
                                    sb.Append('"')
                                        .Append(cell.CachedValue.GetText().Replace("\"", "\"\"", StringComparison.Ordinal))
                                        .Append('"');
                                }
                                else
                                {
                                    sb.Append(cell.CachedValue);
                                }

                                if (i < cells.Count - 1)
                                {
                                    sb.Append(this._columnSeparator);
                                }
                            }

                            sb.AppendLine(this._rowSuffix);
                        }

                    }
                    else
                    {
                        sb.AppendLine($"No data found in Worksheet number: {worksheetNumber}");
                    }

                    if (this._withEndOfWorksheetMarker)
                    {
                        sb.AppendLine(this._endOfWorksheetMarkerTemplate.Replace("{number}", $"{worksheetNumber}", StringComparison.OrdinalIgnoreCase));
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
