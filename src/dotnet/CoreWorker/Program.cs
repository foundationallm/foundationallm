using Azure.Identity;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using FoundationaLLM.Core.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration["FoundationaLLM:AppConfig:ConnectionString"]);

    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(new DefaultAzureCredential());
    });
    options.Select("FoundationaLLM:CoreWorker:*");
    options.Select("FoundationaLLM:CosmosDB:*");
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.Services.AddOptions<CosmosDbSettings>()
    .Bind(builder.Configuration.GetSection("FoundationaLLM:CosmosDB"));
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<ICosmosDbChangeFeedService, CosmosDbChangeFeedService>();
builder.Services.AddHostedService<ChangeFeedWorker>();
builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
{
    options.ConnectionString = builder.Configuration["FoundationaLLM:CoreWorker:AppInsightsConnectionString"];
});

var host = builder.Build();

host.Run();
