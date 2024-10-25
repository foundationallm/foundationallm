using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request
{
    /// <summary>
    /// Provides settings for a completion request of type <see cref="CompletionRequest"/>.
    /// </summary>
    public class OrchestrationSettings
    {
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
