using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <inheritdoc/>
    public class OrchestrationContext : IOrchestrationContext
    {
        /// <inheritdoc/>
        public UnifiedUserIdentity? CurrentUserIdentity { get; set; }
        /// <inheritdoc/>
        public string? InstanceId { get; set; }
    }
}
