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
        public string Transform(string s, Dictionary<string, string>? tokenAndValues = null)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }
            if (tokenAndValues is null)
            {
                tokenAndValues = new Dictionary<string, string>();
            }

            try
            {
                // Expects the format {{foundationallm:variable_name[:format]}}
                var transformedString = s;
                bool hasMoreTokens;

                do
                {
                    hasMoreTokens = false;
                    var matches = VariableRegex().Matches(transformedString);
                    Dictionary<string, string> replacements = new Dictionary<string, string>();

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
                            case TemplateVariables.RouterPrompt:
                                if (tokenAndValues.TryGetValue(TemplateVariables.RouterPrompt, out string? router_prompt_value))
                                    replacements.Add(matchedVariable, router_prompt_value);
                                else
                                    replacements.Add(matchedVariable, string.Empty);
                                break;
                            case TemplateVariables.ToolRouterPrompts:
                                if (tokenAndValues.TryGetValue(TemplateVariables.ToolRouterPrompts, out string? tool_router_prompts_value))
                                    replacements.Add(matchedVariable, tool_router_prompts_value);
                                else
                                    replacements.Add(matchedVariable, string.Empty);
                                break;
                            case TemplateVariables.ToolList:
                                if (tokenAndValues.TryGetValue(TemplateVariables.ToolList, out string? tools_list_value))
                                    replacements.Add(matchedVariable, tools_list_value);
                                else
                                    replacements.Add(matchedVariable, string.Empty);
                                break;
                            default:
                                break;
                        }
                    }

                    foreach (var replacement in replacements)
                    {
                        transformedString = transformedString.Replace(replacement.Key, replacement.Value);
                    }

                    hasMoreTokens = VariableRegex().IsMatch(transformedString);

                } while (hasMoreTokens);

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
