using FoundationaLLM.SemanticKernel.Core.Interfaces;

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Implements the <see cref="ITokenizer"/> interface.
    /// </summary>
    public class SemanticKernelTokenizer : ITokenizer
    {
        /// <summary>
        /// Gets the number of tokens for the input text.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <returns>The number of tokens.</returns>
        public int GetTokensCount(string text)
        {
            return 0; //return GPT3Tokenizer.Encode(text).Count;
        }
    }
}
