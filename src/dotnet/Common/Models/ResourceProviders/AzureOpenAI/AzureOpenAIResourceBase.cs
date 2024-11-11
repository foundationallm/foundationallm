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
    public class AzureOpenAIResourceBase : AzureCosmosDBResource
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The logical partition key for the conversation mapping.
        /// </summary>
        /// <remarks>
        /// This property is used by storage providers that support partitioning of data (e.g. Azure Cosmos DB).
        /// </remarks>
        public string PartitionKey =>
            $"{UPN.NormalizeUserPrincipalName()}-{InstanceId}";

        /// <summary>
        /// The Azure OpenAI endpoint.
        /// </summary>
        public required string OpenAIEndpoint { get; set; }
    }
}
