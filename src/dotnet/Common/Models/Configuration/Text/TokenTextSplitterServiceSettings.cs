﻿using FoundationaLLM.Common.Exceptions;

namespace FoundationaLLM.Common.Models.Configuration.Text
{
    /// <summary>
    /// Provides configuration settings that control token-based text splitting.
    /// </summary>
    /// <param name="Tokenizer">The tokenizer used to split the test into tokens.</param>
    /// <param name="TokenizerEncoder">The name of the encoder used for tokenization.</param>
    /// <param name="ChunkSizeTokens">The target size in tokens for the resulting text chunks.</param>
    /// <param name="OverlapSizeTokens">Teh target size in tokens for the overlapping parts of the adjacent text chunks.</param>
    public record TokenTextSplitterServiceSettings(
        string Tokenizer,
        string TokenizerEncoder,
        int ChunkSizeTokens,
        int OverlapSizeTokens)
    {
        /// <summary>
        /// Creates and instance of the class based on a dictionary.
        /// </summary>
        /// <param name="settings">The dictionary containing the settings.</param>
        /// <returns>A <see cref="TokenTextSplitterServiceSettings"/> instance initialized with the values from the dictionary.</returns>
        public static TokenTextSplitterServiceSettings FromDictionary(Dictionary<string, string> settings)
        {
            if (settings.TryGetValue("tokenizer", out var tokenizer)
                && settings.TryGetValue("tokenizer_encoder", out var tokenizerEncoder)
                && settings.TryGetValue("chunk_size_tokens", out var chunkSizeTokens)
                && settings.TryGetValue("overlap_size_tokens", out var overlapSizeTokens))
                return new TokenTextSplitterServiceSettings(
                    tokenizer,
                    tokenizerEncoder,
                    int.Parse(chunkSizeTokens),
                    int.Parse(overlapSizeTokens));

            throw new TextProcessingException("Invalid text splitter settings.");
        }
    }
}
