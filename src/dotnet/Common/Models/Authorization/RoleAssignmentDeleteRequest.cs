using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a request to delete an existing role assignment.
    /// </summary>
    public class RoleAssignmentDeleteRequest
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        public required string ObjectId { get; set; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonPropertyOrder(-5)]
        public required string Name { get; set; }
    }
}
