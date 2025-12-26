namespace FoundationaLLM.Common.Constants.Authorization
{
    /// <summary>
    /// Provides the names of the authorizable actions managed by the FoundationaLLM.Authorization provider.
    /// </summary>
    public static class AuthorizableActionNames
    {
        #region Authorization

        /// <summary>
        /// Read role assignments.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleAssignments_Read = "FoundationaLLM.Authorization/roleAssignments/read";

        /// <summary>
        /// Create or update role assignments.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleAssignments_Write = "FoundationaLLM.Authorization/roleAssignments/write";

        /// <summary>
        /// Delete role assignments.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleAssignments_Delete = "FoundationaLLM.Authorization/roleAssignments/delete";

        /// <summary>
        /// Read role definitions.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleDefinitions_Read = "FoundationaLLM.Authorization/roleDefinitions/read";

        /// <summary>
        /// Security principals (users, groups, service principals).
        /// </summary>
        public const string FoundationaLLM_Authorization_SecurityPrincipals_Read = "FoundationaLLM.Authorization/securityPrincipals/read";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Authorization_Management_Write = "FoundationaLLM.Authorization/management/write";

        #endregion

        #region Agent

        /// <summary>
        /// Read agents.
        /// </summary>
        public const string FoundationaLLM_Agent_Agents_Read = "FoundationaLLM.Agent/agents/read";

        /// <summary>
        /// Create or update agents.
        /// </summary>
        public const string FoundationaLLM_Agent_Agents_Write = "FoundationaLLM.Agent/agents/write";

        /// <summary>
        /// Delete agents.
        /// </summary>
        public const string FoundationaLLM_Agent_Agents_Delete = "FoundationaLLM.Agent/agents/delete";

        /// <summary>
        /// Read workflows.
        /// </summary>
        public const string FoundationaLLM_Agent_Workflows_Read = "FoundationaLLM.Agent/workflows/read";

        /// <summary>
        /// Create or update workflows.
        /// </summary>
        public const string FoundationaLLM_Agent_Workflows_Write = "FoundationaLLM.Agent/workflows/write";

        /// <summary>
        /// Delete workflows.
        /// </summary>
        public const string FoundationaLLM_Agent_Workflows_Delete = "FoundationaLLM.Agent/workflows/delete";

        /// <summary>
        /// Read tools.
        /// </summary>
        public const string FoundationaLLM_Agent_Tools_Read = "FoundationaLLM.Agent/tools/read";

        /// <summary>
        /// Create or update tools.
        /// </summary>
        public const string FoundationaLLM_Agent_Tools_Write = "FoundationaLLM.Agent/tools/write";

        /// <summary>
        /// Delete tools.
        /// </summary>
        public const string FoundationaLLM_Agent_Tools_Delete = "FoundationaLLM.Agent/tools/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Agent_Management_Write = "FoundationaLLM.Agent/management/write";

        /// <summary>
        /// Read agent templates.
        /// </summary>
        public const string FoundationaLLM_Agent_Templates_Read = "FoundationaLLM.Agent/agentTemplates/read";

        /// <summary>
        /// Create or update agent templates.
        /// </summary>
        public const string FoundationaLLM_Agent_Templates_Write = "FoundationaLLM.Agent/agentTemplates/write";

        /// <summary>
        /// Delete agent templates.
        /// </summary>
        public const string FoundationaLLM_Agent_Templates_Delete = "FoundationaLLM.Agent/agentTemplates/delete";

        #endregion

        #region AzureAI

        /// <summary>
        /// Read Azure AI Agent Service conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureAI_AgentConversationMappings_Read = "FoundationaLLM.AzureAI/agentConversationMappings/read";

        /// <summary>
        /// Create or update Azure AI Agent Service conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureAI_AgentConversationMappings_Write = "FoundationaLLM.AzureAI/agentConversationMappings/write";

        /// <summary>
        /// Delete Azure AI Agent Service conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureAI_ConversationMappings_Delete = "FoundationaLLM.AzureAI/agentConversationMappings/delete";

        /// <summary>
        /// Read Azure AI Agent Service file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureAI_AgentFileMappings_Read = "FoundationaLLM.AzureAI/agentFileMappings/read";

        /// <summary>
        /// Create or update Azure AI Agent Service file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureAI_AgentFileMappings_Write = "FoundationaLLM.AzureAI/agentFileMappings/write";

        /// <summary>
        /// Delete Azure AI Agent Service file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureAI_AgentFileMappings_Delete = "FoundationaLLM.AzureAI/agentFileMappings/delete";

        /// <summary>
        /// Read Azure AI Agent Service project resources.
        /// </summary>
        public const string FoundationaLLM_AzureAI_Projects_Read = "FoundationaLLM.AzureAI/projects/read";

        /// <summary>
        /// Create or update Azure AI project resource.
        /// </summary>
        public const string FoundationaLLM_AzureAI_Projects_Write = "FoundationaLLM.AzureAI/projects/write";

        /// <summary>
        /// Delete Azure AI project resources.
        /// </summary>
        public const string FoundationaLLM_AzureAI_Projects_Delete = "FoundationaLLM.AzureAI/projects/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_AzureAI_Management_Write = "FoundationaLLM.AzureAI/management/write";

        #endregion

        #region AzureOpenAI

        /// <summary>
        /// Read Azure OpenAI conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_ConversationMappings_Read = "FoundationaLLM.AzureOpenAI/conversationMappings/read";

        /// <summary>
        /// Create or update Azure OpenAI conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_ConversationMappings_Write = "FoundationaLLM.AzureOpenAI/conversationMappings/write";

        /// <summary>
        /// Delete Azure OpenAI conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_ConversationMappings_Delete = "FoundationaLLM.AzureOpenAI/conversationMappings/delete";

        /// <summary>
        /// Read Azure OpenAI file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_FileMappings_Read = "FoundationaLLM.AzureOpenAI/fileMappings/read";

        /// <summary>
        /// Create or update Azure OpenAI file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_FileMappings_Write = "FoundationaLLM.AzureOpenAI/fileMappings/write";

        /// <summary>
        /// Delete Azure OpenAI file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_FileMappings_Delete = "FoundationaLLM.AzureOpenAI/fileMappings/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_Management_Write = "FoundationaLLM.AzureOpenAI/management/write";

        #endregion

        #region Configuration

        /// <summary>
        /// Read app configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurations_Read = "FoundationaLLM.Configuration/appConfigurations/read";

        /// <summary>
        /// Create or update app configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurations_Write = "FoundationaLLM.Configuration/appConfigurations/write";

        /// <summary>
        /// Delete app configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurations_Delete = "FoundationaLLM.Configuration/appConfigurations/delete";

        /// <summary>
        /// Read app configuration sets.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurationSets_Read = "FoundationaLLM.Configuration/appConfigurationSets/read";

        /// <summary>
        /// Read key vault secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultSecrets_Read = "FoundationaLLM.Configuration/keyVaultSecrets/read";

        /// <summary>
        /// Create or update key vault secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultSecrets_Write = "FoundationaLLM.Configuration/keyVaultSecrets/write";

        /// <summary>
        /// Delete key vault secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultSecrets_Delete = "FoundationaLLM.Configuration/keyVaultSecrets/delete";

        /// <summary>
        /// Read API endpoint configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpointConfigurations_Read = "FoundationaLLM.Configuration/apiEndpointConfigurations/read";

        /// <summary>
        /// Create or update API endpoint configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpointConfigurations_Write = "FoundationaLLM.Configuration/apiEndpointConfigurations/write";

        /// <summary>
        /// Delete API endpoint configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpoinConfigurations_Delete = "FoundationaLLM.Configuration/apiEndpointConfigurations/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Configuration_Management_Write = "FoundationaLLM.Configuration/management/write";

        #endregion

        #region DataSource

        /// <summary>
        /// Read data sources.
        /// </summary>
        public const string FoundationaLLM_DataSource_DataSources_Read = "FoundationaLLM.DataSource/dataSources/read";

        /// <summary>
        /// Create or update data sources.
        /// </summary>
        public const string FoundationaLLM_DataSource_DataSources_Write = "FoundationaLLM.DataSource/dataSources/write";

        /// <summary>
        /// Delete data sources.
        /// </summary>
        public const string FoundationaLLM_DataSource_DataSources_Delete = "FoundationaLLM.DataSource/dataSources/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_DataSource_Management_Write = "FoundationaLLM.DataSource/management/write";

        #endregion

        #region Prompt

        /// <summary>
        /// Read prompts.
        /// </summary>
        public const string FoundationaLLM_Prompt_Prompts_Read = "FoundationaLLM.Prompt/prompts/read";

        /// <summary>
        /// Create or update prompts.
        /// </summary>
        public const string FoundationaLLM_Prompt_Prompts_Write = "FoundationaLLM.Prompt/prompts/write";

        /// <summary>
        /// Delete prompts.
        /// </summary>
        public const string FoundationaLLM_Prompt_Prompts_Delete = "FoundationaLLM.Prompt/prompts/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Prompt_Management_Write = "FoundationaLLM.Prompt/management/write";

        #endregion

        #region Vectorization

        /// <summary>
        /// Read vectorization pipelines.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationPipelines_Read = "FoundationaLLM.Vectorization/vectorizationPipelines/read";

        /// <summary>
        /// Create or update vectorization pipelines.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationPipelines_Write = "FoundationaLLM.Vectorization/vectorizationPipelines/write";

        /// <summary>
        /// Delete vectorization pipelines.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationPipelines_Delete = "FoundationaLLM.Vectorization/vectorizationPipelines/delete";

        /// <summary>
        /// Read vectorization requests.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationRequests_Read = "FoundationaLLM.Vectorization/vectorizationRequests/read";

        /// <summary>
        /// Create or update vectorization requests.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationRequests_Write = "FoundationaLLM.Vectorization/vectorizationRequests/write";

        /// <summary>
        /// Delete vectorization requests.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationRequests_Delete = "FoundationaLLM.Vectorization/vectorizationRequests/delete";

        /// <summary>
        /// Read vectorization content source profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceProfiles_Read = "FoundationaLLM.Vectorization/contentSourceProfiles/read";

        /// <summary>
        /// Create or update vectorization content source profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceProfiles_Write = "FoundationaLLM.Vectorization/contentSourceProfiles/write";

        /// <summary>
        /// Delete vectorization content source profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceProfiles_Delete = "FoundationaLLM.Vectorization/contentSourceProfiles/delete";

        /// <summary>
        /// Read vectorization text partitioning profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextPartitioningProfiles_Read = "FoundationaLLM.Vectorization/textPartitioningProfiles/read";

        /// <summary>
        /// Create or update vectorization text partitioning profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextPartitioningProfiles_Write = "FoundationaLLM.Vectorization/textPartitioningProfiles/write";

        /// <summary>
        /// Delete vectorization text partitioning profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextPartitioningProfiles_Delete = "FoundationaLLM.Vectorization/textPartitioningProfiles/delete";

        /// <summary>
        /// Read vectorization text embedding profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbeddingProfiles_Read = "FoundationaLLM.Vectorization/textEmbeddingProfiles/read";

        /// <summary>
        /// Create or update vectorization text embedding profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbeddingProfiles_Write = "FoundationaLLM.Vectorization/textEmbeddingProfiles/write";

        /// <summary>
        /// Delete vectorization text embedding profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbeddingProfiles_Delete = "FoundationaLLM.Vectorization/textEmbeddingProfiles/delete";

        /// <summary>
        /// Read vectorization indexing profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_IndexingProfiles_Read = "FoundationaLLM.Vectorization/indexingProfiles/read";

        /// <summary>
        /// Create or update vectorization indexing profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_IndexingProfiles_Write = "FoundationaLLM.Vectorization/indexingProfiles/write";

        /// <summary>
        /// Delete vectorization indexing profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_IndexingProfiles_Delete = "FoundationaLLM.Vectorization/indexingProfiles/delete";

        #endregion

        #region Attachment

        /// <summary>
        /// Read attachments.
        /// </summary>
        public const string FoundationaLLM_Attachment_Attachments_Read = "FoundationaLLM.Attachment/attachments/read";

        /// <summary>
        /// Create or update attachments.
        /// </summary>
        public const string FoundationaLLM_Attachment_Attachments_Write = "FoundationaLLM.Attachment/attachments/write";

        /// <summary>
        /// Delete attachments.
        /// </summary>
        public const string FoundationaLLM_Attachment_Attachments_Delete = "FoundationaLLM.Attachment/attachments/delete";

        #endregion

        #region AIModel

        /// <summary>
        /// Read AI models
        /// </summary>
        public const string FoundationaLLM_AIModel_AIModels_Read = "FoundationaLLM.AIModel/aiModels/read";

        /// <summary>
        /// Create or update AI models.
        /// </summary>
        public const string FoundationaLLM_AIModel_AIModels_Write = "FoundationaLLM.AIModel/aiModels/write";

        /// <summary>
        /// Delete AI models.
        /// </summary>
        public const string FoundationaLLM_AIModel_AIModels_Delete = "FoundationaLLM.AIModel/aiModels/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_AIModel_Management_Write = "FoundationaLLM.AIModel/management/write";

        #endregion

        #region Conversation

        /// <summary>
        /// Read conversations
        /// </summary>
        public const string FoundationaLLM_Conversation_Conversations_Read = "FoundationaLLM.Conversation/conversations/read";

        /// <summary>
        /// Create or update conversations.
        /// </summary>
        public const string FoundationaLLM_Conversation_Conversations_Write = "FoundationaLLM.Conversation/conversations/write";

        /// <summary>
        /// Delete conversations.
        /// </summary>
        public const string FoundationaLLM_Conversation_Conversations_Delete = "FoundationaLLM.Conversation/conversations/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Conversation_Management_Write = "FoundationaLLM.Conversation/management/write";

        #endregion

        #region DataPipeline

        /// <summary>
        /// Read data pipelines.
        /// </summary>
        public const string FoundationaLLM_DataPipeline_DataPipelines_Read = "FoundationaLLM.DataPipeline/dataPipelines/read";

        /// <summary>
        /// Create or update data pipelines.
        /// </summary>
        public const string FoundationaLLM_DataPipeline_DataPipelines_Write = "FoundationaLLM.DataPipeline/dataPipelines/write";

        /// <summary>
        /// Delete data pipelines.
        /// </summary>
        public const string FoundationaLLM_DataPipeline_DataPipelines_Delete = "FoundationaLLM.DataPipeline/dataPipelines/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_DataPipeline_Management_Write = "FoundationaLLM.DataPipeline/management/write";

        #endregion

        #region Plugin

        /// <summary>
        /// Read plugins.
        /// </summary>
        public const string FoundationaLLM_Plugin_Plugins_Read = "FoundationaLLM.Plugin/plugins/read";

        /// <summary>
        /// Create or update plugins.
        /// </summary>
        public const string FoundationaLLM_Plugin_Plugins_Write = "FoundationaLLM.Plugin/plugins/write";

        /// <summary>
        /// Delete plugins.
        /// </summary>
        public const string FoundationaLLM_Plugin_Plugins_Delete = "FoundationaLLM.Plugin/plugins/delete";

        /// <summary>
        /// Read plugin packages.
        /// </summary>
        public const string FoundationaLLM_Plugin_PluginPackages_Read = "FoundationaLLM.Plugin/pluginPackages/read";

        /// <summary>
        /// Create or update plugin packages.
        /// </summary>
        public const string FoundationaLLM_Plugin_PluginPackages_Write = "FoundationaLLM.Plugin/pluginPackages/write";

        /// <summary>
        /// Delete plugin packages.
        /// </summary>
        public const string FoundationaLLM_Plugin_PluginPackages_Delete = "FoundationaLLM.Plugin/pluginPackages/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Plugin_Management_Write = "FoundationaLLM.Plugin/management/write";

        #endregion

        #region Vector

        /// <summary>
        /// Read vector databases.
        /// </summary>
        public const string FoundationaLLM_Vector_VectorDatabases_Read = "FoundationaLLM.Vector/vectorDatabases/read";

        /// <summary>
        /// Create or update vector databases.
        /// </summary>
        public const string FoundationaLLM_Vector_VectorDatabases_Write = "FoundationaLLM.Vector/vectorDatabases/write";

        /// <summary>
        /// Delete vector databases.
        /// </summary>
        public const string FoundationaLLM_Vector_VectorDatabases_Delete = "FoundationaLLM.Vector/vectorDatabases/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Vector_Management_Write = "FoundationaLLM.Vector/management/write";

        #endregion

        #region Context

        /// <summary>
        /// Read context knowledge sources.
        /// </summary>
        public const string FoundationaLLM_Context_KnowledgeSources_Read = "FoundationaLLM.Context/knowledgeSources/read";

        /// <summary>
        /// Create or update context knowledge sources.
        /// </summary>
        public const string FoundationaLLM_Context_KnowledgeSources_Write = "FoundationaLLM.Context/knowledgeSources/write";

        /// <summary>
        /// Delete context knowledge sources.
        /// </summary>
        public const string FoundationaLLM_Context_KnowledgeSources_Delete = "FoundationaLLM.Context/knowledgeSources/delete";

        /// <summary>
        /// Read context knowledge units.
        /// </summary>
        public const string FoundationaLLM_Context_KnowledgeUnits_Read = "FoundationaLLM.Context/knowledgeUnits/read";

        /// <summary>
        /// Create or update context knowledge units.
        /// </summary>
        public const string FoundationaLLM_Context_KnowledgeUnits_Write = "FoundationaLLM.Context/knowledgeUnits/write";

        /// <summary>
        /// Delete context knowledge units.
        /// </summary>
        public const string FoundationaLLM_Context_KnowledgeUnits_Delete = "FoundationaLLM.Context/knowledgeUnits/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Context_Management_Write = "FoundationaLLM.Context/management/write";

        #endregion

        #region Infrastructure

        /// <summary>
        /// Read Azure Container Apps Environments.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureContainerAppsEnvironments_Read = "FoundationaLLM.Infrastructure/azureContainerAppsEnvironments/read";

        /// <summary>
        /// Read Azure Container Apps.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureContainerApps_Read = "FoundationaLLM.Infrastructure/azureContainerApps/read";

        /// <summary>
        /// Create or update Azure Container Apps.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureContainerApps_Write = "FoundationaLLM.Infrastructure/azureContainerApps/write";

        /// <summary>
        /// Delete Azure Container Apps.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureContainerApps_Delete = "FoundationaLLM.Infrastructure/azureContainerApps/delete";

        /// <summary>
        /// Read Azure Kubernetes Services.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureKubernetesServices_Read = "FoundationaLLM.Infrastructure/azureKubernetesServices/read";

        /// <summary>
        /// Read Azure Kubernetes Service Deployments.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureKubernetesServiceDeployments_Read = "FoundationaLLM.Infrastructure/azureKubernetesServiceDeployments/read";

        /// <summary>
        /// Create or update Azure Kubernetes Service Deployments.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureKubernetesServiceDeployments_Write = "FoundationaLLM.Infrastructure/azureKubernetesServiceDeployments/write";

        /// <summary>
        /// Delete Azure Kubernetes Service Deployments.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_AzureKubernetesServiceDeployments_Delete = "FoundationaLLM.Infrastructure/azureKubernetesServiceDeployments/delete";

        /// <summary>
        /// Execute management actions.
        /// </summary>
        public const string FoundationaLLM_Infrastructure_Management_Write = "FoundationaLLM.Infrastructure/management/write";

        #endregion
    }
}
