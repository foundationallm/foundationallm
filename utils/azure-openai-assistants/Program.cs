#pragma warning disable OPENAI001

using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;
using OpenAI.VectorStores;
using System.ClientModel;
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
    Required = false
};

var fileIdOption = new Option<string>("--file-id")
{
    Description = "The file identifier.",
    Required = false
};

var threadIdOption = new Option<string>("--thread-id")
{
    Description = "The thread identifier.",
    Required = false
};

var idsOption = new Option<string[]>("--ids")
{
    Description = "The list of identifiers.",
    AllowMultipleArgumentsPerToken = true,
    Arity = ArgumentArity.OneOrMore,
    Required = false
};

var rootCommand = new RootCommand("FoundationaLLM Azure OpenAI Utility");

var assistantCommand = new Command("assistant", "Manage Azure OpenAI assistants");
var vectorStoreCommand = new Command("vector-store", "Manage Azure OpenAI vector stores");
var fileCommand = new Command("file", "Manage Azure OpenAI files");
var threadCommand = new Command("thread", "Manage Azure OpenAI assistant threads");

var assistantCreateCommand = new Command("create", "Create an Azure OpenAI assistant")
{
    accountOption,
    assistantNameOption,
    modelDeploymentNameOption,
    instructionsFileOption,
    instructionsOption
};
AddMutuallyExclusiveValidator<string, string>(
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
    vectorStoreIdOption,
    idsOption
};
AddMutuallyExclusiveValidator<string, string[]>(
    vectorStoreDeleteCommand,
    true,
    vectorStoreIdOption,
    idsOption);
vectorStoreDeleteCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var vectorStoreId = parseResult.GetValue(vectorStoreIdOption);
    var ids = parseResult.GetValue(idsOption);
    await DeleteVectorStore(
        account!,
        vectorStoreId,
        ids);
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

var fileShowCommand = new Command("show", "Show the details of an Azure OpenAI file")
{
    accountOption,
    fileIdOption
};
fileShowCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var fileId = parseResult.GetValue(fileIdOption);
    await ShowFile(
        account!,
        fileId);
});

var fileDeleteCommand = new Command("delete", "Delete an Azure OpenAI file")
{
    accountOption,
    fileIdOption,
    idsOption
};
AddMutuallyExclusiveValidator<string, string[]>(
    fileDeleteCommand,
    true,
    fileIdOption,
    idsOption);
fileDeleteCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var fileId = parseResult.GetValue(fileIdOption);
    var ids = parseResult.GetValue(idsOption);
    await DeleteFile(
        account!,
        fileId,
        ids);
});

var threadShowCommand = new Command("show", "Show the details of an Azure OpenAI assistant thread")
{
    accountOption,
    threadIdOption
};
threadShowCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var assistantId = parseResult.GetValue(assistantIdOption);
    var threadId = parseResult.GetValue(threadIdOption);
    await ShowThread(
        account!,
        threadId!);
});

var threadDeleteCommand = new Command("delete", "Delete an Azure OpenAI assistant thread")
{
    accountOption,
    threadIdOption,
    idsOption
};
AddMutuallyExclusiveValidator<string, string[]>(
    threadDeleteCommand,
    true,
    threadIdOption,
    idsOption);
threadDeleteCommand.SetAction(async parseResult =>
{
    var account = parseResult.GetValue(accountOption);
    var threadId = parseResult.GetValue(threadIdOption);
    var ids = parseResult.GetValue(idsOption);
    await DeleteThread(
        account!,
        threadId,
        ids);
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

rootCommand.Subcommands.Add(fileCommand);
fileCommand.Subcommands.Add(fileShowCommand);
fileCommand.Subcommands.Add(fileDeleteCommand);

rootCommand.Subcommands.Add(threadCommand);
threadCommand.Subcommands.Add(threadShowCommand);
threadCommand.Subcommands.Add(threadDeleteCommand);

ParseResult parseResult = rootCommand.Parse(args);
return parseResult.Invoke();


static void AddMutuallyExclusiveValidator<T1, T2>(
    Command cmd,
    bool requireExactlyOne,
    Option<T1> option1,
    Option<T2> option2)
{
    cmd.Validators.Add(ctx =>
    {
        int count = 0;
        if (ctx.GetValue<T1>(option1) is not null) count++;
        if (ctx.GetValue<T2>(option2) is not null) count++;
        if (count > 1)
            ctx.AddError($"Options {option1.Aliases.First()} and {option2.Aliases.First()} are mutually exclusive.");
        else if (requireExactlyOne && count == 0)
            ctx.AddError($"Specify exactly one of {option1.Aliases.First()} and {option2.Aliases.First()}.");
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
    string? vectorStoreId,
    string[]? ids)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();

        if (ids is not null)
        {
            var results = new Dictionary<string, string>();
            foreach (var id in ids)
            {
                try
                {
                    var clientResult = await vectorStoreClient.DeleteVectorStoreAsync(id);
                    if (!clientResult.Value.Deleted)
                        results[id] = "NotDeleted";
                    else
                        results[id] = "Deleted";
                }
                catch (ClientResultException ex) when (ex.Status == 404)
                {
                    results[id] = "NotFound";
                }
            }
            Console.WriteLine(
                JsonSerializer.Serialize(results));
        }
        else
        {
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

async Task ShowFile(
    string account,
    string fileId)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var fileClient = azureOpenAIClient.GetOpenAIFileClient();
        var clientResult = await fileClient.GetFileAsync(fileId);
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

async Task DeleteFile(
    string account,
    string? fileId,
    string[]? ids)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var fileClient = azureOpenAIClient.GetOpenAIFileClient();

        if (ids is not null)
        {
            var results = new Dictionary<string, string>();
            foreach (var id in ids)
            {
                try
                {
                    var clientResult = await fileClient.DeleteFileAsync(id);
                    if (!clientResult.Value.Deleted)
                        results[id] = "NotDeleted";
                    else
                        results[id] = "Deleted";
                }
                catch (ClientResultException ex) when (ex.Status == 404)
                {
                    results[id] = "NotFound";
                }
            }
            Console.WriteLine(
                JsonSerializer.Serialize(results));
        }
        else
        {
            var clientResult = await fileClient.DeleteFileAsync(fileId);

            if (!clientResult.Value.Deleted)
                Console.WriteLine(
                    JsonSerializer.Serialize(
                    new
                    {
                        success = false,
                        error = "Failed to delete file."
                    }));
            else
                Console.WriteLine(JsonSerializer.Serialize(
                    new
                    {
                        success = true
                    }));
        }
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

async Task ShowThread(
    string account,
    string threadId)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var assistantClient = azureOpenAIClient.GetAssistantClient();
        var clientResult = await assistantClient.GetThreadAsync(threadId);
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

async Task DeleteThread(
    string account,
    string? threadId,
    string[]? ids)
{
    try
    {
        var azureOpenAIClient = new AzureOpenAIClient(new Uri($"https://{account}.openai.azure.com/"), new AzureCliCredential());
        var assistantClient = azureOpenAIClient.GetAssistantClient();

        if (ids is not null)
        {
            var results = new Dictionary<string, string>();
            foreach (var id in ids)
            {
                try
                {
                    var clientResult = await assistantClient.DeleteThreadAsync(id);
                    if (!clientResult.Value.Deleted)
                        results[id] = "NotDeleted";
                    else
                        results[id] = "Deleted";
                }
                catch (ClientResultException ex) when (ex.Status == 404)
                {
                    results[id] = "NotFound";
                }
            }
            Console.WriteLine(
                JsonSerializer.Serialize(results));
        }
        else
        {
            var clientResult = await assistantClient.DeleteThreadAsync(threadId);

            if (!clientResult.Value.Deleted)
                Console.WriteLine(
                    JsonSerializer.Serialize(
                    new
                    {
                        success = false,
                        error = "Failed to delete thread."
                    }));
            else
                Console.WriteLine(JsonSerializer.Serialize(
                    new
                    {
                        success = true
                    }));
        }
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
