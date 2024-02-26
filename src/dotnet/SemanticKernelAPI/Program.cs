using Asp.Versioning;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Plugins;
using Microsoft.Extensions.Options;
//using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.SemanticKernel.API
{
    /// <summary>
    /// Program class for the Semantic Kernel API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the Semantic Kernel API.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(new DefaultAzureCredential());
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_DurableSystemPrompt);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CognitiveSearchMemorySource);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_BlobStorageMemorySource);
                options.Select(AppConfigurationKeys.FoundationaLLM_AzureOpenAI_API_Key);
                options.Select(AppConfigurationKeys.FoundationaLLM_AzureOpenAI_API_Endpoint);
                options.Select(AppConfigurationKeys.FoundationaLLM_AzureOpenAI_API_Version);
                options.Select(AppConfigurationKeys.FoundationaLLM_AzureOpenAI_API_Completions_DeploymentName);
                options.Select(AppConfigurationKeys.FoundationaLLM_AzureOpenAI_API_Completions_ModelVersion);
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add services to the container.
            //builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services
                .AddApiVersioning(options =>
                {
                    // Reporting api versions will return the headers
                    // "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(new DateOnly(2024, 2, 16));
                })
                .AddMvc()
                .AddApiExplorer();

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_SemanticKernelAPI));
            builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(
                    options =>
                    {
                        // Add a custom operation filter which sets default values
                        options.OperationFilter<SwaggerDefaultValues>();

                        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
                        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                        // Integrate xml comments
                        options.IncludeXmlComments(filePath);

                        // Adds auth via X-API-KEY header
                        options.AddAPIKeyAuth();
                    })
                .AddSwaggerGenNewtonsoftSupport();

            //builder.Services.AddOptions<SemanticKernelServiceSettings>()
            //    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_SemanticKernelAPI));
            //builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();

            builder.Services.AddScoped<IKnowledgeManagementAgentPlugin, KnowledgeManagementAgentPlugin>();
            builder.Services.AddScoped<ILegacyAgentPlugin, LegacyAgentPlugin>();

            // Simple, static system prompt service
            //builder.Services.AddSingleton<ISystemPromptService, InMemorySystemPromptService>();

            // Add the OpenTelemetry telemetry service and send telemetry data to Azure Monitor.
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_SemanticKernelAPI_AppInsightsConnectionString];
            });

            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                { "service.name", "SemanticKernelAPI" },
                { "service.namespace", "FoundationaLLM" },
                { "service.instance.id", Guid.NewGuid().ToString() }
            };

            // Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
            builder.Services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
                builder.ConfigureResource(resourceBuilder =>
                    resourceBuilder.AddAttributes(resourceAttributes)));

            // System prompt service backed by an Azure blob storage account
            //builder.Services.AddOptions<DurableSystemPromptServiceSettings>()
            //    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_DurableSystemPrompt));
            //builder.Services.AddSingleton<ISystemPromptService, DurableSystemPromptService>();

            //builder.Services.AddOptions<AzureCognitiveSearchMemorySourceSettings>()
            //    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CognitiveSearchMemorySource));
            //builder.Services.AddTransient<IMemorySource, AzureCognitiveSearchMemorySource>();

            //builder.Services.AddOptions<BlobStorageMemorySourceSettings>()
            //    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_BlobStorageMemorySource));
            //builder.Services.AddTransient<IMemorySource, BlobStorageMemorySource>();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger(p => p.SerializeAsV2 = true);
            app.UseSwaggerUI(
                options =>
                {
                    var descriptions = app.DescribeApiVersions();

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in descriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, name);
                    }

                    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { "resource", builder.Configuration[AppConfigurationKeys.FoundationaLLM_Management_Entra_ClientId] } });
                });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
