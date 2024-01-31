using Asp.Versioning;
using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Tokenizers;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.ResourceProviders;
using FoundationaLLM.Vectorization.Services;
using FoundationaLLM.Vectorization.Services.ContentSources;
using FoundationaLLM.Vectorization.Services.RequestSources;
using FoundationaLLM.Vectorization.Services.Text;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration[AppConfigurationKeys.FoundationaLLM_AppConfig_ConnectionString]);
    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(new DefaultAzureCredential());
    });
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Vectorization);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs_VectorizationAPI);
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_VectorizationAPI_AppInsightsConnectionString],
    DeveloperMode = builder.Environment.IsDevelopment()
});

var allowAllCorsOrigins = "AllowAllOrigins";
builder.Services.AddCors(policyBuilder =>
{
    policyBuilder.AddPolicy(allowAllCorsOrigins,
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since", "Cache-Control", "Content-Type", "Range", "Authorization", "X-AGENT-HINT");
            policy.AllowAnyMethod();
        });
});

// Add configurations to the container
builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));

builder.Services.AddOptions<VectorizationWorkerSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeys.FoundationaLLM_Vectorization_VectorizationWorker));

builder.Services.AddOptions<BlobStorageServiceSettings>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_ResourceProviderService_Storage));

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
    DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService, (sp, obj) =>
    {
        var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
            .Get(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService);
        var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

        return new BlobStorageService(
            Options.Create<BlobStorageServiceSettings>(settings),
            logger);
    });

// Vectorization state
builder.Services.AddSingleton<IVectorizationStateService, MemoryVectorizationStateService>();

// Vectorization resource provider
builder.Services.AddKeyedSingleton<IResourceProviderService, VectorizationResourceProviderService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService);
builder.Services.ActivateKeyedSingleton<IResourceProviderService>(
    DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService);

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

// Request sources cache
builder.Services.AddSingleton<IRequestSourcesCache, RequestSourcesCache>();
builder.Services.ActivateSingleton<IRequestSourcesCache>();

// Vectorization
builder.Services.AddScoped<IVectorizationService, VectorizationService>();

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
         options.DefaultApiVersion = new ApiVersion(1, 0);
     })
    .AddApiExplorer(options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";
    });

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

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
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
