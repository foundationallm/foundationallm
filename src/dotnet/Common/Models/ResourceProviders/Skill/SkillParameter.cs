using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Defines an input parameter for a skill.
    /// </summary>
    public class SkillParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// The Python type of the parameter (e.g., "str", "int", "float", "list", "dict").
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        /// <summary>
        /// A description of the parameter's purpose.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Whether the parameter is required.
        /// </summary>
        [JsonPropertyName("required")]
        public bool Required { get; set; } = true;

        /// <summary>
        /// The default value for the parameter, if any.
        /// </summary>
        [JsonPropertyName("default_value")]
        public object? DefaultValue { get; set; }
    }
}
