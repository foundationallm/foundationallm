using FoundationaLLM.Common.Interfaces;
using Microsoft.ML.Tokenizers;

namespace FoundationaLLM.Common.Services.Tokenizers
{
    /// <summary>
    /// Represents a tokenizer service that uses the Microsoft ML Tokenizer.
    /// </summary>
    public class MicrosoftMLTokenizer : ITokenizerService
    {
        private readonly Tokenizer _tokenizer =
            TiktokenTokenizer.CreateForEncoding(TikTokenizerEncoders.CL100K_BASE);

        /// <summary>
        /// Gets the underlying tokenizer instance.
        /// </summary>
        public Tokenizer Tokenizer => _tokenizer;

        /// <inheritdoc/>
        public long CountTokens(string text, string? encoderName = null) =>
            _tokenizer.CountTokens(text);

        /// <inheritdoc/>
        public string Decode(int[] tokens, string? encoderName) =>
            _tokenizer.Decode(tokens);

        /// <inheritdoc/>
        public List<int> Encode(string text, string? encoderName) =>
            [.. _tokenizer.EncodeToIds(text)];
    }
}
