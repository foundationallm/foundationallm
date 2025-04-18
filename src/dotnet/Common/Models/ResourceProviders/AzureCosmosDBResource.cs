using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Basic model for resources persisted in Azure Cosmos DB.
    /// </summary>
    public class AzureCosmosDBResource : ResourceBase
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// The user principal name (UPN) of the user who created the resource.
        /// </summary>
        [JsonPropertyName("upn")]
        public required string UPN { get; set; }

        /// <summary>
        /// The FoundationaLLM instance identifier.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public required string InstanceId { get; set; }
    }
}
