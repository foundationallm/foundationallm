using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.Tokenizers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Common;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextPartitioning
{
    /// <summary>
    /// Implements the Token Content Text Partitioning plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TokenContentTextPartitioningPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IContentTextPartitioningPlugin
    {
        protected override string Name => PluginNames.TOKEN_CONTENTTEXTPARTITIONING;

        private readonly ILogger<TokenContentTextPartitioningPlugin> _logger =
            serviceProvider.GetRequiredService<ILogger<TokenContentTextPartitioningPlugin>>();

        /// <inheritdoc/>
        public async Task<PluginResult<List<DataPipelineContentItemPart>>> PartitionText(
            string contentItemCanonicalId,
            string text)
        {
            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONSIZETOKENS,
                out var partitionSizeTokens))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONSIZETOKENS} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONOVERLAPTOKENS,
                out var partitionOverlapTokens))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONOVERLAPTOKENS} parameter.");

            ITokenizerService tokenizerService = null!;
            if (!_packageManager.TryGetService(
                nameof(MicrosoftBPETokenizerService), out var tokenizerServiceObject))
            {
                tokenizerService = new MicrosoftBPETokenizerService(
                    _serviceProvider.GetRequiredService<ILogger<MicrosoftBPETokenizerService>>());
                _packageManager.RegisterService(
                    nameof(MicrosoftBPETokenizerService), tokenizerService);
            }
            else
            {
                tokenizerService = (ITokenizerService)tokenizerServiceObject!;
            }

            var contentItemParts = SplitPlainText(
                contentItemCanonicalId,
                text,
                tokenizerService,
                (int)partitionSizeTokens,
                (int)partitionOverlapTokens);

            return new PluginResult<List<DataPipelineContentItemPart>>(
                contentItemParts, true, false);
        }

        private List<DataPipelineContentItemPart> SplitPlainText(
            string contentItemCanonicalId,
            string text,
            ITokenizerService tokenizerService,
            int partitionSizeTokens,
            int partitionOverlapTokens)
        {
            var encoderName = TikTokenizerEncoders.CL100K_BASE;
            var tokens = tokenizerService.Encode(text, encoderName);

            if (tokens != null)
            {
                _logger.LogInformation("The tokenizer identified {TokensCount} tokens.", tokens.Count);

                var chunksCount = (int)Math.Ceiling((1f * tokens!.Count - partitionOverlapTokens) / (partitionSizeTokens - partitionOverlapTokens));

                if (chunksCount <= 1)
                    return [ new()
                    {
                        ContentItemCanonicalId = contentItemCanonicalId,
                        Position = 1,
                        Content = text,
                        ContentSizeTokens = tokens.Count
                    } ];

                var chunks = Enumerable.Range(0, chunksCount - 1)
                    .Select(i => new
                    {
                        Position = i + 1,
                        Tokens = tokens.Skip(i * (partitionSizeTokens - partitionOverlapTokens)).Take(partitionSizeTokens).ToArray()
                    })
                    .Select(x => new DataPipelineContentItemPart
                    {
                        ContentItemCanonicalId = contentItemCanonicalId,
                        Position = x.Position,
                        Content = tokenizerService.Decode(x.Tokens, encoderName),
                        ContentSizeTokens = x.Tokens.Length
                    })
                    .ToList();

                var lastChunkStart = (chunksCount - 1) * (partitionSizeTokens - partitionOverlapTokens);
                var lastChunkSize = tokens.Count - lastChunkStart + 1;

                if (lastChunkSize < 2 * partitionOverlapTokens)
                {
                    // The last chunk is to small, will just incorporate it into the second to last.
                    var secondToLastChunkStart = (chunksCount - 2) * (partitionSizeTokens - partitionOverlapTokens);
                    var newLastChunkSize = tokens.Count - secondToLastChunkStart + 1;
                    var newLastChunk = tokenizerService.Decode(
                        tokens
                            .Skip(secondToLastChunkStart)
                            .Take(newLastChunkSize)
                            .ToArray(),
                        encoderName);
                    chunks.RemoveAt(chunks.Count - 1);
                    chunks.Add(new DataPipelineContentItemPart
                    {
                        ContentItemCanonicalId = contentItemCanonicalId,
                        Position = chunks.Count + 1,
                        Content = newLastChunk,
                        ContentSizeTokens = newLastChunkSize
                    });
                }
                else
                {
                    var lastChunk = tokenizerService.Decode(
                        tokens
                            .Skip(lastChunkStart)
                            .Take(lastChunkSize)
                            .ToArray(),
                        encoderName);
                    chunks.Add(new DataPipelineContentItemPart
                    {
                        ContentItemCanonicalId = contentItemCanonicalId,
                        Position = chunks.Count + 1,
                        Content = lastChunk,
                        ContentSizeTokens = lastChunkSize
                    });
                }

                return chunks;
            }
            else
                throw new PluginException("The tokenizer service failed to split the text into tokens.");
        }
    }
}
