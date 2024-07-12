using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Core.API
{
    /// <summary>
    /// Main entry point for the Core API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Core API service configuration.
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DefaultAuthentication.Initialize(
                builder.Environment.IsProduction(),
                ServiceNames.CoreAPI);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(DefaultAuthentication.AzureCredential);
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CosmosDB);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CoreAPI_Entra);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Agent);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Events);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Attachment);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add authorization services.
            builder.AddGroupMembership();
            builder.AddAuthorizationService();

            // CORS policies
            builder.AddCorsPolicies();

            builder.Services.AddInstanceProperties(builder.Configuration);

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));
            builder.Services.AddOptions<ClientBrandingConfiguration>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Branding));
            builder.Services.AddOptions<CoreServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_CoreAPI));

            // Add Azure ARM services
            builder.Services.AddAzureResourceManager();

            // Add event services
            builder.Services.AddAzureEventGridEvents(
                builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService_Profiles_CoreAPI);

            // Add resource providers
            builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();
            builder.AddAgentResourceProvider();
            builder.AddAttachmentResourceProvider();
            builder.AddConfigurationResourceProvider();

            // Register the downstream services and HTTP clients.
            builder.AddHttpClientFactoryService();
            RegisterDownstreamServices(builder);

            builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
                return new CosmosClientBuilder(settings.Endpoint, DefaultAuthentication.AzureCredential)
                    .WithSerializerOptions(new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    })
                    .WithConnectionModeGateway()
                    .Build();
            });

            builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
            builder.Services.AddScoped<ICoreService, CoreService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<ICallContext, CallContext>();

            // Add authentication configuration.
            var e2ETestEnvironmentValue = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Environment) ?? string.Empty;
            var isE2ETestEnvironment = e2ETestEnvironmentValue.Equals(EnvironmentTypes.E2ETest, StringComparison.CurrentCultureIgnoreCase);
            builder.AddAuthenticationConfiguration(
                AppConfigurationKeys.FoundationaLLM_CoreAPI_Entra_Instance,
                AppConfigurationKeys.FoundationaLLM_CoreAPI_Entra_TenantId,
                AppConfigurationKeys.FoundationaLLM_CoreAPI_Entra_ClientId,
                AppConfigurationKeys.FoundationaLLM_CoreAPI_Entra_Scopes,
                requireScopes: !isE2ETestEnvironment,
                allowACLAuthorization: isE2ETestEnvironment
            );

            // Add OpenTelemetry.
            builder.AddOpenTelemetry(
                AppConfigurationKeys.FoundationaLLM_APIs_CoreAPI_AppInsightsConnectionString,
                ServiceNames.CoreAPI);

            builder.Services.AddControllers();

            builder.Services.AddProblemDetails();
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

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "azure_auth",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            [ "user_impersonation" ]
                        }
                    });

                    options.AddSecurityDefinition("azure_auth", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Azure Active Directory Oauth2 Flow",
                        Name = "azure_auth",
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://login.microsoftonline.com/common/oauth2/authorize"),
                                Scopes = new Dictionary<string, string>
                                {
                                    {
                                        "user_impersonation",
                                        "impersonate your user account"
                                    }
                                }
                            }
                        },
                        BearerFormat = "JWT",
                        Scheme = "bearer"
                    });
                })
                .AddSwaggerGenNewtonsoftSupport();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            // Set the CORS policy before other middleware.
            app.UseCors(CorsPolicyNames.AllowAllOrigins);

            // For the CoreAPI, we need to make sure that UseAuthentication is called before the UserIdentityMiddleware.
            app.UseAuthentication();
            app.UseAuthorization();

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();

            app.UseExceptionHandler(exceptionHandlerApp
                    => exceptionHandlerApp.Run(async context
                        => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
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

                    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { "resource", builder.Configuration[AppConfigurationKeys.FoundationaLLM_CoreAPI_Entra_ClientId]! } });
                });

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Bind the downstream API settings to the configuration and register the HTTP clients.
        /// The AddResilienceHandler extension method is used to add the standard Polly resilience
        /// strategies to the HTTP clients.
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterDownstreamServices(WebApplicationBuilder builder)
        {
            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = []
            };

            var gatekeeperAPISettings = new DownstreamAPIClientConfiguration
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_GatekeeperAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_GatekeeperAPI_APIKey]!,
                Timeout = TimeSpan.FromMinutes(40)
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.GatekeeperAPI] = gatekeeperAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.GatekeeperAPI,
                        client => {
                            client.BaseAddress = new Uri(gatekeeperAPISettings.APIUrl);
                            client.Timeout = gatekeeperAPISettings.Timeout.Value;
                        })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        static strategyBuilder =>
                        {
                            CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                        });

            var orchestrationAPISettings = new DownstreamAPIClientConfiguration
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_OrchestrationAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_OrchestrationAPI_APIKey]!,
                Timeout = TimeSpan.FromMinutes(35)
            };

            downstreamAPISettings.DownstreamAPIs[HttpClients.OrchestrationAPI] = orchestrationAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.OrchestrationAPI,
                        client => {
                            client.BaseAddress = new Uri(orchestrationAPISettings.APIUrl);
                            client.Timeout = orchestrationAPISettings.Timeout.Value;
                        })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        static strategyBuilder =>
                        {
                            CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                        });

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);

            builder.Services.AddScoped<IDownstreamAPIService, DownstreamAPIService>((serviceProvider)
                => new DownstreamAPIService(HttpClients.GatekeeperAPI, serviceProvider.GetService<ICallContext>()!, serviceProvider.GetService<IHttpClientFactoryService>()!));
            builder.Services.AddScoped<IDownstreamAPIService, DownstreamAPIService>((serviceProvider)
                => new DownstreamAPIService(HttpClients.OrchestrationAPI, serviceProvider.GetService<ICallContext>()!, serviceProvider.GetService<IHttpClientFactoryService>()!));
        }
    }
}
