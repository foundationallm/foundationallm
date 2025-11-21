using FoundationaLLM.Common.Models.Context;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides the result of downloading files from a code session.
    /// </summary>
    public class CodeSessionFileDownloadResponse
    {
        /// <summary>
        /// Gets or sets the dictionary containing the file identifiers and associated file records for each file that was successfully downloaded.
        /// </summary>
        [JsonPropertyName("file_records")]
        public Dictionary<string, ContextFileRecord> FileRecords { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of file paths that could not be downloaded from the code session or could not be added to the
        /// FoundationaLLM file store once they were downloaded.
        /// </summary>
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = [];
    }
}
