using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Represents daily active user count.
    /// </summary>
    public class DailyActiveUserCount
    {
        /// <summary>
        /// The date for this count.
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of active users on this date.
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}

