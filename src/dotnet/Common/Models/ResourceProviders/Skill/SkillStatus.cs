using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Skill approval status values.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SkillStatus
    {
        /// <summary>
        /// Skill is active and available for use.
        /// </summary>
        Active,

        /// <summary>
        /// Skill is pending approval (when require_skill_approval is enabled).
        /// </summary>
        PendingApproval,

        /// <summary>
        /// Skill was rejected by a user or administrator.
        /// </summary>
        Rejected,

        /// <summary>
        /// Skill has been disabled but not deleted.
        /// </summary>
        Disabled
    }
}
