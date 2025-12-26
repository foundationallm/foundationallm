namespace FoundationaLLM.Common.Constants.Authorization
{
    /// <summary>
    /// Provides the names of the role definitions managed by the FoundationaLLM.Authorization provider.
    /// </summary>
    public static class RoleDefinitionNames
    {
        /// <summary>
        /// Manage access to FoundationaLLM resources by assigning roles using FoundationaLLM RBAC.
        /// </summary>
        public const string Role_Based_Access_Control_Administrator = "17ca4b59-3aee-497d-b43b-95dd7d916f99";

        /// <summary>
        /// View all resources without the possiblity of making any changes.
        /// </summary>
        public const string Reader = "00a53e72-f66e-4c03-8f81-7e885fd2eb35";

        /// <summary>
        /// Full access to manage all resources without the possiblity of assigning roles in FoundationaLLM RBAC.
        /// </summary>
        public const string Contributor = "a9f0020f-6e3a-49bf-8d1d-35fd53058edf";

        /// <summary>
        /// Manage access to FoundationaLLM resources.
        /// </summary>
        public const string User_Access_Administrator = "fb8e0fd0-f7e2-4957-89d6-19f44f7d6618";

        /// <summary>
        /// Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC.
        /// </summary>
        public const string Owner = "1301f8d4-3bea-4880-945f-315dbd2ddb46";

        /// <summary>
        /// Upload attachments including uploading to Azure OpenAI file store.
        /// </summary>
        public const string Attachments_Contributor = "8e77fb6a-7a78-43e1-b628-d9e2285fe25a";

        /// <summary>
        /// Create and update conversations, including Azure OpenAI Assistants threads.
        /// </summary>
        public const string Conversations_Contributor = "d0d21b90-5317-499a-9208-3a6cb71b84f9";

        /// <summary>
        /// Create new data pipelines.
        /// </summary>
        public const string Data_Pipelines_Contributor = "2da16a58-ed63-431a-b90e-9df32c2cae4a";

        /// <summary>
        /// Manage all aspects related to data pipeline runs.
        /// </summary>
        public const string Data_Pipelines_Execution_Manager = "e959eecb-8edf-4442-b532-4990f9a1df2b";

        /// <summary>
        /// Create new agents.
        /// </summary>
        public const string Agents_Contributor = "3f28aa77-a854-4aa7-ae11-ffda238275c9";

        /// <summary>
        /// Create new agent access tokens.
        /// </summary>
        public const string Agent_Access_Tokens_Contributor = "8c5ea0d3-f5a1-4be5-90a7-a12921c45542";

        /// <summary>
        /// Create new prompts.
        /// </summary>
        public const string Prompts_Contributor = "479e7b36-5965-4a7f-baf7-84e57be854aa";

        /// <summary>
        /// Create new vector databases.
        /// </summary>
        public const string Vector_Databases_Contributor = "c026f070-abc2-4419-aed9-ec0676f81519";

        /// <summary>
        /// Execute management actions on resource providers.
        /// </summary>
        public const string Resource_Providers_Administrator = "63b6cc4d-9e1c-4891-8201-cf58286ebfe6";

        /// <summary>
        /// Manage infrastructure resources including Azure Container Apps and Azure Kubernetes Service deployments.
        /// </summary>
        public const string Infrastructure_Contributor = "a7e0b2d4-8c3f-4e5a-9b1d-6f2c8a9e0d3b";

    }
}
