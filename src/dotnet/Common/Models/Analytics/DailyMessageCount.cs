using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Represents daily message counts per agent.
    /// </summary>
    public class DailyMessageCount
    {
        /// <summary>
        /// The date for this count.
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Dictionary of agent names and their message counts for this date.
        /// </summary>
        [JsonPropertyName("agent_counts")]
        public Dictionary<string, int> AgentCounts { get; set; } = new();
    }
}

