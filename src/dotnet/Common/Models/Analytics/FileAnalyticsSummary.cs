using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Summary of file analytics.
    /// </summary>
    public class FileAnalyticsSummary
    {
        /// <summary>
        /// The average number of files per conversation.
        /// </summary>
        [JsonPropertyName("avg_files_per_conversation")]
        public double AvgFilesPerConversation { get; set; }

        /// <summary>
        /// The total number of files uploaded.
        /// </summary>
        [JsonPropertyName("total_files")]
        public int TotalFiles { get; set; }

        /// <summary>
        /// The average file size in bytes.
        /// </summary>
        [JsonPropertyName("avg_file_size_bytes")]
        public double AvgFileSizeBytes { get; set; }

        /// <summary>
        /// The median file size in bytes.
        /// </summary>
        [JsonPropertyName("median_file_size_bytes")]
        public double MedianFileSizeBytes { get; set; }

        /// <summary>
        /// The total storage used in bytes.
        /// </summary>
        [JsonPropertyName("total_storage_bytes")]
        public long TotalStorageBytes { get; set; }

        /// <summary>
        /// File type distribution.
        /// </summary>
        [JsonPropertyName("file_type_distribution")]
        public Dictionary<string, int> FileTypeDistribution { get; set; } = new();
    }
}
