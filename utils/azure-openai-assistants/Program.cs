#pragma warning disable OPENAI001

using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;
using OpenAI.VectorStores;
using System.ClientModel.Primitives;
using System.CommandLine;
using System.Text.Json;

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
    Required = false
};

var instructionsOption = new Option<string>("--instructions")
{
    Description = "The instructions for the assistant.",
    Required = false
};

var vectorStoreIdOption = new Option<string>("--vector-store-id")
{
    Description = "The vector store identifier.",
    Required = true
};

var fileIdOption = new Option<string>("--file-id")
{
    Description = "The file identifier.",
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
    instructionsFileOption,
    instructionsOption
};
AddMutuallyExclusiveValidator<string>(
    assistantCreateCommand,
    true,
    instructionsFileOption,
    instructionsOption);
assistantCreateCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var assistantName = parseResult.GetValue(assistantNameOption);
    var modelDeploymentName = parseResult.GetValue(modelDeploymentNameOption);
    var instructionsFile = parseResult.GetValue(instructionsFileOption);
    var instructions = parseResult.GetValue(instructionsOption);
    CreateAssistant(
        account!,
        assistantName!,
        modelDeploymentName!,
        instructionsFile,
        instructions);
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

var vectorStoreCreateCommand = new Command("create", "Create an Azure OpenAI vector store")
{
    accountOption
};
vectorStoreCreateCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    CreateVectorStore(
        account!);
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

var vectorStoreFileCommand = new Command("file", "Manage the files in an Azure OpenAI vector store");

var vectorStoreFileListCommand = new Command("list", "List the content of an Azure OpenAI vector store")
{
    accountOption,
    vectorStoreIdOption
};
vectorStoreFileListCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    ListVectorStoreFiles(
        account!,
        vectorStoreId!);
});

var vectorStoreFileAddCommand = new Command("add", "Add a file to an Azure OpenAI vector store")
{
    accountOption,
    vectorStoreIdOption,
    fileIdOption
};
vectorStoreFileAddCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    var fileId = parseResult.GetValue(fileIdOption);
    AddVectorStoreFile(
        account!,
        vectorStoreId!,
        fileId!);
});

var vectorStoreFileShowCommand = new Command("show", "Show the details of a file in an Azure OpenAI vector store")
{
    accountOption,
    vectorStoreIdOption,
    fileIdOption
};
vectorStoreFileShowCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    var fileId = parseResult.GetValue(fileIdOption);
    ShowVectorStoreFile(
        account!,
        vectorStoreId!,
        fileId!);
});

rootCommand.Subcommands.Add(assistantCommand);
assistantCommand.Subcommands.Add(assistantCreateCommand);
assistantCommand.Subcommands.Add(assistantShowCommand);
assistantCommand.Subcommands.Add(assistantSetCommand);

assistantSetCommand.Subcommands.Add(assistantSetVectorStoreCommand);

rootCommand.Subcommands.Add(vectorStoreCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreCreateCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreShowCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreFileCommand);
vectorStoreCommand.Subcommands.Add(vectorStoreDeleteCommand);

vectorStoreFileCommand.Subcommands.Add(vectorStoreFileListCommand);
vectorStoreFileCommand.Subcommands.Add(vectorStoreFileAddCommand);
vectorStoreFileCommand.Subcommands.Add(vectorStoreFileShowCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

static void AddMutuallyExclusiveValidator<T>(
    Command cmd,
    bool requireExactlyOne,
    params Option<T>[] options)
{
    cmd.Validators.Add(ctx =>
    {
        int count = options.Count(o => ctx.GetValue<T>(o) is not null);
        if (count > 1)
            ctx.AddError($"Options {string.Join(", ", options.Select(o => o.Aliases.First()))} are mutually exclusive.");
        else if (requireExactlyOne && count == 0)
            ctx.AddError($"Specify exactly one of {string.Join(", ", options.Select(o => o.Aliases.First()))}.");
    });
}

void CreateAssistant(
    string account,
    string assistantName,
    string modelDeploymentName,
    string? instructionsFile,
    string? instructions)
{
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
                Instructions = instructionsFile is not null
                    ? File.ReadAllText(instructionsFile)
                    : instructions!,
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
        Console.WriteLine(
            ModelReaderWriter.Write(assistantResult.Value).ToString());
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

void SetAssistantVectorStore(
    string account,
    string assistantId,
    string vectorStoreId)
{
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

        Console.WriteLine(
            ModelReaderWriter.Write(updateAssistantResult.Value).ToString());
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

void ShowAssistant(
    string account,
    string assistantId)
{
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
        Console.WriteLine(
            JsonSerializer.Serialize(
                new
                {
                    error = ex.Message
                }));
    }
}

void CreateVectorStore(
    string account)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var clientResult = vectorStoreClient.CreateVectorStore(true, new VectorStoreCreationOptions
        {
            ExpirationPolicy = new VectorStoreExpirationPolicy(
                VectorStoreExpirationAnchor.LastActiveAt,
                365)
        });
        Console.WriteLine(
            ModelReaderWriter.Write(clientResult.Value!).ToString());
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

async Task DeleteVectorStore(
    string account,
    string vectorStoreId)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var clientResult = await vectorStoreClient.DeleteVectorStoreAsync(vectorStoreId);

        if (!clientResult.Value.Deleted)
            Console.WriteLine(
                JsonSerializer.Serialize(
                new
                {
                    success = false,
                    error = "Failed to delete vector store."
                }));
        else
            Console.WriteLine(JsonSerializer.Serialize(
                new
                {
                    success = true
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

void AddVectorStoreFile(
    string account,
    string vectorStoreId,
    string fileId)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var addFileResult = vectorStoreClient.AddFileToVectorStore(vectorStoreId, fileId, false);
        Console.WriteLine(
            ModelReaderWriter.Write(addFileResult.Value!).ToString());
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

void ShowVectorStoreFile(
    string account,
    string vectorStoreId,
    string fileId)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var fileAssociationResult = vectorStoreClient.GetFileAssociation(vectorStoreId, fileId);
        Console.WriteLine(
            ModelReaderWriter.Write(fileAssociationResult.Value!).ToString());
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

void ListVectorStoreFiles(
    string account,
    string vectorStoreId)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
        var clientResult = vectorStoreClient.GetVectorStore(vectorStoreId);

        var filesResult = vectorStoreClient.GetFileAssociations(vectorStoreId, new VectorStoreFileAssociationCollectionOptions
        {
            PageSizeLimit = 100
        });

        var fileIds = filesResult
            .Where(fa => fa.Status == VectorStoreFileAssociationStatus.Completed)
            .Select(fa => fa.FileId)
            .ToList();

        Console.WriteLine(
            JsonSerializer.Serialize(fileIds));
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

void ShowVectorStore(
    string account,
    string vectorStoreId)
{
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
        Console.WriteLine(
            JsonSerializer.Serialize(
                new
                {
                    error = ex.Message
                }));
    }
}