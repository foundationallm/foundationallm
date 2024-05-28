using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents the result of a role assignment request.
    /// </summary>
    public class RoleAssignmentResult
    {
        /// <summary>
        /// Indicates whether the role assignment was successful or not.
        /// </summary>
        [JsonPropertyName("success")]
        public required bool Success { get; set; }
    }
}
