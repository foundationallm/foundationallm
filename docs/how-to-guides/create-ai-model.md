# Create an AI model in the Management Portal

This guide explains how to register a new AI model in the FoundationaLLM Management Portal so it can be reused across agents and workloads.

## Prerequisites

- You can sign in to the Management Portal for your FoundationaLLM deployment.
- You have already configured at least one AI model endpoint (for example through **How to create an API endpoint**).
- Your account is assigned a role that can manage models (such as **FoundationaLLM Owner** or **FoundationaLLM Contributor**).
- You know the deployment name, version, and any default parameters for the target model.

## Step-by-step instructions

1. **Sign in to the Management Portal.**
   - Open the Management Portal URL for your deployment and authenticate with an account that has permission to manage models.
2. **Open the Models workspace.**
   - In the left navigation pane, select **Models** to view the list of existing model registrations.
3. **Start creating a new model.**
   - Select **+ Create Model** to open the create flow.
4. **Provide a model display name.**
   - In **What would you like the model display name to be?**, enter a unique name. Only letters, numbers, dashes, and underscores are accepted. The portal automatically validates that the name is available before you continue.
5. **Select the model type.**
   - Under **What is the model type?**, choose the option that matches the capability of the underlying model:
     - **Basic** – for simple chat or text models without specialized behaviors.
     - **Embedding** – for embedding/vector generation models.
     - **Completion** – for text completion or instruction-following models.
     - **ImageGeneration** – for models that produce images.
6. **Choose the model endpoint.**
   - Under **What is the model endpoint?**, pick the endpoint configuration that routes calls to the hosting service. Only endpoints whose subcategory is **AIModel** are listed.
7. **Enter the deployment name.**
   - In **What is the model deployment name?**, type the deployment or model identifier used by the hosting service. Include the full name the service expects (for example, `gpt-4o-mini` or `modelName:version`).
8. **Enter the model version.**
   - In **What is the model version?**, provide the version string that corresponds to the deployment you selected.
9. **Configure default model parameters (optional).**
   - Under **What are the model parameters?**, use the property builder to add key-value pairs for any defaults you want to apply when the model is invoked (such as temperature, top_p, or max_tokens).
10. **Create the model.**
    - Select **Create Model** to save the registration. The portal displays a success notification and returns you to the Models list.

## Next steps

- Attach the model to an agent profile so conversations can call it.
- Share the model name with other team members so they can reuse the same configuration.
