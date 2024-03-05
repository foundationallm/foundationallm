using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Completion request object that excludes session-based properties.
    /// </summary>
    public class DirectCompletionRequest : OrchestrationRequest
    {
    }
}
