using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Basic model for resources managed by the FoundationaLLM.AzureOpenAI resource manager.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AzureOpenAIConversationMapping), AzureOpenAITypes.ConversationMapping)]
    [JsonDerivedType(typeof(AzureOpenAIFileMapping), AzureOpenAITypes.FileMapping)]
    public class AzureOpenAIResourceBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The unique identifier of the conversation mapping.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The logical partition key for the conversation mapping.
        /// </summary>
        /// <remarks>
        /// This property is used by storage providers that support partitioning of data (e.g. Azure Cosmos DB).
        /// </remarks>
        public string PartitionKey =>
            $"{UPN.NormalizeUserPrincipalName()}-{InstanceId}";

        /// <summary>
        /// The user principal name (UPN) of the user who created the conversation mapping.
        /// </summary>
        public required string UPN { get; set; }

        /// <summary>
        /// The FoundationaLLM instance identifier
        /// </summary>
        public required string InstanceId { get; set; }

        /// <summary>
        /// The Azure OpenAI endpoint.
        /// </summary>
        public required string OpenAIEndpoint { get; set; }
    }
}
