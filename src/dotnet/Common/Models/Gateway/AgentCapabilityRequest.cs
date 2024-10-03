namespace FoundationaLLM.Common.Models.Gateway
{
    /// <summary>
    /// Provides the details required to create an agent capability.
    /// </summary>
    public class AgentCapabilityRequest
    {
        /// <summary>
        /// The category of the capability.
        /// </summary>
        public required string CapabilityCategory { get; set; }

        /// <summary>
        /// The name of the capability to be created.
        /// </summary>
        public required string CapabilityName { get; set; }

        /// <summary>
        /// The dictionary of parameter values used to create the capability.
        /// </summary>
        public required Dictionary<string, object> Parameters { get; set; } = [];
    }
}
