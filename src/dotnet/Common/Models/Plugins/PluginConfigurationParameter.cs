using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// Provides the model for a plugin configuration parameter.
    /// </summary>
    public class PluginConfigurationParameter
    {
        /// <summary>
        /// Gets or sets the <see cref="PluginParameterDefinition"/> that defines the parameter.
        /// </summary>
        [JsonPropertyName("parameter_definition")]
        public required PluginParameterDefinition ParameterDefinition { get; set; }

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        [JsonPropertyName("default_value")]
        public object? DefaultValue { get; set; }
    }
}
