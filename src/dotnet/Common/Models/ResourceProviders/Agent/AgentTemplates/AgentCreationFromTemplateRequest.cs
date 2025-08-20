using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentTemplates
{
    /// <summary>
    /// Represents a request to create an agent from a template.
    /// </summary>
    public class AgentCreationFromTemplateRequest
    {
        /// <summary>
        /// Gets or sets the dictionary of template parameters and their values.
        /// </summary>
        [JsonPropertyName("template_parameters")]
        public Dictionary<string, string> TemplateParameters { get; set; } = [];
    }
}
