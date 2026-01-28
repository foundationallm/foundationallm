namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides a collection of predefined parameter names used in agent templates.
    /// </summary>
    public static class AgentTemplateParameterNames
    {
        /// <summary>
        /// The agent name.
        /// </summary>
        public const string AgentName = "AGENT_NAME";

        /// <summary>
        /// The display name of the agent.
        /// </summary>
        public const string AgentDisplayName = "AGENT_DISPLAY_NAME";

        /// <summary>
        /// The expiration date of the agent.
        /// </summary>
        public const string AgentExpirationDate = "AGENT_EXPIRATION_DATE";

        /// <summary>
        /// The list of tools used by the agent.
        /// </summary>
        public const string AgentTools = "AGENT_TOOLS";

        /// <summary>
        /// The identifier of the agent's virtual security group.
        /// </summary>
        public const string VirtualSecurityGroupId = "VIRTUAL_SECURITY_GROUP_ID";

        /// <summary>
        /// The LLM used by the agent's workflow and some of the tools.
        /// </summary>
        public const string MainLLM = "MAIN_LLM";

        /// <summary>
        /// The LLM used by the agent's Knowledge tool.
        /// </summary>
        public const string MainKnowledgeLLM = "MAIN_KNOWLEDGE_LLM";
    }
}
