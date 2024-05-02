using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Models.Chat
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
        public string Type { get; set; }
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
        public Citation[]? Citations { get; set; }

        /// <summary>
        /// Constructor for Completion Prompt.
        /// </summary>
        public CompletionPrompt(string sessionId, string messageId, string prompt, Citation[]? citations = null)
        {
            Id = Guid.NewGuid().ToString();
            Type = nameof(CompletionPrompt);
            SessionId = sessionId;
            MessageId = messageId;
            Prompt = prompt;
            Citations = citations;
        }
    }
}
