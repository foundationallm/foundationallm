using Azure.Identity;
using BackupTool.Interfaces;
using BackupTool.Models.Configuration;
using BackupTool.Services;
using FoundationaLLM;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM.Backup.Tool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            DefaultAuthentication.Initialize(
                builder.Environment.IsProduction(),
                ServiceNames.CoreAPI);

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration((Action<Microsoft.Extensions.Configuration.AzureAppConfiguration.AzureAppConfigurationOptions>)(options =>
            {
                options.Connect(builder.Configuration[FoundationaLLM.Common.Constants.Configuration.EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(DefaultAuthentication.AzureCredential);
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_CoreAPI_Configuration);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_OrchestrationAPI_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatewayAPI_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_GatekeeperAPI_Essentials);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Attachment_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AIModel_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AzureOpenAI_Storage);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
                options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_CoreAPI);

            }));
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            builder.AddGroupMembership();

            builder.Services.AddInstanceProperties(builder.Configuration);

            builder.Services.AddOptions<ClientBrandingConfiguration>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Branding));
            builder.Services.AddOptions<BackupServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_CoreAPI_Configuration));

            builder.Services.AddAzureResourceManager();

            //builder.Services.AddHttpContextAccessor();

            //builder.Services.AddAzureEventGridEvents(
            //    builder.Configuration,
            //    AppConfigurationKeySections.FoundationaLLM_Events_Profiles_CoreAPI);

            // Add resource providers
            //builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();
            //builder.AddAgentResourceProvider();
            //builder.AddAttachmentResourceProvider();
            //builder.AddConfigurationResourceProvider();
            //builder.AddAzureOpenAIResourceProvider();
            //builder.AddAIModelResourceProvider();
            //builder.AddConversationResourceProvider();

            // Register the downstream services and HTTP clients.
            //builder.AddHttpClientFactoryService();
            //builder.AddDownstreamAPIService(HttpClientNames.GatekeeperAPI);
            //builder.AddDownstreamAPIService(HttpClientNames.OrchestrationAPI);
            //builder.AddAuthorizationServiceClient();

            builder.AddAzureCosmosDBService();
            builder.Services.AddScoped<IBackupService, BackupService>();
            //builder.Services.AddScoped<ICallContext, CallContext>();

            builder.AddOpenTelemetry(
                FoundationaLLM.Common.Constants.Configuration.AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_AppInsightsConnectionString,
                ServiceNames.CoreAPI
            );

            builder.Services.AddHostedService<BackupServiceHostedService>();

            IHost host = builder.Build();

            host.Run();
        }
    }
}
