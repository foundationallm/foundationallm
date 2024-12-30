namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides details about a code execution session.
    /// </summary>
    public class CodeExecutionSession
    {
        /// <summary>
        /// The unique identifier for the code execution session.
        /// </summary>
        public required string SessionId { get; set; }

        /// <summary>
        /// The endpoint used to execute the code.
        /// </summary>
        public required string Endpoint { get; set; }
    }
}
