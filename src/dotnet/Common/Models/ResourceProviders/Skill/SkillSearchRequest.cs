using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Request model for searching skills using semantic similarity.
    /// </summary>
    public class SkillSearchRequest
    {
        /// <summary>
        /// The search query (natural language description of desired skill).
        /// </summary>
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        /// <summary>
        /// The agent object ID. Required - skills are scoped to agent-user combination.
        /// </summary>
        [JsonPropertyName("agent_object_id")]
        public required string AgentObjectId { get; set; }

        /// <summary>
        /// The user ID (UPN). Required - skills are scoped to agent-user combination.
        /// </summary>
        [JsonPropertyName("user_id")]
        public required string UserId { get; set; }

        /// <summary>
        /// Optional tags to filter results.
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Minimum similarity score (0.0 to 1.0). Default: 0.7
        /// </summary>
        [JsonPropertyName("min_similarity")]
        public double MinSimilarity { get; set; } = 0.7;

        /// <summary>
        /// Maximum number of results to return. Default: 5
        /// </summary>
        [JsonPropertyName("max_results")]
        public int MaxResults { get; set; } = 5;

        /// <summary>
        /// Only return skills with this status. Default: Active only.
        /// </summary>
        [JsonPropertyName("status_filter")]
        public SkillStatus? StatusFilter { get; set; } = SkillStatus.Active;
    }
}
