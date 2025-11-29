using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants.Context
{
    /// <summary>
    /// Provides constants for the types of file processing.
    /// </summary>
    public static class FileProcessingTypes
    {
        /// <summary>
        /// The file requires no processing.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// The file must be processed by a data pipeline.
        /// </summary>
        public const string DataPipeline = "data_pipeline";

        /// <summary>
        /// The file's content must be used in the completion request context (it's content should be directly embedded into the completion request).
        /// </summary>
        public const string CompletionRequestContext = "completion_request_context";

        /// <summary>
        /// All file processing types.
        /// </summary>
        public static readonly ImmutableArray<string> All = [
            None,
            DataPipeline,
            CompletionRequestContext
        ];
    }
}
