using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    /// <summary>
    /// Azure AI Evaluation Job Request
    /// </summary>
    public class AzureAIJobRequest
    {
        /// <summary>
        /// The target flow to run against.
        /// </summary>
        [JsonPropertyName("flowDefinitionResourceId")]
        public string? FlowDefinitionResourceId { get; set; }
        /// <summary>
        /// The user generated job id.
        /// </summary>
        [JsonPropertyName("runId")]
        public string? RunId { get; set; }
        /// <summary>
        /// Uri to the data to test.
        /// </summary>
        [JsonPropertyName("batchDataInput")]
        public BatchDataInput? BatchDataInput { get; set; }
        /// <summary>
        /// All model connection information for the various tests.
        /// </summary>
        [JsonPropertyName("connections")]
        public GptConnections? Connections { get; set; }
        /// <summary>
        /// How each of the input fields map to the testing fields.
        /// </summary>
        [JsonPropertyName("inputsMapping")]
        public InputsMapping? InputsMapping { get; set; }
        /// <summary>
        /// A name for the job, can be the run Id.
        /// </summary>
        [JsonPropertyName("runExpermimentName")]
        public string? RunExperimentName { get; set; }
        /// <summary>
        /// The target run time for Azure AI studio.
        /// </summary>
        [JsonPropertyName("runtimeName")]
        public string? RuntimeName { get; set; }
        /// <summary>
        /// The generation type name.
        /// </summary>
        [JsonPropertyName("runDisplayNameGenerationType")]
        public string? RunDisplayNameGenerationType { get; set; }
        /// <summary>
        /// Any extra properties to pass to the job.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, string>? Properties { get; set; }
        /// <summary>
        /// The setup mode, typically "SystemWait".
        /// </summary>
        [JsonPropertyName("sessionSetupMode")]
        public string? SessionSetupMode { get; set; }
    }
}
