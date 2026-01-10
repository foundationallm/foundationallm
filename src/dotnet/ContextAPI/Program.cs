using Asp.Versioning;
using FoundationaLLM;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Security;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

ServiceContext.Initialize(
    builder.Environment.IsProduction(),
    ServiceNames.ContextAPI);

builder.AddDIContainerSettings();

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration((Action<AzureAppConfigurationOptions>)(options =>
{
    options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
    options.ConfigureKeyVault(options => { options.SetCredential(ServiceContext.AzureCredential); });

    ConfigurationOptions.SelectForContextAPI(options);
}));

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.AddIdentitiyManagement();
builder.AddAuthorizationServiceClient();

// Add OpenTelemetry.
builder.AddOpenTelemetry(
    AppConfigurationKeys.FoundationaLLM_APIEndpoints_ContextAPI_Essentials_AppInsightsConnectionString,
    ServiceNames.ContextAPI);

// CORS policies
builder.AddCorsPolicies();

// FoundationaLLM instance-leve services
builder.AddInstanceProperties();

// HTTP services
builder.AddHttpClientFactoryService();

// Add Azure ARM services.
builder.AddAzureResourceManager();

// Add event services.
builder.Services.AddAzureEventGridEvents(
    builder.Configuration,
    AppConfigurationKeySections.FoundationaLLM_Events_Profiles_ContextAPI);


//---------------------------
// Singleton services
//---------------------------

builder.AddAzureCosmosDBContextServices();
builder.AddAzureContainerAppsCodeSessionProviderServices();
builder.AddKnowledgeGraphService();
builder.AddAzureCosmosDBService();
builder.AddAzureContentSafetyService();

// Add authorization services.
builder.AddIdentitiyManagement();
builder.AddAuthorizationServiceClient();

//---------------------------
// Scoped services
//---------------------------

builder.AddOrchestrationContext();
builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();
builder.AddFileService();
builder.AddCodeSessionService();

//---------------------------
// Resource providers
//---------------------------
builder.AddResourceProviderCacheSettings();
builder.AddResourceValidatorFactory();

builder.AddAuthorizationResourceProvider();
builder.AddAgentResourceProvider();
builder.AddPromptResourceProvider();
//builder.AddAttachmentResourceProvider();
builder.AddConfigurationResourceProvider();
//builder.AddAzureOpenAIResourceProvider();
//builder.AddAIModelResourceProvider();
//builder.AddConversationResourceProvider();
builder.AddVectorResourceProvider();
builder.AddContextResourceProvider(proxyMode: false);
builder.AddRemoteDataPipelineServiceClient(); // Required by the DataPipeline resource provider.
builder.AddDataPipelineResourceProvider();

// Add API Key Authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<APIKeyAuthenticationFilter>();
builder.Services.AddOptions<APIKeyValidationSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_ContextAPI_Essentials));
builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

builder.Services
    .AddApiVersioning(options =>
    {
        // Reporting api versions will return the headers
        // "api-supported-versions" and "api-deprecated-versions"
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(new DateOnly(2025, 3, 20));
    })
    .AddMvc()
    .AddApiExplorer();

builder.Services.AddAuthorization();

// Increase request size limit to 512 MB.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 536870912; // 512 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 536870912; // 512 MB
});
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
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

var app = builder.Build();

// Set the CORS policy before other middleware.
app.UseCors(CorsPolicyNames.AllowAllOrigins);

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
    });

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
