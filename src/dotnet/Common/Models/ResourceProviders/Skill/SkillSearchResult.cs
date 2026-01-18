using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Result model for a skill search match.
    /// </summary>
    public class SkillSearchResult
    {
        /// <summary>
        /// The matching skill.
        /// </summary>
        [JsonPropertyName("skill")]
        public required SkillReference Skill { get; set; }

        /// <summary>
        /// The similarity score (0.0 to 1.0) indicating how well the skill matches the query.
        /// </summary>
        [JsonPropertyName("similarity")]
        public double Similarity { get; set; }
    }
}
