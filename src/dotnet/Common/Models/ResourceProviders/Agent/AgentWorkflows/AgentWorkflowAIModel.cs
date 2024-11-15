using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Associates a workflow AI model and its associated model parameter overrides.
    /// </summary>
    public class AgentWorkflowAIModel
    {
        /// <summary>
        /// The AI model object ID.
        /// </summary>
        [JsonPropertyName("ai_model_object_id")]
        public required string AIModelObjectId { get; set; }

        /// <summary>
        /// Dictionary with override values for the model parameters.
        /// <para>
        /// For the list of supported keys, see <see cref="ModelParametersKeys"/>.
        /// </para>
        /// </summary>
        [JsonPropertyName("model_parameters")]
        public Dictionary<string, object>? ModelParameters { get; set; }
    }
}
