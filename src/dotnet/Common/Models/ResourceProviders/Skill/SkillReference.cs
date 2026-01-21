using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Represents a skill stored in Cosmos DB for procedural memory.
    /// Skills are scoped to an agent-user combination.
    /// </summary>
    public class SkillReference
    {
        /// <summary>
        /// The unique identifier of the skill (Cosmos DB document id).
        /// Format: {skillName}_{agentId}_{userId}
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// The document type for Cosmos DB filtering.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "skill";

        /// <summary>
        /// The user principal name (UPN) - used as partition key.
        /// </summary>
        [JsonPropertyName("upn")]
        public required string UPN { get; set; }

        /// <summary>
        /// The object ID of the agent that this skill belongs to.
        /// </summary>
        [JsonPropertyName("agent_object_id")]
        public required string AgentObjectId { get; set; }

        /// <summary>
        /// The display name of the skill.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// A description of what the skill does.
        /// Used for semantic search.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// The Python code that implements this skill.
        /// </summary>
        [JsonPropertyName("code")]
        public required string Code { get; set; }

        /// <summary>
        /// Example prompts that this skill can handle.
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
        /// The approval status of the skill.
        /// </summary>
        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SkillStatus Status { get; set; } = SkillStatus.Active;

        /// <summary>
        /// Number of times this skill has been executed.
        /// </summary>
        [JsonPropertyName("execution_count")]
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Success rate (0.0 to 1.0) based on execution history.
        /// </summary>
        [JsonPropertyName("success_rate")]
        public double SuccessRate { get; set; } = 1.0;

        /// <summary>
        /// The version of this skill.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        /// <summary>
        /// The vector embedding for semantic search.
        /// </summary>
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }

        /// <summary>
        /// The time when the skill was created.
        /// </summary>
        [JsonPropertyName("created_on")]
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The time when the skill was last updated.
        /// </summary>
        [JsonPropertyName("updated_on")]
        public DateTimeOffset UpdatedOn { get; set; }

        /// <summary>
        /// Indicates whether the skill has been deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; } = false;
    }
}
