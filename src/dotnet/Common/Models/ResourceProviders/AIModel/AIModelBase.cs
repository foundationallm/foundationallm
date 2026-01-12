using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Provides a set of standard properties for all AIModel objects.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(EmbeddingAIModel), AIModelTypes.Embedding)]
    [JsonDerivedType(typeof(CompletionAIModel), AIModelTypes.Completion)]
    [JsonDerivedType(typeof(ImageGenerationAIModel), AIModelTypes.ImageGeneration)]
    [JsonDerivedType(typeof(RealtimeSpeechAIModel), AIModelTypes.RealtimeSpeech)]
    public class AIModelBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The object id of the <see cref="APIEndpointConfiguration"/> object providing the configuration for the API endpoint used to interact with the model.
        /// </summary>
        [JsonPropertyName("endpoint_object_id")]

        public required string EndpointObjectId { get; set; }

        /// <summary>
        /// The version of the AI model.
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        /// <summary>
        /// The name of the deployment corresponding to the AI model.
        /// </summary>
        [JsonPropertyName("deployment_name")]
        public string? DeploymentName { get; set; }

        /// <summary>
        /// Dictionary with default values for the model parameters.
        /// <para>
        /// For the list of supported keys, see <see cref="ModelParametersKeys"/>.
        /// </para>
        /// </summary>
        [JsonPropertyName("model_parameters")]
        public Dictionary<string, object> ModelParameters { get; set; } = [];
    }
}
