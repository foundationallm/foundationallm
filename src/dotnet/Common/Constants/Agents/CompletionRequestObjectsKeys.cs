using FoundationaLLM.Common.Models.Orchestration.Request;

namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Contains constants for the keys that can be added to the <see cref="LLMCompletionRequest.Objects"/> dictionary.
    /// </summary>
    public static class CompletionRequestObjectsKeys
    {
        /// <summary>
        /// The key name for the dictionary containing names and descriptions of agents other than the completion request's agent.
        /// This value should be a dictionary where keys are agent names and values are agent descriptions.
        /// </summary>
        public const string AllAgents = "AllAgents";

        /// <summary>
        /// The key name for the OpenAI assistant identifier value.
        /// </summary>
        public const string OpenAIAssistantId = "OpenAI.AssistantId";

        /// <summary>
        /// The key name for the OpenAI assistant thread identifier value.
        /// </summary>
        public const string OpenAIAssistantThreadId = "OpenAI.AssistantThreadId";

        /// <summary>
        /// The key name for the Gateway API EndpointConfiguration identifier value.
        /// </summary>
        public const string GatewayAPIEndpointConfiguration = "GatewayAPIEndpointConfiguration";

        /// <summary>
        /// All completion request objects dictionary keys.
        /// </summary>
        public readonly static string[] All = [
            AllAgents
        ];
    }
}
