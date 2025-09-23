# Create an AI model in the Management Portal

This guide explains how to register a new AI model in the FoundationaLLM Management Portal so it can be reused across agents and workloads.

The guide describes the configuration needed for the most common models, as well as the steps to follow in general for other models.

## Prerequisites

- You can sign in to the Management Portal for your FoundationaLLM deployment.
- You have already configured at least one AI model endpoint (see [How to create an API endpoint](create-api-endpoint.md)).
- Your account is assigned a role that can manage models (such as **Owner** or **Contributor** on the FoundationaLLM instance).
- You know the deployment name, version, and any default parameters for the target model.

## Create a New AI Model
1. **Sign in to the Management Portal.**
   - Open the Management Portal URL for your deployment and authenticate with an account that has permission to manage models.
2. **Open the AI Models workspace.**
   - In the left navigation pane, select **AI Models** to view the list of existing model registrations.
3. **Start creating a new model.**
   - Select **+ Create Model** to open the create flow.
4. **Provide a model display name.**
   - In **What would you like the model display name to be?**, enter a unique name. Only letters, numbers, dashes, and underscores are accepted. The portal automatically validates that the name is available before you continue.
   - Continue with the steps in the section that follows that is most appropriate to your model.


## General Step-by-step instructions
1. **Select the model type.**
   - Under **What is the model type?**, choose the option that matches the capability of the underlying model:
     - **Basic** – for simple chat or text models without specialized behaviors.
     - **Embedding** – for embedding/vector generation models.
     - **Completion** – for text completion or instruction-following models.
     - **ImageGeneration** – for models that produce images.
2. **Choose the model endpoint.**
   - Under **What is the model endpoint?**, pick the endpoint configuration that routes calls to the hosting service. Only endpoints whose subcategory is **AIModel** are listed.
3. **Enter the deployment name.**
   - In **What is the model deployment name?**, type the deployment or model identifier used by the hosting service. Include the full name the service expects (for example, `gpt-4o-mini` or `modelName:version`).
4. **Enter the model version.**
   - In **What is the model version?**, provide the version string that corresponds to the deployment you selected.
5. **Configure default model parameters (optional).**
   - Under **What are the model parameters?**, use the property builder to add key-value pairs for any defaults you want to apply when the model is invoked (such as temperature, top_p, or max_tokens).
6. **Create the model.**
    - Select **Create Model** to save the registration. The portal displays a success notification and returns you to the Models list.

## Registering an Azure AI Foundry or Azure OpenAI GPT model

The following steps apply to registering models like `GPT-4o`, `GPT-5`, `DALL-e-3` or `text-embedding-3-large`

1. **Select the model type.**
   - Under **What is the model type?**, for GPT models used for chat completion you should select **Completion**. Alternatively, for embedding models, select **Embedding** and for image generation models select **ImageGeneration**.
2. **Choose the model endpoint.**
   - Under **What is the model endpoint?**, select the endpoint configuration that routes calls to the your deployed Azure AI Foundry or Azure OpenAI service endpoint. 
3. **Enter the deployment name.**
   - In **What is the model deployment name?**, type the deployment name used in Azure. Be sure to use the deployment name and not the model name.
4. **Enter the model version.**
   - In **What is the model version?**, you can leave this at `0.0` as it is not used.
5. **Configure default model parameters (optional).**
   - Under **What are the model parameters?**, use the property builder to add key-value pairs for any defaults you want to apply when the model is invoked (such as temperature, top_p, or max_tokens).
6. **Create the model.**
    - Select **Create Model** to save the registration. The portal displays a success notification and returns you to the Models list.

## Registering an Azure AI Foundry Hosted model

The following steps apply to registering models like `Phi-4`or `Llama-3` that are hosted by Azure AI Foundry serverless or accessed via the Azure AI Foundry Inference API.

1. **Select the model type.**
   - Under **What is the model type?**, for models used for chat completion you should select **Completion**.
2. **Choose the model endpoint.**
   - Under **What is the model endpoint?**, select the endpoint configuration that routes calls to the your deployed Azure AI Foundry service endpoint. 
3. **Enter the deployment name.**
   - In **What is the model deployment name?**, type the deployment name used in Azure. Be sure to use the deployment name and not the model name.
4. **Enter the model version.**
   - In **What is the model version?**, set this to the Model version as used in your deployment.
5. **Configure default model parameters (optional).**
   - Under **What are the model parameters?**, use the property builder to add key-value pairs for any defaults you want to apply when the model is invoked.
6. **Create the model.**
    - Select **Create Model** to save the registration. The portal displays a success notification and returns you to the Models list.

## Registering a Google Vertex Hosted Gemini model

The following steps apply to registering models like `Gemini 2.5 pro` or `Gemini 2.5 Flash`.

1. **Provide a model display name.**
   - In **What would you like the model display name to be?**, enter a unique name. Only letters, numbers, dashes, and underscores are accepted. The portal automatically validates that the name is available before you continue.
2. **Select the model type.**
   - Under **What is the model type?**, for Gemini models used for chat completion you should select **Completion**.
3. **Choose the model endpoint.**
   - Under **What is the model endpoint?**, select the endpoint configuration that routes calls to the your deployed Google Vertex AI service endpoint. 
4. **Enter the deployment name.**
   - In **What is the model deployment name?**, type the model name used in Google Vertex AI. Be sure to use the model name (e.g., `gemini-2.5-pro`).
5. **Enter the model version.**
   - In **What is the model version?**, you can leave this at `0.0` as it is not used.
6. **Configure default model parameters (optional).**
   - Under **What are the model parameters?**, use the property builder to add key-value pairs for any defaults you want to apply when the model is invoked.
7. **Create the model.**
    - Select **Create Model** to save the registration. The portal displays a success notification and returns you to the Models list.

## Registering an AWS Bedrock Hosted Anthropic Claude model

The following steps apply to registering models like `Claude 3.5 Sonnet` or `Claude 4.0`.

1. **Sign in to the Management Portal.**
   - Open the Management Portal URL for your deployment and authenticate with an account that has permission to manage models.
2. **Open the Models workspace.**
   - In the left navigation pane, select **AI Models** to view the list of existing model registrations.
3. **Start creating a new model.**
   - Select **+ Create Model** to open the create flow.
4. **Provide a model display name.**
   - In **What would you like the model display name to be?**, enter a unique name. Only letters, numbers, dashes, and underscores are accepted. The portal automatically validates that the name is available before you continue.
5. **Select the model type.**
   - Under **What is the model type?**, for Claude models used for chat completion you should select **Completion**.
6. **Choose the model endpoint.**
   - Under **What is the model endpoint?**, select the endpoint configuration that routes calls to the your deployed AWS Bedrock service endpoint. 
7. **Enter the deployment name.**
   - In **What is the model deployment name?**, type the model name used in AWS Bedrock. Be sure to use the model name (e.g., `us.anthropic.claude-3-5-sonnet-20241022-v2:0`).
8. **Enter the model version.**
   - In **What is the model version?**, you can leave this at `0.0` as it is not used.
9. **Configure default model parameters (optional).**
   - Under **What are the model parameters?**, use the property builder to add key-value pairs for any defaults you want to apply when the model is invoked.
10. **Create the model.**
    - Select **Create Model** to save the registration. The portal displays a success notification and returns you to the Models list.

## Next steps

- Attach the model to an agent profile so conversations can call it.
