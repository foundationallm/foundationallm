namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides well-known parameter names for agent tools.
    /// </summary>
    public static class AgentToolPropertyNames
    {
        /// <summary>
        /// Indicates whether code execution is enabled or not.
        /// </summary>
        public const string FoundationaLLM_AzureContainerApps_CodeExecution_Enabled = "foundationallm_aca_code_execution_enabled";

        /// <summary>
        /// The session identifier required to execute code in Azure Container Apps Dynamic Sessions.
        /// </summary>
        public const string FoundationaLLM_AzureContainerApps_CodeExecution_SessionId = "foundationallm_aca_code_execution_session_id";

        /// <summary>
        /// The endpoint required to execute code in Azure Container Apps Dynamic Sessions.
        /// </summary>
        public const string FoundationaLLM_AzureContainerApps_CodeExecution_Endpoint = "foundationallm_aca_code_execution_endpoint";
    }
}
