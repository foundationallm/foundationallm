using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides information about the OpenAI artifacts associated with a user.
    /// </summary>
    public class FileUserContext : AzureOpenAIResourceBase
    {
        /// <summary>
        /// Set default property values.
        /// </summary>
        public FileUserContext() =>
            Type = AzureOpenAITypes.FileUserContext;

        /// <summary>
        /// The UPN (user principal name) to which the context is associated.
        /// </summary>
        [JsonPropertyName("user_principal_name")]
        [JsonPropertyOrder(100)]
        public required string UserPrincipalName { get; set; }

        /// <summary>
        /// The Azure OpenAI endpoint used to manage the assistant.
        /// </summary>
        [JsonPropertyName("endpoint")]
        [JsonPropertyOrder(101)]
        public required string Endpoint { get; set; }

        /// <summary>
        /// The dictionary of <see cref="FileMapping"/> objects providing information about the files uploaded to the OpenAI assistant. 
        /// </summary>
        /// <remarks>
        /// The keys of the dictionary are the FoundationaLLM attachment identifiers.
        /// </remarks>
        [JsonPropertyName("files")]
        [JsonPropertyOrder(102)]
        public Dictionary<string, FileMapping> Files { get; set; } = [];

        /// <summary>
        /// The name of the assistant user context name.
        /// </summary>
        [JsonPropertyName("assistant_user_context_name")]
        [JsonPropertyOrder(103)]
        public required string AssistantUserContextName { get; set; }
    }
}
