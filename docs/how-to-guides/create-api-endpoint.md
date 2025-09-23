# Create an API endpoint in the Management Portal

This step-by-step guide walks through creating a new API endpoint in the Management Portal. The API endpoint encapsulates the target URI of a service that provides access to items such as model deployments, OneDrive, and vector indexes, and Orchestrations API's. Follow these instructions when you need to register an external model endpoint that agents can call through FoundationaLLM.

## Prerequisites

- You have access to a FoundationaLLM environment that includes the Management Portal.
- You are assigned a role that allows managing API endpoints (**Owner** or **Contributor** on the FoundationaLLM instance).
- You know the connection details for the target API, including its base URL, version, timeout requirements, and any authentication secrets.

## Create a New API Endpoint

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

Follow the intructions from the section that follows that is most applicable to your scenario.


## General Instructions for Creating an API Endpoint
1. **Select the service that provides the model.**
   - Choose one of the available providers in the **What service provides the model?** drop-down. Options include:
     - **Azure AI Inference API** (`azureai`)
     - **Microsoft** (`microsoft`)
     - **Amazon Bedrock** (`bedrock`)
     - **Google VertexAI** (`vertexai`)
   - Use the provider that matches the API host you are connecting to.
2. **Choose a category and subcategory.**
   - Pick a value in the **Category** drop-down. Options include **Orchestration**, **ExternalOrchestration**, **LLM**, **Gatekeeper**, **AzureAIDirect**, **AzureOpenAIDirect**, **FileStoreConnector**, and **General**.
   - Optionally set a **Subcategory**. Choices include **None**, **OneDriveWorkSchool**, **Indexing**, and **AIModel**. Leave it as **None** when the endpoint does not require an additional grouping.
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to the authentication mechanism the endpoint expects. The portal currently supports **API Key** and **Azure Identity**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint.
     - **API Version** – The version string required by the service (if any).
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out.
     - **Status Endpoint (relative path)** – Optional path the platform can call to monitor endpoint health.
5. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, use the Authentication Parameters builder to add the key-value pairs the endpoint requires (for example, API key header names or Azure Identity scopes). The available fields change based on the selected auth type.
6. **Manage URL exceptions (optional).**
    - In the **URL exceptions** table, select **Add URL Exception** to allow specific URLs to bypass default restrictions. For each exception, provide the URL, the user principal name that is allowed to use it, and whether the exception is enabled. You can edit or delete existing entries using the **Edit** and **Delete** icons in the table.
7. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Creating an API Endpoint for Azure OpenAI in AI Foundry with Azure Identity authentication
1. **Select the service that provides the model.**
   - Choose **Microsoft** from the list of available providers in the **What service provides the model?** drop-down. 
2. **Choose a category and subcategory.**
   - Select the value **General** in the **Category** drop-down. 
   - Set the **Subcategory** to **AIModel**. 
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to **Azure Identity**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint (e.g., `https://aiservice-l42jljz2i5ox6382171035213.openai.azure.com/`).
     - **API Version** – The API version string required by the service (e.g., `2024-12-01-preview`).
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out (e.g., `60`).
5. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Creating an API Endpoint for Azure OpenAI with API Key authentication
1. **Select the service that provides the model.**
   - Choose **Microsoft** from the list of available providers in the **What service provides the model?** drop-down. 
2. **Choose a category and subcategory.**
   - Select the value **General** in the **Category** drop-down. 
   - Set the **Subcategory** to **AIModel**. 
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to **API Key**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint (e.g., `https://openai-z42jlmq2i5ox6.openai.azure.com/`).
     - **API Version** – The API version string required by the service (e.g., `2024-12-01-preview`).
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out (e.g., `60`).
5. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, select Add Authentication Parameter and add the following key-value pair the endpoint requires:
    - Parameter Key: `api_key_configuration_name`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the API Key value(e.g., `FoundationaLLM:APIEndpoints:AzureOpenAI:Essentials:APIKey`)
    - Parameter Key: `api_key_header_name`
    - Parameter Value: `api-key`
5. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Creating an API Endpoint for Azure AI Foundry with API Key authentication
1. **Select the service that provides the model.**
   - Choose **Microsoft** from the list of available providers in the **What service provides the model?** drop-down. 
2. **Choose a category and subcategory.**
   - Select the value **General** in the **Category** drop-down. 
   - Set the **Subcategory** to **AIModel**. 
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to **API Key**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint (e.g., `https://aiservice-l42jljz2i5ox6382171035213.openai.azure.com/`).
     - **API Version** – The API version string required by the service (e.g., `2024-12-01-preview`).
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out (e.g., `60`).
5. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, select Add Authentication Parameter and add the following key-value pair the endpoint requires:
    - Parameter Key: `api_key_configuration_name`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the API Key value (e.g., `FoundationaLLM:APIEndpoints:AzureAIFoundry-Phi4Serverless:Essentials:APIKey`)
5. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Creating an API Endpoint for AWS Bedrock with API Key authentication
1. **Select the service that provides the model.**
   - Choose **Amazon Bedrock** from the list of available providers in the **What service provides the model?** drop-down. 
2. **Choose a category and subcategory.**
   - Select the value **General** in the **Category** drop-down. 
   - Set the **Subcategory** to **AIModel**. 
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to **API Key**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint (e.g., `https://bedrock-runtime.us-east-1.amazonaws.com/`).
     - **API Version** – The API version should be left blank.
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out (e.g., `120`).
5. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, select Add Authentication Parameter and add the following key-value pair the endpoint requires:
    - Parameter Key: `access_key`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the access key value (e.g., `FoundationaLLM:APIEndpoints:AmazonBedrock:Essentials:AccessKey`)
    - Parameter Key: `secret_key`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the secret key value (e.g., `FoundationaLLM:APIEndpoints:AmazonBedrock:Essentials:SecretKey`)
5. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Creating an API Endpoint for AWS Bedrock with Azure Identity authentication
1. **Select the service that provides the model.**
   - Choose **Amazon Bedrock** from the list of available providers in the **What service provides the model?** drop-down. 
2. **Choose a category and subcategory.**
   - Select the value **General** in the **Category** drop-down. 
   - Set the **Subcategory** to **AIModel**. 
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to **Azure Identity**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – The full base URL for the API endpoint (e.g., `https://bedrock-runtime.us-east-1.amazonaws.com/`).
     - **API Version** – The API version should be left blank.
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out (e.g., `120`).
5. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, select Add Authentication Parameter and add the following key-value pair the endpoint requires:
    - Parameter Key: `scope`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the scope value (e.g., `FoundationaLLM:APIEndpoints:AmazonBedrock:Essentials:Scope`)
    - Parameter Key: `role_arn`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the role ARN value (e.g., `FoundationaLLM:APIEndpoints:AmazonBedrock:Essentials:RoleARN`)
5. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Creating an API Endpoint for Google Vertex with API Key authentication
1. **Select the service that provides the model.**
   - Choose **Amazon Bedrock** from the list of available providers in the **What service provides the model?** drop-down. 
2. **Choose a category and subcategory.**
   - Select the value **General** in the **Category** drop-down. 
   - Set the **Subcategory** to **AIModel**. 
3. **Specify the connection type.**
   - Under **What is the connection type?**, set **Auth Type** to **API Key**.
4. **Enter connection details.**
   - Under **What are the connection details?**, provide the following:
     - **Endpoint URL** – Enter a placholder URL (e.g., `https://www.google.com`).
     - **API Version** – The API version should be left blank.
     - **Timeout (seconds)** – The maximum number of seconds to wait for a response before timing out (e.g., `120`).
5. **Configure authentication parameters.**
    - In **What are the authentication parameters?**, select Add Authentication Parameter and add the following key-value pair the endpoint requires:
    - Parameter Key: `service_account_credentials`
    - Parameter Value: the App Configuration key containing the name of the key in Key Vault containing the Vertext service account credentials value (e.g., `FoundationaLLM:APIEndpoints:GoogleVertexAI:Configuration:ServiceAccount`)
5. **Create the endpoint.**
    - After verifying all fields, select **Create API Endpoint** to save the configuration. 

## Next steps

- Assign the API endpoint to models or workflows that need to call the registered service.
