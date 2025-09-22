# Create an API endpoint in the Management Portal

This step-by-step guide walks through creating a new API endpoint in the Management Portal. Follow these instructions when you need to register an external model endpoint that agents can call through FoundationaLLM.

## Prerequisites

- You have access to a FoundationaLLM environment that includes the Management Portal.
- You are assigned a role that allows managing API endpoints (for example, **FoundationaLLM Owner** or **FoundationaLLM Contributor**).
- You know the connection details for the target API, including its base URL, version, timeout requirements, and any authentication secrets.

## Step-by-step instructions

1. **Sign in to the Management Portal.**
   - Open the portal URL for your deployment and sign in with an account that has the required permissions.
2. **Open the API Endpoints workspace.**
   - In the left navigation pane, select **API Endpoints** to view the endpoints that agents can use.
3. **Start creating a new API endpoint.**
   - Select **+ Create API Endpoint**.
4. **Provide a unique API endpoint name.**
   - Under **What is the API endpoint name?**, enter a name that uses only letters, numbers, dashes, and underscores. Spaces and other special characters are not allowed.
   - As you type, the portal sanitizes invalid characters and checks whether the name is available. A ✔️ icon indicates that the name can be used, while ❌ indicates a conflict you must resolve before saving.
5. **Describe the endpoint.**
   - Under **What are the endpoint details?**, provide a description that helps other administrators understand the endpoint’s purpose.
6. **Select the service that provides the model.**
   - Choose one of the available providers in the **What service provides the model?** drop-down. Options include:
     - **Azure AI Inference API** (`azureai`)
     - **Microsoft** (`microsoft`)
     - **Amazon Bedrock** (`bedrock`)
     - **Google VertexAI** (`vertexai`)
   - Use the provider that matches the API host you are connecting to.
7. **Choose a category and subcategory.**
   - Pick a value in the **Category** drop-down. Options include **Orchestration**, **ExternalOrchestration**, **LLM**, **Gatekeeper**, **AzureAIDirect**, **AzureOpenAIDirect**, **FileStoreConnector**, and **General**.
   - Optionally set a **Subcategory**. Choices include **None**, **OneDriveWorkSchool**, **Indexing**, and **AIModel**. Leave it as **None** when the endpoint does not require an additional grouping.
8. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to the authentication mechanism the endpoint expects. The portal currently supports **API Key** and **Azure Identity**.
9. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint.
     - **API Version** – The version string required by the service (if any).
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out.
     - **Status Endpoint (relative path)** – Optional path the platform can call to monitor endpoint health.
10. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, use the Authentication Parameters builder to add the key-value pairs the endpoint requires (for example, API key header names or Azure Identity scopes). The available fields change based on the selected auth type.
11. **Manage URL exceptions (optional).**
    - In the **URL exceptions** table, select **Add URL Exception** to allow specific URLs to bypass default restrictions. For each exception, provide the URL, the user principal name that is allowed to use it, and whether the exception is enabled. You can edit or delete existing entries using the **Edit** and **Delete** icons in the table.
12. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. Select **Cancel** if you decide not to continue.

## Next steps

- Assign the API endpoint to agents or workflows that need to call the registered service.
- Monitor the endpoint’s usage and health through the Management Portal and update URL exceptions or authentication parameters as required.
