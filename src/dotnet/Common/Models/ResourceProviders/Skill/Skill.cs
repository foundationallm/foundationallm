using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Represents a reusable code skill that can be executed deterministically.
    /// Skills are scoped to an agent-user combination for procedural memory.
    /// </summary>
    public class Skill : ResourceBase
    {
        /// <summary>
        /// The Python code that implements this skill.
        /// </summary>
        [JsonPropertyName("code")]
        public required string Code { get; set; }

        /// <summary>
        /// Example prompts that this skill can handle.
        /// Improves skill discovery via semantic matching.
        /// </summary>
        [JsonPropertyName("example_prompts")]
        public List<string> ExamplePrompts { get; set; } = [];

        /// <summary>
        /// Input parameters expected by the skill code.
        /// </summary>
        [JsonPropertyName("parameters")]
        public List<SkillParameter> Parameters { get; set; } = [];

        /// <summary>
        /// Tags for categorization and filtering.
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];

        /// <summary>
        /// The object ID of the agent that this skill belongs to.
        /// Required - skills are always scoped to an agent.
        /// </summary>
        [JsonPropertyName("owner_agent_object_id")]
        public required string OwnerAgentObjectId { get; set; }

        /// <summary>
        /// The user ID (UPN) of the user that this skill belongs to.
        /// Required - skills are scoped to the agent-user combination.
        /// Example: "user@example.com"
        /// </summary>
        [JsonPropertyName("owner_user_id")]
        public required string OwnerUserId { get; set; }

        /// <summary>
        /// The approval status of the skill.
        /// Skills with status "Active" are available for use.
        /// </summary>
        [JsonPropertyName("status")]
        public SkillStatus Status { get; set; } = SkillStatus.Active;

        /// <summary>
        /// Number of times this skill has been successfully executed.
        /// </summary>
        [JsonPropertyName("execution_count")]
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Success rate (0.0 to 1.0) based on execution history.
        /// </summary>
        [JsonPropertyName("success_rate")]
        public double SuccessRate { get; set; } = 1.0;

        /// <summary>
        /// The version of this skill (for skill evolution).
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        /// <summary>
        /// The vector embedding of the skill description and example prompts.
        /// Used for semantic search.
        /// </summary>
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }
    }
}
