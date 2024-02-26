using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Contains constants for the names of all Azure Key Vault secrets.
    /// </summary>
    public static class KeyVaultSecretNames
    {
        /// <summary>
        /// The foundationallm-agenthub-storagemanager-blobstorage-connectionstring Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_AgentHub_StorageManager_BlobStorage_ConnectionString = "foundationallm-agenthub-storagemanager-blobstorage-connectionstring";
        /// <summary>
        /// The foundationallm-agent-resourceprovider-storage-connectionstring Key Vault secret name.
        /// The connection string to the Azure Storage account used for the agent resource provider.
        /// </summary>
        public const string FoundationaLLM_Agent_ResourceProvider_Storage_ConnectionString = "foundationallm-agent-resourceprovider-storage-connectionstring";
        /// <summary>
        /// The foundationallm-apis-agentfactoryapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentFactoryAPI_APIKey = "foundationallm-apis-agentfactoryapi-apikey";
        /// <summary>
        /// The foundationallm-app-insights-connection-string Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_App_Insights_Connection_String = "foundationallm-app-insights-connection-string";
        /// <summary>
        /// The foundationallm-apis-agenthubapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentHubAPI_APIKey = "foundationallm-apis-agenthubapi-apikey";
        /// <summary>
        /// The foundationallm-apis-datasourcehubapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_DataSourceHubAPI_APIKey = "foundationallm-apis-datasourcehubapi-apikey";
        /// <summary>
        /// The foundationallm-apis-gatekeeperapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_APIKey = "foundationallm-apis-gatekeeperapi-apikey";
        /// <summary>
        /// The foundationallm-apis-gatekeeperintegrationapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIKey = "foundationallm-apis-gatekeeperintegrationapi-apikey";
        /// <summary>
        /// The foundationallm-apis-langchainapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_LangChainAPI_APIKey = "foundationallm-apis-langchainapi-apikey";
        /// <summary>
        /// The foundationallm-prompt-resourceprovider-storage-connectionstring Key Vault secret name.
        /// The connection string to the Azure Storage account used for the prompt resource provider.
        /// </summary>
        public const string FoundationaLLM_Prompt_ResourceProvider_Storage_ConnectionString = "foundationallm-prompt-resourceprovider-storage-connectionstring";
        /// <summary>
        /// The foundationallm-apis-prompthubapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_PromptHubAPI_APIKey = "foundationallm-apis-prompthubapi-apikey";
        /// <summary>
        /// The foundationallm-apis-semantickernelapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_SemanticKernelAPI_APIKey = "foundationallm-apis-semantickernelapi-apikey";
        /// <summary>
        /// The foundationallm-azurecontentsafety-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_APIKey = "foundationallm-azurecontentsafety-apikey";
        /// <summary>
        /// The foundationallm-azureopenai-api-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_Api_Key = "foundationallm-azureopenai-api-key";
        /// <summary>
        /// The foundationallm-blobstoragememorysource-blobstorageconnection Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource_Blobstorageconnection = "foundationallm-blobstoragememorysource-blobstorageconnection";
        /// <summary>
        /// The foundationallm-chat-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_ClientSecret = "foundationallm-chat-entra-clientsecret";
        /// <summary>
        /// The foundationallm-cognitivesearch-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearch_Key = "foundationallm-cognitivesearch-key";
        /// <summary>
        /// The foundationallm-cognitivesearchmemorysource-blobstorageconnection Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_Blobstorageconnection = "foundationallm-cognitivesearchmemorysource-blobstorageconnection";
        /// <summary>
        /// The foundationallm-cognitivesearchmemorysource-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_Key = "foundationallm-cognitivesearchmemorysource-key";
        /// <summary>
        /// The foundationallm-coreapi-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Coreapi_Entra_ClientSecret = "foundationallm-coreapi-entra-clientsecret";
        /// <summary>
        /// The foundationallm-cosmosdb-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_Key = "foundationallm-cosmosdb-key";
        /// <summary>
        /// The foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_DataSourceHub_StorageManager_BlobStorage_ConnectionString = "foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring";
        /// <summary>
        /// The foundationallm-durablesystemprompt-blobstorageconnection Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_DurableSystemPrompt_BlobStorageConnection = "foundationallm-durablesystemprompt-blobstorageconnection";
        /// <summary>
        /// The foundationallm-langchain-csvfile-url Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_LangChain_CsvFile_Url = "foundationallm-langchain-csvfile-url";
        /// <summary>
        /// The foundationallm-langchain-sqldatabase-testdb-password Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_LangChain_SQLDatabase_Testdb_Password = "foundationallm-langchain-sqldatabase-testdb-password";
        /// <summary>
        /// The foundationallm-management-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_ClientSecret = "foundationallm-management-entra-clientsecret";
        /// <summary>
        /// The foundationallm-managementapi-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Managementapi_Entra_ClientSecret = "foundationallm-managementapi-entra-clientsecret";
        /// <summary>
        /// The foundationallm-openai-api-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_OpenAI_Api_Key = "foundationallm-openai-api-key";
        /// <summary>
        /// The foundationallm-prompthub-storagemanager-blobstorage-connectionstring Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_PromptHub_StorageManager_BlobStorage_ConnectionString = "foundationallm-prompthub-storagemanager-blobstorage-connectionstring";
        /// <summary>
        /// The foundationallm-semantickernelapi-openai-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_Key = "foundationallm-semantickernelapi-openai-key";
        /// <summary>
        /// The foundationallm-apis-vectorizationapi-apikey Key Vault secret name.
        /// The API key of the vectorization API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI_APIKey = "foundationallm-apis-vectorizationapi-apikey";
        /// <summary>
        /// The foundationallm-apis-vectorizationworker-apikey Key Vault secret name.
        /// The API key of the vectorization worker API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker_APIKey = "foundationallm-apis-vectorizationworker-apikey";
        /// <summary>
        /// The foundationallm-vectorization-queues-connectionstring Key Vault secret name.
        /// The connection string to the Azure Storage account used for the embed vectorization queue.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_ConnectionString = "foundationallm-vectorization-queues-connectionstring";
        /// <summary>
        /// The foundationallm-vectorization-state-connectionstring Key Vault secret name.
        /// The connection string to the Azure Storage account used for the vectorization state service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_State_ConnectionString = "foundationallm-vectorization-state-connectionstring";
        /// <summary>
        /// The foundationallm-vectorization-resourceprovider-storage-connectionstring Key Vault secret name.
        /// The connection string to the Azure Storage account used for the vectorization state service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ResourceProvider_Storage_ConnectionString = "foundationallm-vectorization-resourceprovider-storage-connectionstring";
        /// <summary>
        /// The foundationallm-events-azureeventgrid-apikey Key Vault secret name.
        /// The API key for the Azure Event Grid service.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGrid_APIKey = "foundationallm-events-azureeventgrid-apikey";
    }
}
