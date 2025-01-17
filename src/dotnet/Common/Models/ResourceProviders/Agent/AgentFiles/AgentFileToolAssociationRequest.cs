namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Request object to associate a tool with an agent file.
    /// Agent and File are obtained from the request URL.
    /// </summary>
    public class AgentFileToolAssociationRequest
    {
        /// <summary>
        /// The object ID of the the tool to associate with the agent file.
        /// </summary>
        public required string ToolObjectId { get; set; }
    }
}
