using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// The Knowledge Management agent metadata model.
    /// </summary>
    public class KnowledgeManagementAgent : AgentBase
    {
        /// <summary>
        /// Set default property values.
        /// </summary>
        public KnowledgeManagementAgent() =>
            Type = AgentTypes.KnowledgeManagement;
    }
}
