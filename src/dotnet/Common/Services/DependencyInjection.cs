using Azure.Monitor.OpenTelemetry.AspNetCore;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Configuration;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds CORS policies the the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddCorsPolicies(this IHostApplicationBuilder builder) =>
            builder.Services.AddCors(policyBuilder =>
                {
                    policyBuilder.AddPolicy(CorsPolicyNames.AllowAllOrigins,
                        policy =>
                        {
                            policy.AllowAnyOrigin();
                            policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since",
                                "Cache-Control", "Content-Type", "Range", "Authorization", "X-User-Identity",
                                "Access-Control-Request-Headers");
                            policy.AllowAnyMethod();
                        });
                });

        /// <summary>
        /// Adds OpenTelemetry the the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="connectionStringConfigurationKey">The configuration key for the OpenTelemetry connection string.</param>
        /// <param name="serviceName">The name of the service.</param>
        public static void AddOpenTelemetry(this IHostApplicationBuilder builder,
            string connectionStringConfigurationKey,
            string serviceName)
        {
            // Add the OpenTelemetry telemetry service and send telemetry data to Azure Monitor.
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = builder.Configuration[connectionStringConfigurationKey];
            });

            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                { "service.name", serviceName },
                { "service.namespace", "FoundationaLLM" },
                { "service.instance.id", ValidatedEnvironment.MachineName }
            };

            // Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
            builder.Services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
                builder.ConfigureResource(resourceBuilder =>
                    resourceBuilder.AddAttributes(resourceAttributes)));
        }

        /// <summary>
        /// Adds authentication configuration to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="entraInstanceConfigurationKey">The configuration key for the Entra ID instance.</param>
        /// <param name="entraTenantIdConfigurationKey">The configuration key for the Entra ID tenant id.</param>
        /// <param name="entraClientIdConfigurationkey">The configuration key for the Entra ID client id.</param>
        /// <param name="entraScopesConfigurationKey">The configuration key for the Entra ID scopes.</param>
        /// <param name="policyName">The name of the authorization policy.</param>
        /// <param name="requireScopes">Indicates whether a scope claim (scp) is required for authorization.</param>
        /// <param name="allowACLAuthorization">Indicates whether tokens that do not have either of the "scp" or "roles" claims are accepted (True means they are accepted).</param>
        public static void AddAuthenticationConfiguration(this IHostApplicationBuilder builder,
            string entraInstanceConfigurationKey,
            string entraTenantIdConfigurationKey,
            string entraClientIdConfigurationkey,
            string? entraScopesConfigurationKey,
            string policyName = "DefaultPolicy",
            bool requireScopes = true,
            bool allowACLAuthorization = false)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(jwtOptions => { },
                    identityOptions =>
                    {
                        identityOptions.Instance = builder.Configuration[entraInstanceConfigurationKey] ?? "";
                        identityOptions.TenantId = builder.Configuration[entraTenantIdConfigurationKey];
                        identityOptions.ClientId = builder.Configuration[entraClientIdConfigurationkey];
                        identityOptions.AllowWebApiToBeAuthorizedByACL = allowACLAuthorization;
                    });

            builder.Services.AddScoped<IUserClaimsProviderService, EntraUserClaimsProviderService>();

            // Configure the policy used by the API controllers:
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(policyName, policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    if (requireScopes)
                    {
                        var requiredScopes = builder.Configuration[entraScopesConfigurationKey]?.Split(' ');
                        if (requiredScopes != null && requiredScopes.Length > 0)
                        {
                            policyBuilder.RequireClaim(ClaimConstants.Scope, requiredScopes);
                        }
                        else
                        {
                            throw new InvalidOperationException("Scopes are required but no valid scopes are configured.");
                        }
                    }
                });
            });
        }

        /// <summary>
        /// Registers the <see cref="AzureKeyVaultService"/> with the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        /// <param name="keyVaultUriConfigurationKeyName">The name of the configuration key that provides the URI of the Azure Key Vault service.</param>
        public static void AddAzureKeyVaultService(this IHostApplicationBuilder builder,
            string keyVaultUriConfigurationKeyName)
        {
            builder.Services.AddAzureClients(clientBuilder =>
            {
                var keyVaultUri = builder.Configuration[keyVaultUriConfigurationKeyName];
                clientBuilder.AddSecretClient(new Uri(keyVaultUri!))
                    .WithCredential(DefaultAuthentication.AzureCredential);
            });

            // Configure logging to filter out Azure Core and Azure Key Vault informational logs.
            builder.Logging.AddFilter("Azure.Core", LogLevel.Warning);
            builder.Logging.AddFilter("Azure.Security.KeyVault.Secrets", LogLevel.Warning);
            builder.Services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService>();
        }

        /// <summary>
        /// Registers the <see cref="HttpClientFactoryService"/> with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddHttpClientFactoryService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();
            builder.Services.ActivateSingleton<IHttpClientFactoryService>();
        }

        /// <summary>
        /// Registers the <see cref="HttpClientFactoryService"/> with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddHttpClientFactoryService(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();
            services.ActivateSingleton<IHttpClientFactoryService>();
        }

        /// <summary>
        /// Registers the <see cref="IDownstreamAPIService"/> implementation for a named API service with the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        /// <param name="apiServiceName">The name of the API service whose implementation is being registered.</param>
        public static void AddDownstreamAPIService(this IHostApplicationBuilder builder, string apiServiceName) =>
            builder.Services.AddScoped<IDownstreamAPIService, DownstreamAPIService>(
                (serviceProvider) => new DownstreamAPIService(
                    apiServiceName,
                    serviceProvider.GetService<ICallContext>()!,
                    serviceProvider.GetService<IHttpClientFactoryService>()!,
                    serviceProvider.GetService<ILogger<DownstreamAPIService>>()!
                ));

        /// <summary>
        /// Registers the <see cref="IAzureResourceManagerService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddAzureResourceManager(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddSingleton<IAzureResourceManagerService, AzureResourceManagerService>();

        /// <summary>
        /// Registers the <see cref="IAzureResourceManagerService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddAzureResourceManager(
            this IServiceCollection services) =>
            services.AddSingleton<IAzureResourceManagerService, AzureResourceManagerService>();

        /// <summary>
        /// Registers the <see cref="ICosmosDBService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddAzureCosmosDBService(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureCosmosDBService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="ICosmosDBService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddAzureCosmosDBService(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddOptions<CosmosDbSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB));

            services.AddSingleton<CosmosClient>(serviceProvider =>
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

            services.AddSingleton<ICosmosDBService, AzureCosmosDBService>();
        }
    }
}
