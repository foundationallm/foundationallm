using FoundationaLLM.Common.Models.Configuration.KeyVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Configuration.Catalog
{
    /// <summary>
    /// A catalog of Key Vault secrets used in this solution.
    /// </summary>
    public static class KeyVaultSecretsCatalog
    {
        /// <summary>
        /// The list of Key Vault secret entries.
        /// </summary>
        public static readonly List<KeyVaultSecretEntry> Entries =
        [
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_AgentHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_Agent_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.3.0",
                description: "The connection string to the Azure Storage account used for the agent resource provider."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_AgentFactoryAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_AgentHubAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_DataSourceHubAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_GatekeeperAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_LangChainAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_Prompt_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.3.0",
                description: "The connection string to the Azure Storage account used for the prompt resource provider."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_PromptHubAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_SemanticKernelAPI_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_AzureContentSafety_APIKey,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_AzureOpenAI_Api_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_BlobStorageMemorySource_Blobstorageconnection,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_CognitiveSearch_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_CognitiveSearchMemorySource_Blobstorageconnection,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_CognitiveSearchMemorySource_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_CosmosDB_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_DataSourceHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_DataSourceHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_DurableSystemPrompt_BlobStorageConnection,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_OpenAI_Api_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_PromptHub_StorageManager_BlobStorage_ConnectionString,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_SemanticKernelAPI_OpenAI_Key,
                minimumVersion: "0.3.0",
                description: ""
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_VectorizationAPI_APIKey,
                minimumVersion: "0.3.0",
                description: "The API key of the vectorization API."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description: "The connection string to the Application Insights instance used by the vectorization API."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_APIs_VectorizationWorker_APIKey,
                minimumVersion: "0.3.0",
                description: "The API key of the vectorization worker API."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_App_Insights_Connection_String,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Application Insights instance used by the vectorization worker API."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the embed vectorization queue."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the extract vectorization queue."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the index vectorization queue."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_Vectorization_Queues_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the partition vectorization queue."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_Vectorization_State_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the vectorization state service."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_Vectorization_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.3.0",
                description:
                "The connection string to the Azure Storage account used for the vectorization state service."
            ),
            new (
                secretName: Common.Constants.KeyVaultSecretNames.FoundationaLLM_Events_AzureEventGrid_APIKey,
                minimumVersion: "0.4.0",
                description:
                "The API key for the Azure Event Grid service."
            ),
            new(
                secretName: Common.Constants.KeyVaultSecretNames
                    .FoundationaLLM_DataSource_ResourceProvider_Storage_ConnectionString,
                minimumVersion: "0.5.0",
                description: "The connection string to the Azure Storage account used for the data source resource provider."
            ),
        ];

        /// <summary>
        /// Returns the list of all the Key Vault secrets for this solution that are required for the given version.
        /// </summary>
        /// <param name="version">The current version of the caller.</param>
        /// <returns></returns>
        public static IEnumerable<KeyVaultSecretEntry> GetRequiredKeyVaultSecretsForVersion(string version)
        {
            // Extract the numeric part of the version, ignoring pre-release tags.
            var numericVersionPart = version.Split('-')[0];
            if (!Version.TryParse(numericVersionPart, out var currentVersion))
            {
                throw new ArgumentException($"Invalid version format for the provided version ({version}).", nameof(version));
            }

            // Compare based on the Major, Minor, and Build numbers only.
            return Entries.Where(entry =>
            {
                if (string.IsNullOrWhiteSpace(entry.MinimumVersion))
                {
                    return false;
                }

                var entryNumericVersionPart = entry.MinimumVersion.Split('-')[0];
                if (!Version.TryParse(entryNumericVersionPart, out var entryVersion))
                {
                    return false;
                }

                var entryVersionWithoutRevision = new Version(entryVersion.Major, entryVersion.Minor, entryVersion.Build);
                var currentVersionWithoutRevision = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                return entryVersionWithoutRevision <= currentVersionWithoutRevision;
            });
        }
    }

}
