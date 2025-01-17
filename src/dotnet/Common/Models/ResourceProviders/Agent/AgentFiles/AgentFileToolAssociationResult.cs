namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Represents the result of associating a tool with a file.
    /// </summary>
    public class AgentFileToolAssociationResult
    {
        /// <summary>
        /// Indicates if the tool association was successful.
        /// </summary>
        public required bool Success { get; set; }
    }
}
