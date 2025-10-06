#pragma warning disable OPENAI001

using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;
using OpenAI.VectorStores;
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

var assistantNameOption = new Option<string>("--assistant-name")
{
    Description = "The assistant name.",
    Required = true
};

var modelDeploymentNameOption = new Option<string>("--model-deployment-name")
{
    Description = "The model deployment name.",
    Required = true
};

var instructionsFileOption = new Option<string>("--instructions-file")
{
    Description = "The path of the text file containing the instructions for the assistant.",
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

var assistantCreateCommand = new Command("create", "Create an Azure OpenAI assistant")
{
    accountOption,
    assistantNameOption,
    modelDeploymentNameOption,
    instructionsFileOption
};
assistantCreateCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var assistantName = parseResult.GetValue(assistantNameOption);
    var modelDeploymentName = parseResult.GetValue(modelDeploymentNameOption);
    var instructionsFile = parseResult.GetValue(instructionsFileOption);
    CreateAssistant(
        account!,
        assistantName!,
        modelDeploymentName!,
        instructionsFile!);
});

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

var assistantSetCommand = new Command("set", "Set the properties of an Azure OpenAI assistant");

var assistantSetVectorStoreCommand = new Command("vector-store", "Set the vector store of an Azure OpenAI assistant")
{
    accountOption,
    assistantIdOption,
    vectorStoreIdOption
};
assistantSetVectorStoreCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var assistantId = parseResult.GetValue(assistantIdOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    SetAssistantVectorStore(
        account!,
        assistantId!,
        vectorStoreId!);
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
assistantCommand.Subcommands.Add(assistantCreateCommand);
assistantCommand.Subcommands.Add(assistantShowCommand);
assistantCommand.Subcommands.Add(assistantSetCommand);

assistantSetCommand.Subcommands.Add(assistantSetVectorStoreCommand);

rootCommand.Subcommands.Add(vectorStoreCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreShowCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreDeleteCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

void CreateAssistant(
    string account,
    string assistantName,
    string modelDeploymentName,
    string instructionsFile)
{
    Console.WriteLine($"Starting to create assistant {assistantName} in Azure OpenAI account {account}...");

    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var assistantClient = azureOpenAIClient.GetAssistantClient();
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();

        // Create the assistant-level vector store and assign it to the file search tool definition for the assistant.
        var vectorStoreResult = vectorStoreClient.CreateVectorStore(true, new VectorStoreCreationOptions
        {
            Name = assistantName,
            ExpirationPolicy = new VectorStoreExpirationPolicy(
                VectorStoreExpirationAnchor.LastActiveAt,
                365)
        });
        var fileSearchToolResources = new FileSearchToolResources();
        fileSearchToolResources.VectorStoreIds.Add(vectorStoreResult.Value!.Id);

        var assistantResult = assistantClient.CreateAssistant(
            modelDeploymentName,
            new AssistantCreationOptions()
            {
                Name = assistantName,
                Instructions = File.ReadAllText(instructionsFile),
                Tools =
                    {
                    new CodeInterpreterToolDefinition(),
                    new FileSearchToolDefinition()
                    },
                ToolResources = new ToolResources()
                {
                    FileSearch = fileSearchToolResources
                }

            });
        Console.WriteLine("Assistant created successfully.");
        Console.WriteLine(
            ModelReaderWriter.Write(assistantResult.Value).ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the assistant: {ex.Message}");
    }
}

void SetAssistantVectorStore(
    string account,
    string assistantId,
    string vectorStoreId)
{
    Console.WriteLine($"Starting to set vector store {vectorStoreId} for assistant {assistantId} in Azure OpenAI account {account}...");
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var assistantClient = azureOpenAIClient.GetAssistantClient();

        var fileSearchToolResources = new FileSearchToolResources();
        fileSearchToolResources.VectorStoreIds.Add(vectorStoreId);

        // Update the assistant with the new vector store file search tool resource               
        var updateAssistantResult = assistantClient.ModifyAssistant(
            assistantId,
            new AssistantModificationOptions
                {
                    ToolResources = new ToolResources()
                    {
                        FileSearch = fileSearchToolResources
                    }
                });

        Console.WriteLine("Assistant vector store updated successfully.");
        Console.WriteLine(
            ModelReaderWriter.Write(updateAssistantResult.Value).ToString());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while updating the assistant: {ex.Message}");
    }
}

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