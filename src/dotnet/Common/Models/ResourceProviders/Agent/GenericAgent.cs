using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// The Generic agent metadata model.
    /// </summary>
    public class GenericAgent : AgentBase
    {
        /// <summary>
        /// Set default property values.
        /// </summary>
        public GenericAgent() =>
            Type = AgentTypes.GenericAgent;
    }
}
