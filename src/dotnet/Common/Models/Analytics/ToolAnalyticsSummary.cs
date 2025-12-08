using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Summary of analytics for a tool.
    /// </summary>
    public class ToolAnalyticsSummary
    {
        /// <summary>
        /// The name of the tool.
        /// </summary>
        [JsonPropertyName("tool_name")]
        public string ToolName { get; set; } = string.Empty;

        /// <summary>
        /// The total number of times this tool has been used.
        /// </summary>
        [JsonPropertyName("usage_frequency")]
        public int UsageFrequency { get; set; }

        /// <summary>
        /// The average execution time in milliseconds.
        /// </summary>
        [JsonPropertyName("avg_execution_time_ms")]
        public double AvgExecutionTimeMs { get; set; }

        /// <summary>
        /// The success rate as a percentage.
        /// </summary>
        [JsonPropertyName("success_rate")]
        public double SuccessRate { get; set; }

        /// <summary>
        /// The number of errors encountered.
        /// </summary>
        [JsonPropertyName("error_count")]
        public int ErrorCount { get; set; }

        /// <summary>
        /// The list of agents using this tool.
        /// </summary>
        [JsonPropertyName("agents_using_tool")]
        public List<string> AgentsUsingTool { get; set; } = [];
    }
}
