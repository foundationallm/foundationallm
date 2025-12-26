using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Common.Services.Tokenizers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextPartitioning
{
    /// <summary>
    /// Implements the Token Content Text Partitioning plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TokenContentTextPartitioningPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IContentTextPartitioningPlugin
    {
        protected override string Name => PluginNames.TOKEN_CONTENTTEXTPARTITIONING;

        private new readonly ILogger<TokenContentTextPartitioningPlugin> _logger =
            serviceProvider.GetRequiredService<ILogger<TokenContentTextPartitioningPlugin>>();

        /// <inheritdoc/>
        public async Task<PluginResult<List<DataPipelineContentItemContentPart>>> PartitionText(
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

            if (string.IsNullOrWhiteSpace(text))
                return new PluginResult<List<DataPipelineContentItemContentPart>>(
                [], true, false);

            var tokenizerService =
                _serviceProvider.GetRequiredKeyedService<ITokenizerService>("MicrosoftML");

            var contentItemParts = SplitPlainText(
                contentItemCanonicalId,
                text,
                tokenizerService,
                (int)partitionSizeTokens,
                (int)partitionOverlapTokens);

            return new PluginResult<List<DataPipelineContentItemContentPart>>(
                contentItemParts, true, false);
        }

        private List<DataPipelineContentItemContentPart> SplitPlainText(
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
                    return [ DataPipelineContentItemContentPart.Create
                        (contentItemCanonicalId, 1, text, tokens.Count) ];

                var chunks = Enumerable.Range(0, chunksCount - 1)
                    .Select(i => new
                    {
                        Position = i + 1,
                        Tokens = tokens.Skip(i * (partitionSizeTokens - partitionOverlapTokens)).Take(partitionSizeTokens).ToArray()
                    })
                    .Select(x => DataPipelineContentItemContentPart.Create(
                        contentItemCanonicalId,
                        x.Position,
                        tokenizerService.Decode(x.Tokens, encoderName),
                        x.Tokens.Length))
                    .ToList();

                var lastChunkStart = (chunksCount - 1) * (partitionSizeTokens - partitionOverlapTokens);
                var lastChunkSize = tokens.Count - lastChunkStart + 1;

                if (lastChunkSize < 2 * partitionOverlapTokens)
                {
                    // The last chunk is to small, will just incorporate it into the second to last.
                    var secondToLastChunkStart = (chunksCount - 2) * (partitionSizeTokens - partitionOverlapTokens);
                    var newLastChunkSize = tokens.Count - secondToLastChunkStart + 1;
                    var newLastChunk = tokenizerService.Decode(
                        [.. tokens
                            .Skip(secondToLastChunkStart)
                            .Take(newLastChunkSize)],
                        encoderName);
                    chunks.RemoveAt(chunks.Count - 1);
                    chunks.Add(DataPipelineContentItemContentPart.Create(
                        contentItemCanonicalId,
                        chunks.Count + 1,
                        newLastChunk,
                        newLastChunkSize));
                }
                else
                {
                    var lastChunk = tokenizerService.Decode(
                        [.. tokens
                            .Skip(lastChunkStart)
                            .Take(lastChunkSize)],
                        encoderName);
                    chunks.Add(DataPipelineContentItemContentPart.Create(
                        contentItemCanonicalId,
                        chunks.Count + 1,
                        lastChunk,
                        lastChunkSize));
                }

                return chunks;
            }
            else
                throw new PluginException("The tokenizer service failed to split the text into tokens.");
        }
    }
}
