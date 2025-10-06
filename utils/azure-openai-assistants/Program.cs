#pragma warning disable OPENAI001

using Azure.AI.OpenAI;
using Azure.Identity;
using System.ClientModel.Primitives;
using System.CommandLine;

var accountOption = new Option<string>("--account")
{
    Description = "The name of the Azure OpenAI account.",
    Required = true
};

var assistantIdOption = new Option<string>("--assistant-id")
{
    Description = "The assistant identifier.",
    Required = true
};

var vectorStoreIdOption = new Option<string>("--vector-store-id")
{
    Description = "The vector store identifier.",
    Required = true
};

var rootCommand = new RootCommand("FoundationaLLM Azure OpenAI Utility");

var assistantCommand = new Command("assistant", "Manage Azure OpenAI assistants");
var vectorStoreCommand = new Command("vector-store", "Manage Azure OpenAI vector stores");

var assistantShowCommand = new Command("show", "Show the details of an Azure OpenAI assistant")
{
    accountOption,
    assistantIdOption
};
assistantShowCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var assistantId = parseResult.GetValue(assistantIdOption);
    ShowAssistant(
        account!,
        assistantId!);
});

var vectorStoreShowCommand = new Command("show", "Show the details of an Azure OpenAI vector store")
{
    accountOption,
    vectorStoreIdOption
};

vectorStoreShowCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    ShowVectorStore(
        account!,
        vectorStoreId!);
});

var vectorStoreDeleteCommand = new Command("delete", "Delete an Azure OpenAI vector store")
{
    accountOption,
    vectorStoreIdOption
};

vectorStoreDeleteCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    await DeleteVectorStore(
        account!,
        vectorStoreId!);
});

rootCommand.Subcommands.Add(assistantCommand);
assistantCommand.Subcommands.Add(assistantShowCommand);

rootCommand.Subcommands.Add(vectorStoreCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreShowCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreDeleteCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

void ShowAssistant(
    string account,
    string assistantId)
{
    Console.WriteLine($"Retrieving assistant {assistantId} from Azure OpenAI account {account}...");
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var assistantClient = azureOpenAIClient.GetAssistantClient();
        var clientResult = assistantClient.GetAssistant(assistantId);
        Console.WriteLine(
            ModelReaderWriter.Write(clientResult.Value).ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while retrieving the assistant: {ex.Message}");
    }
}

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

void ShowVectorStore(
    string account,
    string vectorStoreId)
{
    Console.WriteLine($"Retrieving vector store {vectorStoreId} from Azure OpenAI account {account}...");

    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var clientResult = vectorStoreClient.GetVectorStore(vectorStoreId);

        Console.WriteLine(
            ModelReaderWriter.Write(clientResult.Value).ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while retrieving the vector store: {ex.Message}");
    }
}