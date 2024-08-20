namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// The Knowledge Management agent metadata model.
    /// </summary>
    public class AudioClassificationAgent : KnowledgeManagementAgent
    {
        /// <summary>
        /// Set default property values.
        /// </summary>
        public AudioClassificationAgent() =>
            Type = AgentTypes.AudioClassification;
    }
}
