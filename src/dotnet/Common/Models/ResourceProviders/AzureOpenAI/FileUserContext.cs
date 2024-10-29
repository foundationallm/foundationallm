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
        /// The name of the assistant user context name.
        /// </summary>
        [JsonPropertyName("assistant_user_context_name")]
        [JsonPropertyOrder(101)]
        public required string AssistantUserContextName { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of <see cref="AzureOpenAI.AgentFileUserContext"/> objects providing information about the OpenAI files associated with the user.
        /// </summary>
        /// <remarks>
        /// The keys in the dictionary are the object identifiers of the FoundationaLLM agents that are backed by Azure OpenAI Assistants capabilities.
        /// </remarks>
        [JsonPropertyName("agent_files")]
        [JsonPropertyOrder(102)]
        public Dictionary<string, AgentFileUserContext> AgentFiles { get; set; } = [];

        /// <summary>
        /// Gets the <see cref="FileMapping"/> associated with the specified file identifier.
        /// </summary>
        /// <param name="fileId">The Azure OpenAI Assistants file identifier whose mapping is being retrieved.</param>
        /// <param name="agentFileUserContext">The <see cref="AgentFileUserContext"/> object where the file mapping was found.</param>
        /// <param name="fileMapping">The <see cref="FileMapping"/> object containing the file mapping.</param>
        /// <returns> if</returns>
        public bool TryGetFileMapping(string fileId, out AgentFileUserContext? agentFileUserContext, out FileMapping? fileMapping)
        {
            foreach (var internalAgentFileUserContext in AgentFiles.Values)
            {
                fileMapping = internalAgentFileUserContext.Files.Values.FirstOrDefault(fm => fm.OpenAIFileId == fileId);
                if(fileMapping is not null)
                {
                    agentFileUserContext = internalAgentFileUserContext;
                    return true;
                }
            }
            agentFileUserContext = null;
            fileMapping = null;
            return false;
        }
    }
}
