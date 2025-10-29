// -------------------------------------------------------------------------------
//
// WARNING!
// This file is auto-generated based on the AppConfiguration.json file.
// Do not make changes to this file, as they will be automatically overwritten.
//
// -------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Defines methods to select configuration options.
    /// </summary>
    public static class ConfigurationOptions
    {
        /// <summary>
        /// Selects configuration options for ManagementAPI.
        /// </summary>
        public static void SelectForManagementAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProvidersCache);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_Entra);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_ManagementAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_VectorizationAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatewayAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_DataPipelineAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_ContextAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AzureAI_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Prompt_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_DataSource_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Attachment_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AIModel_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_DataPipeline_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Plugin_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Vector_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Context_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Vectorization_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
            options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_ManagementAPI);
        }
    }
}
