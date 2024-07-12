using Asp.Versioning;
using FoundationaLLM;
using FoundationaLLM.Authorization.Services;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Tokenizers;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services.Indexing;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Serializers;
using FoundationaLLM.Vectorization.Services.ContentSources;
using FoundationaLLM.Vectorization.Services.RequestProcessors;
using FoundationaLLM.Vectorization.Services.RequestSources;
using FoundationaLLM.Vectorization.Services.Text;
using FoundationaLLM.Vectorization.Services.VectorizationServices;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

DefaultAuthentication.Initialize(
    builder.Environment.IsProduction(),
    ServiceNames.VectorizationAPI);

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
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Vectorization);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs_VectorizationAPI);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs_GatewayAPI);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Events);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_DataSource); //resource provider settings
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_DataSources); //data source settings
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

// NOTE: This is required while the service uses API key authentication.
// Once the service is moved over to Entra ID authentication, this must be replaced with the proper implementation.
builder.Services.AddSingleton<IAuthorizationService, NullAuthorizationService>();

// Add OpenTelemetry.
builder.AddOpenTelemetry(
    AppConfigurationKeys.FoundationaLLM_APIs_VectorizationAPI_AppInsightsConnectionString,
    ServiceNames.VectorizationAPI);

// CORS policies
builder.AddCorsPolicies();

// Add configurations to the container
builder.Services.AddInstanceProperties(builder.Configuration);

// Add Azure ARM services
builder.Services.AddAzureResourceManager();

// Add event services
builder.Services.AddAzureEventGridEvents(
    builder.Configuration,
    AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService_Profiles_VectorizationAPI);

builder.Services.AddOptions<VectorizationWorkerSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeys.FoundationaLLM_Vectorization_VectorizationWorker));

builder.Services.AddOptions<SemanticKernelTextEmbeddingServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService));

builder.Services.AddOptions<AzureAISearchIndexingServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_AzureAISearchIndexingService));

builder.Services.AddOptions<AzureCosmosDBNoSQLIndexingServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_AzureCosmosDBNoSQLIndexingService));

builder.Services.AddOptions<PostgresIndexingServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_PostgresIndexingService));

// Add queue and step configurations
builder.Services.AddKeyedSingleton(
    typeof(IConfigurationSection),
    DependencyInjectionKeys.FoundationaLLM_Vectorization_Queues,
    builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_Queues));

builder.Services.AddKeyedSingleton(
    typeof(IConfigurationSection),
    DependencyInjectionKeys.FoundationaLLM_Vectorization_Steps,
    builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_Steps));

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AzureCosmosDBNoSQLIndexingServiceSettings>>().Value;
    return new CosmosClientBuilder(settings.ConnectionString)
        .WithCustomSerializer(new CosmosSystemTextJsonSerializer(JsonSerializerOptions.Default))
        .WithConnectionModeGateway()
        .Build();
});

// Request sources cache
builder.Services.AddSingleton<IRequestSourcesCache, RequestSourcesCache>();
builder.Services.ActivateSingleton<IRequestSourcesCache>();

// Vectorization state
builder.Services.AddSingleton<MemoryVectorizationStateService, MemoryVectorizationStateService>(); //for sync requests
builder.Services.AddSingleton<IVectorizationStateService, BlobStorageVectorizationStateService>(); //for async requests

// Register the vectorization service factory.
builder.Services.AddSingleton<VectorizationServiceFactory>();

// Register the local vectorization processor.
builder.Services.AddSingleton<IVectorizationRequestProcessor, LocalVectorizationRequestProcessor>();

// Resource validation
builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

// Resource providers
builder.AddConfigurationResourceProvider();
builder.AddDataSourceResourceProvider();
builder.AddVectorizationResourceProvider();

builder.AddPipelineExecution();

// Service factories
builder.Services.AddSingleton<IVectorizationServiceFactory<IContentSourceService>, ContentSourceServiceFactory>();
builder.Services.AddSingleton<IVectorizationServiceFactory<ITextSplitterService>, TextSplitterServiceFactory>();
builder.Services.AddSingleton<IVectorizationServiceFactory<ITextEmbeddingService>, TextEmbeddingServiceFactory>();
builder.Services.AddSingleton<IVectorizationServiceFactory<IIndexingService>, IndexingServiceFactory>();

// Tokenizer
builder.Services.AddKeyedSingleton<ITokenizerService, MicrosoftBPETokenizerService>(TokenizerServiceNames.MICROSOFT_BPE_TOKENIZER);
builder.Services.ActivateKeyedSingleton<ITokenizerService>(TokenizerServiceNames.MICROSOFT_BPE_TOKENIZER);

// Gateway text embedding
builder.Services.AddKeyedScoped<ITextEmbeddingService, GatewayTextEmbeddingService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_GatewayTextEmbeddingService);
builder.AddGatewayService();

builder.Services.AddScoped<ICallContext, CallContext>();
builder.AddHttpClientFactoryService();

// Indexing
builder.Services.AddKeyedSingleton<IIndexingService, AzureAISearchIndexingService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_AzureAISearchIndexingService);
builder.Services.AddKeyedSingleton<IIndexingService, AzureCosmosDBNoSQLIndexingService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_AzureCosmosDBNoSQLIndexingService);
builder.Services.AddKeyedSingleton<IIndexingService, PostgresIndexingService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_PostgresIndexingService);

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
builder.Services.AddControllers();

// Add API Key Authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<APIKeyAuthenticationFilter>();
builder.Services.AddOptions<APIKeyValidationSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_VectorizationAPI));

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
                            new[] {"user_impersonation"}
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

var app = builder.Build();

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
    });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
