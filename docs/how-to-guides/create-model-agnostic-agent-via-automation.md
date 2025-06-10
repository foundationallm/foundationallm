# Overview

In this step-by-step guide, you will create a model agnostic agent via automation. These agents can use Claude or GPT models, a code interpreter tool that uses Python dynamic sessions or custom containers and a knowledge search tool that uses the uploaded files as a data source.

# Pre-requisites
- [PowerShell 7 or higher](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.5) installed on your machine. 
- Install the FoundationaLLM.Core module by running the following command in PowerShell: `Install-Module FoundationaLLM.Core`
- Within PowerShell, login in with your credentials to the Azure Subscription containing FoundationaLLM by running: `az login`, selecting your user, and then the subscription.

# Clone the FoundationaLLM-Packages repo
1. Clone the rep https://github.com/foundationallm/foundationallm-packages to your local machine.
2. Open the FoundationaLLM-Packages folder in Visual Studio Code. 
3. Navigate to the ModelAgnosticAgent subfolder and open deploy.ps1. This script file has several example Model Agnostic Agents you can deploy. Before you deploy any of them, however, you should update their settings as the following section shows.

# Configuring a model agnostic agent
1. Update the InstanceId, CoreAPIBaseUrl and ManagementAPIBaseURL with the values from your FoundationaLLM instance. You can get all of these values from the Management Portal, by selecting Deployment Information. These values should appear like:
```
$global:InstanceId = "8ac6174c-bede-43cb-a140-ec0002d96d2b"
$global:CoreAPIBaseUrl = "https://cacoreapil45jljq2i5ox6.lemonrock-a0804c39.eastus2.azurecontainerapps.io"
$global:ManagementAPIBaseUrl = "https://camanagementapil43jljq2i5ox6.lemonrock-a0804c39.eastus2.azurecontainerapps.io"
```
2. In the deploy1.ps, take a look at the first code block that invokes the `Deploy-FoundationaLLMPackage` command. It should look similar to the following:
```
Deploy-FoundationaLLMPackage `
    -PackageRoot "./ModelAgnosticAgent" `
    -Parameters @{
        "AGENT_NAME" = "MAA-01"
        "AGENT_DISPLAY_NAME" = "Model Agnostic Agent 01"
        "AGENT_DESCRIPTION" = "Model agnostic agent using GPT-4o and Python dynamic sessions."
        "AGENT_TOOLS" = @("Code-01", "Knowledge-Conversation-Files")
        "MAIN_LLM" = "GPT4oCompletionAIModel"
        "VIRTUAL_SECURITY_GROUP_ID" = "8637cb25-0fff-4717-8e1c-4a44ff3c3514"
        "CODE_SESSION_ENDPOINT_PROVIDER" = "AzureContainerAppsCodeInterpreter"
    }
```
3. You need to update several of the parameters as follows:
- AGENT_NAME: Set this to a unique name for the agent. Remember this name cannot have any spaces or special characters except the dash (-).
- AGENT_DISPLAY_NAME: Set this to the user friendly name of the agent.
- AGENT_DESCRIPTION: Provide a description for the agent.
- AGENT_TOOLS: Leave unchanged to enable both Code Interpreter and Knowledge Search over uploaded files.
- MAIN_LLM: Set this to the name of one of the GPT or Claude models registered with FoundationaLLM.
- VIRTUAL_SECURITY_GROUP_ID: Using your PowerShell terminal, run the command `new-guid` and replace the existing GUID with your newly generated value.
- CODE_SESSION_ENDPOINT_PROVIDER: Set the value to "AzureContainerAppsCodeInterpreter" to use Dynamic sessions, or "AzureContainerAppsCustomContainer" to use the configured custom container in your FoundationaLLM environment. 

# Deploying an agent
1. Delete all of the other Deploy-FoundatinaLLMPackage commands from the script except those you have customized and want to deploy.
2. Select all of the contents of deploy.ps1. Right click in the window and select Run Selection. 
3. The script should complete without error in a few moments.
4. Navigate to the FoundationaLLM User Portal and refresh the page to see the updated list of agents. You should see your new agent listed. At this point you can try it out by prompting it like any other agent.

