using Asp.Versioning;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using FoundationaLLM;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Common.Services.Tokenizers;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.ResourceProviders;
using FoundationaLLM.Vectorization.Services.ContentSources;
using FoundationaLLM.Vectorization.Services.Text;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using FoundationaLLM.Vectorization.Worker;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

DefaultAuthentication.Production = builder.Environment.IsProduction();

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(DefaultAuthentication.GetAzureCredential());
    });
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Vectorization);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs_VectorizationWorker);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Events);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
});

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

// Add the Configuration resource provider
builder.AddConfigurationResourceProvider();

// Add the OpenTelemetry telemetry service and send telemetry data to Azure Monitor.
builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_VectorizationWorker_AppInsightsConnectionString];
});

// Create a dictionary of resource attributes.
var resourceAttributes = new Dictionary<string, object> {
    { "service.name", "VectorizationWorker" },
    { "service.namespace", "FoundationaLLM" },
    { "service.instance.id", Guid.NewGuid().ToString() }
};

// Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
builder.Services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
    builder.ConfigureResource(resourceBuilder =>
        resourceBuilder.AddAttributes(resourceAttributes)));

var allowAllCorsOrigins = "AllowAllOrigins";
builder.Services.AddCors(policyBuilder =>
{
    policyBuilder.AddPolicy(allowAllCorsOrigins,
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since", "Cache-Control", "Content-Type", "Range", "Authorization");
            policy.AllowAnyMethod();
        });
});

// Add configurations to the container
builder.Services.AddInstanceProperties(builder.Configuration);

// Add Azure ARM services
builder.Services.AddAzureResourceManager();

// Add event services
builder.Services.AddAzureEventGridEvents(
    builder.Configuration,
    AppConfigurationKeySections.FoundationaLLM_Events_AzureEventGridEventService_Profiles_VectorizationWorker);

builder.Services.AddOptions<VectorizationWorkerSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeys.FoundationaLLM_Vectorization_VectorizationWorker));

builder.Services.AddOptions<BlobStorageServiceSettings>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_BlobStorageVectorizationStateService)
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_StateService));

builder.Services.AddOptions<SemanticKernelTextEmbeddingServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService));

builder.Services.AddOptions<AzureAISearchIndexingServiceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_AzureAISearchIndexingService));

builder.Services.AddKeyedSingleton(
    typeof(IConfigurationSection),
    DependencyInjectionKeys.FoundationaLLM_Vectorization_Queues,
    builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_Queues));

builder.Services.AddKeyedSingleton(
    typeof(IConfigurationSection),
    DependencyInjectionKeys.FoundationaLLM_Vectorization_Steps,
    builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_Steps));

// Add services to the container.

builder.Services.AddKeyedSingleton<IStorageService, BlobStorageService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_BlobStorageVectorizationStateService, (sp, obj) =>
    {
        var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
            .Get(DependencyInjectionKeys.FoundationaLLM_Vectorization_BlobStorageVectorizationStateService);
        var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

        return new BlobStorageService(
            Options.Create<BlobStorageServiceSettings>(settings),
            logger);
    });

// Vectorization state
builder.Services.AddSingleton<IVectorizationStateService, BlobStorageVectorizationStateService>();

// Resource validation
builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

// Vectorization resource provider
builder.Services.AddVectorizationResourceProvider(builder.Configuration);

// Service factories
builder.Services.AddSingleton<IVectorizationServiceFactory<IContentSourceService>, ContentSourceServiceFactory>();
builder.Services.AddSingleton<IVectorizationServiceFactory<ITextSplitterService>, TextSplitterServiceFactory>();
builder.Services.AddSingleton<IVectorizationServiceFactory<ITextEmbeddingService>, TextEmbeddingServiceFactory>();
builder.Services.AddSingleton<IVectorizationServiceFactory<IIndexingService>, IndexingServiceFactory>();

// Tokenizer
builder.Services.AddKeyedSingleton<ITokenizerService, MicrosoftBPETokenizerService>(TokenizerServiceNames.MICROSOFT_BPE_TOKENIZER);
builder.Services.ActivateKeyedSingleton<ITokenizerService>(TokenizerServiceNames.MICROSOFT_BPE_TOKENIZER);

// Text embedding
builder.Services.AddKeyedSingleton<ITextEmbeddingService, SemanticKernelTextEmbeddingService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService);

// Indexing
builder.Services.AddKeyedSingleton<IIndexingService, AzureAISearchIndexingService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_AzureAISearchIndexingService);

builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers();

// Add API Key Authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<APIKeyAuthenticationFilter>();
builder.Services.AddOptions<APIKeyValidationSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_VectorizationWorker));

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
    });

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
