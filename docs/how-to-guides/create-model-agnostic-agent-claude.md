# Overview

In this step-by-step guide, you will create a model agnostic agent using Claude, a code interpreter tool that uses Python custom containers and a knowledge search tool that uses the uploaded files as a data source.

# Creating a model agnostic agent

1. Navigate to the **Management Portal**.
2. Select **Create New Agent** from the menu bar.
3. In the Agent Name provide a unique name for the agent.
4. In the Agent Display Name provide a user friendly name for the agent.
5. In the Description, provide a description of the agent.
6. Under User Portal Experience, change **Would you like to allow the uder to upload files?** to `Yes`.
6. In the Knowledge Source section, under does this agent have an inline context, select `Yes`.
7. In the Workflow section, and provide the following values:
- What workflow should the agent use? Select `ExternalAgentWorkflow` in the drop down.
- Workflow name: `MAA-Workflow`
- Workflow package name: `foundationallm_agent_plugins`
- Workflow class name:  `FoundationaLLMFunctionCallingWorkflow`
- Workflow host: select `LangChain`
- Workflow main model: select a Claude based model from the list.
8. Under workflow main model parameters, select **Add Property** and in the dialog that appears provide these values:
- Property Key: `temperature`
- Property Type: `number`
- Property Value: `0.5` 
9. Select Save to create the property.
10. Under what is the main workflow prompt, copy and paste the following:
``` 
You are a helpful assistant that answers questions in a polite way.

{{foundationallm:router_prompt}}

{{files_prompt}}
```

## Create the Workflow Prompts
Open a new browser window to the Management Portal. Select **Prompts**.

First you will create the main workflow prompts.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Workflow-Files`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides instructions that are specific for the identification of the files that are relevant to the question.`
- Category: `Workflow`
- Prompt Prefix: Copy and paste the following:
```
You should determine the files relevant to the question and use them to answer the question. If the question does not refer to any files, you can answer directly without using any files.
Always attempt to identify the relevant files and pass their names as parameters to the selected tools.

You have access to the files listed below in the CONVERSATION_FILES and ATTACHED_FILES sections.
If a section is empty, it means there are no files in that section.
The CONVERSATION_FILES section contains files that are part of the conversation.
The ATTACHED_FILES section contains files that are attached to the question.

CONVERSATION_FILES:
{{conversation_files}}
END_CONVERSATION_FILES

ATTACHED_FILES:
{{attached_files}}
END_ATTACHED_FILES

If the question refers to one or more files and no file names are specified, follow these steps:

1. If there are files in the ATTACHED_FILES section, use them to answer the question.
2. If there are no files in the ATTACHED_FILES section, attempt to use the files in the CONVERSATION_FILES section to answer the question.
3. If there are no files in either section, answer the question directly without using any files.
```
3. Select **Create Prompt**

Next, create the Workflow Final prompt.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Workflow-Final`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides instructions to build the final response based on the results provided by tools.`
- Category: `Workflow`
- Prompt Prefix: Copy and paste the following:
```
Respond to the question specified in the QUESTION section.
Always assume that you have already completed all the tasks resulting from the question.
You have access to the additional information specified in the TOOL_RESULTS section.

QUESTION:
{{prompt}}
END_QUESTION

TOOL_RESULTS:
{{tool_results}}
END_TOOL_RESULTS

When producing your response, follow these rules:

- Do not repeat the instructions from the question.
- Use the information provided in the TOOL_RESULTS section.
- If the TOOL_RESULTS section is empty, attempt to respond directly.

```
3. Select **Create Prompt**

Next, create the Workflow Router prompt.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Workflow-Router`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides instructions that are specific for the selection of tools.`
- Category: `Workflow`
- Prompt Prefix: Copy and paste the following:
```
You must always answer based on the information you have, using the available tools.
Do not ask the user for more information or clarification.
If you do not know the answer, say "I don't know" or "I don't have that information."

You can use one of the following tools or respond directly:
{{foundationallm:tool_list}}

Additional instructions for the usage of the tools:

{{foundationallm:tool_router_prompts}}

```
3. Select **Create Prompt**

## Create the Tool Prompts
First, create the Tool Code Main prompt.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Tool-Code-Main`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides the main instructions for the tool.`
- Category: `Tool`
- Prompt Prefix: Copy and paste the following:
```
You are an expert Python programmer. Write clear, correct, and efficient Python code.

- Use idiomatic Python style (PEP 8).
- Do not add comments or anything else than the Python source code.
- Ensure the code is ready to run as-is, without any extra characters or details.
- Never use the **show()** function when generating code that uses the **matplotlib** library. Always use use **savefig()** instead of **show()**.
- Always assume files are loaded and saved from the '/mnt/data/' directory.
- When referring to existing files, always ensure the names match the names from the AVAILABLE_FILE_NAMES section

AVAILABLE_FILE_NAMES:
{{file_names}}
END_AVAILABLE_FILE_NAMES
```
3. Select **Create Prompt**


Next, create the Tool Code Router prompt.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Tool-Code-Router`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides additional instructions for the selection of this tool.`
- Category: `Tool`
- Prompt Prefix: Copy and paste the following:
```
**Code-01**: Answers questions that require dynamic generation of code. Always use the original question to invoke this tool. Do not generate code when calling this tool.  Use this tool to process .zip files. Use this tool to analyze .xlsx, .csv, .json, and .XML files. Use this tool when asked to generate a file of any kind.
```
3. Select **Create Prompt**

Next, create the Tool Knowledge Main prompt.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Tool-Knowledge-Main`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides the main instructions for the tool.`
- Category: `Tool`
- Prompt Prefix: Copy and paste the following:
```
Answer the question using only the context provided.

Context:
{{context}}

Question:
{{prompt}}
```
3. Select **Create Prompt**

Next, create the Tool Knowledge Router prompt.
1. Select Create Prompt.
2. Provide the values as follows:
- Prompt Name: `Your-Agent-Name-Tool-Knowledge-Router`. Replace Your-Agent-Name with the name of your agent.
- Description: `Provides the main instructions for the tool.`
- Category: `Tool`
- Prompt Prefix: Copy and paste the following:
```

**Knowledge-Conversation-Files**: This tool retrieves knowledge from document files. Use this tool to search content from .txt, .pdf, .doc, .docx, .ppt, and .pptx files.
```
3. Select **Create Prompt**

## Configure the Prompt Resources
1. Under the additional workflow resources, select `Add Workflow Resource`.
2. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Router prompt (e.g., `Your-Agent-Name--Workflow-Router`)  you previously created for this agent.
- Resource Role: Enter `router_prompt`.
3. Select Save to add the prompt resource.
4. Under the additional workflow resources, select `Add Workflow Resource`.
5. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Files prompt (e.g., `Your-Agent-Name--Workflow-Files`)  you previously created for this agent.
- Resource Role: Enter `files_prompt`.
6. Select Save to add the prompt resource.
7. Under the additional workflow resources, select `Add Workflow Resource`.
8. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Final prompt (e.g., `Your-Agent-Name--Workflow-Final`) you previously created for this agent.
- Resource Role: Enter `final_prompt`.
9. Select Save to add the prompt resource.

## Configure the Tools

First, you will add a code interpreter tool.

1. Under the Tools sections select **Add New Tool**.
2. Enter the following values:
- Tool name enter `Code-01`. This MUST be called `Code-01` to match the name used in the prompts.
- Tool description: `Answers questions that require dynamic generation of code.`
- Tool package name: `foundationallm_agent_plugins`
- Tool class name: `FoundationaLLMCodeInterpreterTool`
3. Under Tool resources, select **Add Tool Resource**. 
4. In the Add Resource dialog, provides these values:
- Resource Type: `Model`
- Resource: Select your Claude model
- Resource Role: Enter `main_model`.
5. Select Save.
6. Expand the newly created AI model object and select Add Property.
- Property Key: `model_parameters`
- Property Type: Select `Object / Array`
- Property Value: Select text and then enter the following and select Save:
```
{
  "temperature": 0.2
}
```
7. Select **Add Tool Resource**. 
8. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Code Interpreter Main prompt (e.g., `Your-Agent-Name-Tool-Code-Main`)  you previously created for this agent.
- Resource Role: Enter `main_prompt`.
9. Select **Add Tool Resource**. 
10. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Code Interpreter Router prompt (e.g., `Your-Agent-Name-Tool-Code-Router`)  you previously created for this agent.
- Resource Role: Enter `router_prompt`.
11. Select **Save**.
12. Under Tool properties, select **Add Property** and provide these values, then select Save:
- Property Key: `code_session_required`
- Property Type: `Boolean`
- Property Value: `True`
13. Under Tool properties, select **Add Property** and provide these values, then select Save:
- Property Key: `code_session_endpoint_provider`
- Property Type: `String`
- Property Value: `AzureContainerAppsCustomContainer`
14. Under Tool properties, select **Add Property** and provide these values, then select Save:
- Property Key: `code_session_language`
- Property Type: `String`
- Property Value: `Python`
15. In the Configure Tool dialog, select **Save**.


Next, you will add Knowledge Conversation Files tool.

1. Under the Tools sections select **Add New Tool**.
2. Enter the following values:
- Tool name enter `Knowledge-Conversation-Files`. This MUST be called `Knowledge-Conversation-Files` to match the name used in the prompts.
- Tool description: `Retrieves content from files uploaded to conversations.`
- Tool package name: `foundationallm_agent_plugins`
- Tool class name: `FoundationaLLMKnowledgeTool`
3. Under Tool resources, select **Add Tool Resource**. 
4. In the Add Resource dialog, provides these values:
- Resource Type: `Model`
- Resource: Select your Claude model
- Resource Role: Enter `main_model`.
5. Select Save.
6. Select **Add Tool Resource**. 
8. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Knowledge Main prompt (e.g., `Your-Agent-Name-Tool-Knowledge-Main`)  you previously created for this agent.
- Resource Role: Enter `main_prompt`.
9. Select **Add Tool Resource**. 
10. In the Add Resource dialog, provides these values:
- Resource Type: `Prompt`
- Resource: Select the Knowledge Router prompt (e.g., `Your-Agent-Name-Tool-Knowledge-Router`)  you previously created for this agent.
- Resource Role: Enter `router_prompt`.
11. Select **Save**.

12. Select **Add Tool Resource**. 
13. In the Add Resource dialog, provides these values:
- Resource Type: `Data Pipeline`
- Resource: Select `DefaultFileUpload` 
- Resource Role: Enter `file_upload_data_pipeline`.
14. Select **Save**.

15. Select **Add Tool Resource**. 
16. In the Add Resource dialog, provides these values:
- Resource Type: `Vector Database`
- Resource: Select `ConversationFiles` 
- Resource Role: Enter `vector_database`.
17. Select **Save**.

18. Under Tool properties, select **Add Property** and provide these values, then select Save:
- Property Key: `embedding_model`
- Property Type: `String`
- Property Value: `text-embedding-3-large`
19. Under Tool properties, select **Add Property** and provide these values, then select Save:
- Property Key: `embedding_dimensions`
- Property Type: `Number`
- Property Value: `2048`
20. In the Configure Tool dialog, select **Save**.

21. Select Create Agent to save the new agent.