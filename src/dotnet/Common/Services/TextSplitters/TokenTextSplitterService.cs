using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Text;
using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.TextSplitters
{

    /// <summary>
    /// Splits text based on number of tokens.
    /// </summary>
    /// <param name="tokenizerService">The <see cref="ITokenizerService"/> used to tokenize the input text.</param>
    /// <param name="options">The <see cref="IOptions{TOptions}"/> providing the settings for the service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class TokenTextSplitterService(
        ITokenizerService tokenizerService,
        IOptions<TokenTextSplitterServiceSettings> options,
        ILogger<TokenTextSplitterService> logger) : ITextSplitterService
    {
        private readonly ITokenizerService _tokenizerService = tokenizerService;
        private readonly TokenTextSplitterServiceSettings _settings = options.Value;
        private readonly ILogger<TokenTextSplitterService> _logger = logger;

        /// <inheritdoc/>
        public List<TextChunk> SplitPlainText(string text)
        {
            var tokens = _tokenizerService.Encode(text, _settings.TokenizerEncoder);

            if (tokens != null)
            {
                _logger.LogInformation("The tokenizer identified {TokensCount} tokens.", tokens.Count);

                var chunksCount = (int)Math.Ceiling((1f * tokens!.Count - _settings.OverlapSizeTokens) / (_settings.ChunkSizeTokens - _settings.OverlapSizeTokens));

                if (chunksCount <= 1)
                    return new List<TextChunk> { new()
                    {
                        Position = 1,
                        Content = text,
                        TokensCount = tokens.Count
                    } };

                var chunks = Enumerable.Range(0, chunksCount - 1)
                    .Select(i => new
                        {
                            Position = i + 1,
                            Tokens = tokens.Skip(i * (_settings.ChunkSizeTokens - _settings.OverlapSizeTokens)).Take(_settings.ChunkSizeTokens).ToArray()
                        })
                    .Select(x => new TextChunk {
                        Position = x.Position,
                        Content = _tokenizerService.Decode(x.Tokens, _settings.TokenizerEncoder),
                        TokensCount = x.Tokens.Length })
                    .ToList();

                var lastChunkStart = (chunksCount - 1) * (_settings.ChunkSizeTokens - _settings.OverlapSizeTokens);
                var lastChunkSize = tokens.Count - lastChunkStart + 1;

                if (lastChunkSize < 2 * _settings.OverlapSizeTokens)
                {
                    // The last chunk is to small, will just incorporate it into the second to last.
                    var secondToLastChunkStart = (chunksCount - 2) * (_settings.ChunkSizeTokens - _settings.OverlapSizeTokens);
                    var newLastChunkSize = tokens.Count - secondToLastChunkStart + 1;
                    var newLastChunk = _tokenizerService.Decode(
                        tokens
                            .Skip(secondToLastChunkStart)
                            .Take(newLastChunkSize)
                            .ToArray(),
                        _settings.TokenizerEncoder);
                    chunks.RemoveAt(chunks.Count - 1);
                    chunks.Add(new TextChunk {
                        Position = chunks.Count + 1,
                        Content = newLastChunk,
                        TokensCount = newLastChunkSize
                    });
                }
                else
                {
                    var lastChunk = _tokenizerService.Decode(
                        tokens
                            .Skip(lastChunkStart)
                            .Take(lastChunkSize)
                            .ToArray(),
                        _settings.TokenizerEncoder);
                    chunks.Add(new TextChunk {
                        Position = chunks.Count + 1,
                        Content = lastChunk,
                        TokensCount = lastChunkSize
                    });
                }

                return chunks;
            }
            else
                throw new TextProcessingException("The tokenizer service failed to split the text into tokens.");
        }
    }
}
