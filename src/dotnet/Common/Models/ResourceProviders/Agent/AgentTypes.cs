namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Contains constants for the types of agents.
    /// </summary>
    public static class AgentTypes
    {
        /// <summary>
        /// Basic agent without practical functionality. Used as base for all the other agents.
        /// </summary>
        public const string Basic = "basic";

        /// <summary>
        /// Knowledge Management agents are best for Q&amp;A, summarization, and reasoning over textual data.
        /// </summary>
        public const string KnowledgeManagement = "knowledge-management";

        /// <summary>
        /// Analytic agents are best for querying, analyzing, calculating, and reporting on tabular data.
        /// </summary>
        public const string Analytic = "analytic";
    }
}
