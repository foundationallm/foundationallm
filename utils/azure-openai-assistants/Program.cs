#pragma warning disable OPENAI001

using Azure.AI.OpenAI;
using Azure.Identity;
using System.CommandLine;

var accountOption = new Option<string>("--account")
{
    Description = "The name of the Azure OpenAI account.",
    Required = true
};

var vectorStoreIdOption = new Option<string>("--vector-store-id")
{
    Description = "The vector store identifier.",
    Required = true
};

var rootCommand = new RootCommand("FoundationaLLM Azure OpenAI Utility");

var vectorStoreCommand = new Command("vector-store", "Manage Azure OpenAI vector stores")
{
    accountOption,
    vectorStoreIdOption
};

vectorStoreCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    await DeleteVectorStore(
        account!,
        vectorStoreId!);
});

rootCommand.Subcommands.Add(vectorStoreCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

async Task DeleteVectorStore(
    string account,
    string vectorStoreId)
{
    Console.WriteLine($"Starting to delete vector store {vectorStoreId} from Azure OpenAI account {account}...");

    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var clientResult = await vectorStoreClient.DeleteVectorStoreAsync(vectorStoreId);

        if (!clientResult.Value.Deleted)
            Console.WriteLine($"Failed to delete vector store.");
        else
            Console.WriteLine("Vector store deleted successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while deleting the vector store: {ex.Message}");
    }
}