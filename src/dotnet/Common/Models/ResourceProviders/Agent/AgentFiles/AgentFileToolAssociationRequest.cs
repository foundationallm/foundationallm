using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Request object to associate a tool with an agent file.
    /// Agent and File are obtained from the request URL.
    /// </summary>
    public class AgentFileToolAssociationRequest
    {
        /// <summary>
        /// The object ID of the file to associate with the tool.
        /// </summary>
        [JsonPropertyName("agent_object_id")]
        public required string AgentObjectId { get; set; }

        /// <summary>
        /// The agent file tool association matrix.
        /// </summary>
        [JsonPropertyName("agent_file_tool_associations")]
        public required Dictionary<string, Dictionary<string, bool>> AgentFileToolAssociations {  get; set; }
    }
}
