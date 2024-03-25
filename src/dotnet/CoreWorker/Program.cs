using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Services;
using FoundationaLLM.Core.Worker;

var builder = Host.CreateApplicationBuilder(args);

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
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_CoreWorker);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_CosmosDB);
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.Services.AddOptions<CosmosDbSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<ICosmosDbChangeFeedService, CosmosDbChangeFeedService>();
builder.Services.AddHostedService<ChangeFeedWorker>();
builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
{
    options.ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_CoreWorker_AppInsightsConnectionString];
});

var host = builder.Build();

host.Run();
