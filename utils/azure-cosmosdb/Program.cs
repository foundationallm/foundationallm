using Azure.Identity;
using Microsoft.Azure.Cosmos;
using System.CommandLine;
using System.Text.Json;

var accountOption = new Option<string>("--account")
{
    Description = "The connection string to the FoundationaLLM Azure Cosmos DB account.",
    Required = true
};

var directConnectionOption = new Option<bool>("--direct-connection")
{
    Description = "Use ConnectionMode = Direct to connect to FoundationaLLM Azure Cosmos DB account.",
    Arity = ArgumentArity.Zero
};

var noProgressOption = new Option<bool>("--no-progress")
{
    Description = "Suppress progress output.",
    Arity = ArgumentArity.Zero
};

var maxDaysAgeOption = new Option<int>("--max-days-age")
{
    Description = "The maximum age in days of a purgeable conversation. Must be greater than 30.",
    DefaultValueFactory = (argResult) => 60
};

var exportFileNameOption = new Option<string>("--export-file")
{
    Description = "The name of the export file to write the list of purgeable conversations to.",
    DefaultValueFactory = (argResult) => $"{DateTimeOffset.UtcNow:yyyy-MM-dd-HHmmss}-export.jsonl"
};

var rootCommand = new RootCommand("FoundationaLLM Azure Cosmos DB Utility");

var conversationCommand = new Command("conversation", "Manage conversations in Azure Cosmos DB");
var conversationPurgeableItemCommand = new Command("purgeable-item", "Manage purgeable conversation items in Azure Cosmos DB");

var listConversationPurgeableItemCommand = new Command("list", "List purgeable conversations")
{
    accountOption,
    directConnectionOption,
    maxDaysAgeOption,
    exportFileNameOption,
    noProgressOption
};
listConversationPurgeableItemCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var directConnection = parseResult.GetValue(directConnectionOption);
    var maxDaysAge = parseResult.GetValue(maxDaysAgeOption);
    var exportFileName = parseResult.GetValue(exportFileNameOption);
    var noProgress = parseResult.GetValue(noProgressOption);
    await ListConversationPurgeableItems(
        account!,
        directConnection,
        maxDaysAge,
        exportFileName!,
        noProgress);
});

rootCommand.Subcommands.Add(conversationCommand);
conversationCommand.Subcommands.Add(conversationPurgeableItemCommand);
conversationPurgeableItemCommand.Subcommands.Add(listConversationPurgeableItemCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

async Task ListConversationPurgeableItems(
    string account,
    bool directConnection, 
    int maxDaysAge,
    string exportFileName,
    bool noProgress)
{
    try
    {
        if (maxDaysAge <= 30)
            throw new Exception("The maximum age in days of a purgeable conversation item must be greater than 30.");
        var refTimestamp = DateTime.UtcNow;

        var connectionMode = directConnection
            ? ConnectionMode.Direct
            : ConnectionMode.Gateway;
        var client = new CosmosClient(account, new AzureCliCredential(), new CosmosClientOptions
        {
            ConnectionMode = connectionMode,
            UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions()
        });
        if (!noProgress)
            Console.Error.WriteLine($"Connection mode is {connectionMode}");
        var c = client.GetContainer("database", "Sessions");

        var contentMatchingCriteria = "c.deleted and (c.type = \"Session\" or (c.type = \"Message\" and ((not IS_NULL(c.attachments) and ARRAY_LENGTH(c.attachments) > 0) or (not IS_NULL(c.content) and ARRAY_LENGTH(c.content) > 0 and (ARRAY_CONTAINS(c.content, {\"type\": \"image_file\"}, true) or ARRAY_CONTAINS(c.content, {\"type\": \"file_path\"}, true))))))";
        var timeMatchingCriteria = "c._ts * 1000 < GetCurrentTimestamp() - 30*24*60*60*1000 and c._ts * 1000 >= GetCurrentTimestamp() - @maxDaysAge*24*60*60*1000";
        var q = new QueryDefinition($"SELECT c.id, c.sessionId as conversationId, c.type, c.content, c.attachments, TimestampToDateTime(c._ts * 1000) AS lastModifiedUtc FROM c WHERE {contentMatchingCriteria} and {timeMatchingCriteria}")
            .WithParameter("@maxDaysAge", maxDaysAge);

        var items = new List<string>();
        using var feed = c.GetItemQueryIterator<JsonElement>(q, requestOptions: new QueryRequestOptions
        {
            MaxItemCount = 1000, // tune page size
            ResponseContinuationTokenLimitInKb = 2
        });

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();

            foreach (var doc in response)
            {
                items.Add(JsonSerializer.Serialize(doc));
                if (!noProgress
                    && items.Count % 100 == 0)
                    Console.Error.WriteLine($"Found {items.Count} conversation purgeable items so far...");
            }
        }

        if (!noProgress)
            Console.Error.WriteLine($"Found {items.Count} conversation purgeable items.");

        File.WriteAllText(exportFileName, string.Join(Environment.NewLine, items));

        var result = new
        {
            reference_timestamp = refTimestamp,
            max_days_age = maxDaysAge,
            count = items.Count
        };

        Console.WriteLine(JsonSerializer.Serialize(result));
    }
    catch (CosmosException cex)
    {
        Console.WriteLine(
            JsonSerializer.Serialize(
                new
                {
                    status_code = cex.StatusCode,
                    sub_status_code = cex.SubStatusCode,
                    error = cex.ToString()
                }));
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            JsonSerializer.Serialize(
                new
                {
                    error = ex.Message
                }));
    }
}