using FoundationaLLM;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using FoundationaLLM.Core.Worker;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

ServiceContext.Initialize(
    builder.Environment.IsProduction(),
    ServiceNames.CoreWorker);

builder.AddDIContainerSettings();

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);

    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(ServiceContext.AzureCredential);
    });

    ConfigurationOptions.SelectForCoreWorker(options);
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.Services.AddOptions<AzureCosmosDBSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB));

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AzureCosmosDBSettings>>().Value;
    return new CosmosClientBuilder(settings.Endpoint, ServiceContext.AzureCredential)
        .WithSerializerOptions(new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        })
        .WithConnectionModeGateway()
        .Build();
});

builder.Services.AddSingleton<IAzureCosmosDBService, AzureCosmosDBService>();
builder.Services.AddSingleton<ICosmosDbChangeFeedService, AzureCosmosDBChangeFeedService>();
builder.Services.AddHostedService<ChangeFeedWorker>();
builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
{
    options.ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreWorker_Essentials_AppInsightsConnectionString];
});

var host = builder.Build();

host.Run();
