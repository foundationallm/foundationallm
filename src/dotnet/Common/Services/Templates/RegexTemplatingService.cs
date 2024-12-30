using FoundationaLLM.Common.Constants.Templates;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Common.Services.Templates
{
    /// <summary>
    /// Templating engine that uses regular expressions to replace tokens in strings.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    public partial class RegexTemplatingService(
        ILogger<RegexTemplatingService> logger) : ITemplatingService
    {
        /// <summary>
        /// Regular expression pattern for template variables.
        /// </summary>
        private const string REGEX_VARIABLE_PATTERN = "\\{\\{foundationallm:(.*?)\\}\\}";

        private readonly ILogger<RegexTemplatingService> _logger = logger;

        [GeneratedRegex(REGEX_VARIABLE_PATTERN, RegexOptions.Compiled)]
        private static partial Regex VariableRegex();

        /// <inheritdoc/>
        public string Transform(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }

            try
            {
                // Expects the format {{foundationallm:variable_name[:format]}}

                var matches = VariableRegex().Matches(s);
                Dictionary<string, string> replacements = [];

                foreach (Match match in matches)
                {
                    var matchedVariable = match.Value;

                    var variableTokens = match.Groups[1].Value.Split(":", 2);
                    var variableName = variableTokens[0];
                    var variableFormat = variableTokens.Length > 1 ? variableTokens[1] : null;

                    switch (variableName)
                    {
                        case TemplateVariables.CurrentDateTimeUTC:
                            replacements.Add(
                                matchedVariable,
                                string.IsNullOrWhiteSpace(variableFormat)
                                    ? DateTime.UtcNow.ToString()
                                    : DateTime.UtcNow.ToString(variableFormat));
                            break;
                        default:
                            break;
                    }
                }

                var transformedString = s;
                foreach (var replacement in replacements)
                {
                    transformedString = transformedString.Replace(replacement.Key, replacement.Value);
                }

                return transformedString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while transforming the string.");
            }

            return s;
        }
    }
}
