using FoundationaLLM.Common.Interfaces;
using Tiktoken;
using Tiktoken.Encodings;

namespace FoundationaLLM.Common.Services.Tokenizers
{
    /// <summary>
    /// A tokenizer service that uses the Tiktoken library to encode and decode text into tokens.
    /// </summary>
    public class TryAGITokenizer : ITokenizerService
    {
        private readonly Encoder _encoder =
            new(new Cl100KBase());

        /// <inheritdoc/>
        public long CountTokens(string text, string? encoderName = null) =>
            _encoder.CountTokens(text);

        /// <inheritdoc/>
        public string Decode(int[] tokens, string? encoderName = null) =>
            _encoder.Decode(tokens);

        /// <inheritdoc/>
        public List<int> Encode(string text, string? encoderName = null) =>
            _encoder.Encode(text).ToList();
    }
}
