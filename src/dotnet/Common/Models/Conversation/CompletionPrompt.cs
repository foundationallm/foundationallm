using FoundationaLLM.Common.Models.Orchestration.Response;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// The completion prompt object.
    /// </summary>
    public class CompletionPrompt
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The type of the completion.
        /// </summary>
        public string Type { get; set; } = nameof(CompletionPrompt);
        /// <summary>
        /// The sessionId associated with the completion.
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// The messageId of the completion.
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// The completion prompt.
        /// </summary>
        public string Prompt { get; set; }
        /// <summary>
        /// Deleted flag used for soft delete.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// The sources used in the creation of the completion response.
        /// </summary>
        public ContentArtifact[]? ContentArtifacts { get; set; }
    }
}
