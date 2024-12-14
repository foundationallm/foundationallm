using System.Text.RegularExpressions;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace FoundationaLLM.Orchestration.Core.Services.TokenReplacement
{
    /// <summary>
    /// Token replacement engine, responsible for replacing tokens in a string with their computed values.
    /// </summary>
    public class TokenReplacementEngine
    {
        private readonly List<TokenReplacementDefinition> _tokenReplacements;
        private readonly ScriptOptions _scriptOptions;

        /// <summary>
        /// Creates an instance of the <see cref="TokenReplacementEngine"/> class.
        /// </summary>
        /// <param name="tokenReplacements"></param>
        public TokenReplacementEngine(List<TokenReplacementDefinition> tokenReplacements)
        {
            _tokenReplacements = tokenReplacements;

            // Define script options, such as referencing necessary assemblies and namespaces
            _scriptOptions = ScriptOptions.Default              
                .AddImports("System");
        }

        /// <summary>
        /// Replaces tokens in the input string with their corresponding computed values.
        /// </summary>
        /// <param name="input">The input string containing tokens to be replaced.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the string with tokens replaced.</returns>
        public async Task<string> ReplaceTokensAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            string pattern = @"{{\s*(\w+)\s*}}";
            return await Task.Run(() => Regex.Replace(input, pattern, match =>
            {
                string tokenName = match.Groups[1].Value;
                var tokenReplacement = _tokenReplacements.Find(tr => tr.Token == $"{{{{{tokenName}}}}}");

                if (tokenReplacement != null)
                {
                    try
                    {
                        // Evaluate the compute code
                        var result = CSharpScript.EvaluateAsync<string>(
                            tokenReplacement.ComputeCode,
                            _scriptOptions).Result;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        // Handle errors in compute code
                        return $"[Error: {ex.Message}]";
                    }
                }

                // If token not found, return it unchanged or handle as needed
                return match.Value;
            }, RegexOptions.IgnoreCase));
        }
    }
}
