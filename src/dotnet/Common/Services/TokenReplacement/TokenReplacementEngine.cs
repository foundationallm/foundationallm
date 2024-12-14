using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Services.TokenReplacement
{
    /// <summary>
    /// Engine for replacing tokens in strings.
    /// </summary>
    public class TokenReplacementEngine
    {
        /// <summary>
        /// Replaces tokens in the input string with the corresponding values.
        /// </summary>
        /// <param name="input">The string from which to replace the tokens.</param>
        /// <returns>Input string with tokens replaced.</returns>
        public static string ReplaceTokens(string input)
        {
            if(string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            if (input.Contains(PromptTokens.CurrentDateUTC))
            {
                input = input.Replace(PromptTokens.CurrentDateUTC, DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy"));
            }
            return input;
            
        }
    }
}
