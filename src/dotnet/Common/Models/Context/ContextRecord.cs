using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a file record.
    /// </summary>
    public abstract class ContextRecord
    {
        /// <summary>
        /// Gets or sets the Cosmos DB identifier.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonPropertyOrder(-100)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the context record.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-99)]
        public abstract string Type { get; set; }

        /// <summary>
        /// Gets or sets the FoundationaLLM instance identifier.
        /// </summary>
        [JsonPropertyName("instance_id")]
        [JsonPropertyOrder(-98)]
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the user prinicpal name of the user that owns the record.
        /// </summary>
        [JsonPropertyName("upn")]
        [JsonPropertyOrder(-97)]
        public string UPN { get; set; }

        /// <summary>
        /// Gets or sets the origin of the context record.
        /// </summary>
        [JsonPropertyName("origin")]
        [JsonPropertyOrder(-96)]
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the time of the record creation.
        /// </summary>
        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(100)]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the user that created the record.
        /// </summary>
        [JsonPropertyName("created_by")]
        [JsonPropertyOrder(101)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the time of the record update.
        /// </summary>
        [JsonPropertyName("updated_at")]
        [JsonPropertyOrder(102)]
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user that updated the record.
        /// </summary>
        [JsonPropertyName("updated_by")]
        [JsonPropertyOrder(103)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the flag that indicates if the record was deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        [JsonPropertyOrder(104)]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the context record.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonPropertyOrder(105)]
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextRecord"/> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ContextRecord()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            // Required for deserialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextRecord"/> class.
        /// </summary>
        /// <param name="id">The identifier of the context record.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="origin">The origin of the context record.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        /// <param name="metadata">Optional metadata dictionary associated with the context record.</param>
        public ContextRecord(
            string id,
            string instanceId,
            string origin,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata = null)
        {
            Id = id;
            InstanceId = instanceId;
            UPN = userIdentity.UPN!;
            Origin = origin;
            Metadata = metadata ?? [];
            CreatedAt = DateTimeOffset.UtcNow;
            CreatedBy = userIdentity.UPN!;
        }
    }
}
