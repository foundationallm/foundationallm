using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Configuration for agent procedural memory capabilities.
    /// When enabled is false, the Code Interpreter tool behaves exactly as before.
    /// </summary>
    public class ProceduralMemorySettings
    {
        /// <summary>
        /// Whether procedural memory (skill learning) is enabled.
        /// When false, the Code Interpreter tool works in backwards-compatible mode.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Whether the agent can automatically register new skills.
        /// If false, skills must be registered manually/by admins.
        /// </summary>
        [JsonPropertyName("auto_register_skills")]
        public bool AutoRegisterSkills { get; set; } = true;

        /// <summary>
        /// Whether newly registered skills require admin approval before becoming active.
        /// Default is false (auto-approve).
        /// When true, new skills are created with status "PendingApproval".
        /// </summary>
        [JsonPropertyName("require_skill_approval")]
        public bool RequireSkillApproval { get; set; } = false;

        /// <summary>
        /// Maximum number of skills per agent-user combination.
        /// 0 = unlimited.
        /// </summary>
        [JsonPropertyName("max_skills_per_user")]
        public int MaxSkillsPerUser { get; set; } = 0;

        /// <summary>
        /// Similarity threshold for skill retrieval (0.0 to 1.0).
        /// Higher values = more precise matching.
        /// </summary>
        [JsonPropertyName("skill_search_threshold")]
        public double SkillSearchThreshold { get; set; } = 0.8;

        /// <summary>
        /// Whether to prefer using existing skills over generating new code.
        /// When true, agent will search for skills first before generating code.
        /// </summary>
        [JsonPropertyName("prefer_skills")]
        public bool PreferSkills { get; set; } = true;
    }
}
