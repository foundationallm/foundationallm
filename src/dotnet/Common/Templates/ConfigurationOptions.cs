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
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
            options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_ManagementAPI);
        }
        /// <summary>
        /// Selects configuration options for ManagementPortal.
        /// </summary>
        public static void SelectForManagementPortal(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeys.FoundationaLLM_Instance_Id);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
            options.Select(AppConfigurationKeys.FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_AllowedUploadFileExtensions);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ManagementPortal_Authentication_Entra);
            options.Select(AppConfigurationKeys.FeatureFlag_FoundationaLLM_Agent_PrivateStore);
        }
        /// <summary>
        /// Selects configuration options for CoreAPI.
        /// </summary>
        public static void SelectForCoreAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProvidersCache);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_UserPortal_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_OrchestrationAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatewayAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatekeeperAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_ContextAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_DataPipelineAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Prompt_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Attachment_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AIModel_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AzureAI_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_DataPipeline_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Quota_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
            options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_CoreAPI);
            options.Select(AppConfigurationKeys.FeatureFlag_FoundationaLLM_Agent_SelfService);
        }
        /// <summary>
        /// Selects configuration options for CoreWorker.
        /// </summary>
        public static void SelectForCoreWorker(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreWorker_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB);
        }
        /// <summary>
        /// Selects configuration options for UserPortal.
        /// </summary>
        public static void SelectForUserPortal(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeys.FoundationaLLM_Instance_Id);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
            options.Select(AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_UserPortal_Authentication_Entra);
            options.Select(AppConfigurationKeys.FeatureFlag_FoundationaLLM_Agent_SelfService);
        }
        /// <summary>
        /// Selects configuration options for StateAPI.
        /// </summary>
        public static void SelectForStateAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_StateAPI_Essentials);
        }
        /// <summary>
        /// Selects configuration options for GatewayAPI.
        /// </summary>
        public static void SelectForGatewayAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProvidersCache);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatewayAPI_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatewayAPI_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Prompt_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Attachment_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
            options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_GatewayAPI);
        }
        /// <summary>
        /// Selects configuration options for OrchestrationAPI.
        /// </summary>
        public static void SelectForOrchestrationAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProvidersCache);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AIModel_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Attachment_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AzureAI_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Context_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_DataSource_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_DataPipeline_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Prompt_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Vector_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
            options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_OrchestrationAPI);
        }
        /// <summary>
        /// Selects configuration options for ContextAPI.
        /// </summary>
        public static void SelectForContextAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProvidersCache);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Code_CodeExecution_AzureContainerAppsDynamicSessions);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Context_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Prompt_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Vector_Storage);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
            options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_ContextAPI);
        }
        /// <summary>
        /// Selects configuration options for LangChainAPI.
        /// </summary>
        public static void SelectForLangChainAPI(
            AzureAppConfigurationOptions options)
        {
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_PythonSDK);
        }
    }
}
