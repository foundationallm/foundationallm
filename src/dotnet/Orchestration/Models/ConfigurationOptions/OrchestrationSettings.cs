namespace FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Settings for an Agent Factory.  Currenlty only sets the default orchestration (Semantickernal, LangChain)
    /// </summary>
    public record OrchestrationSettings
    {
        /// <summary>
        /// The default orchenstration service (SemanticKernal, LangChain)
        /// </summary>
        public string? DefaultOrchestrationService { init; get; }
        // TODO: integrate the agent factory settings
    }
}
