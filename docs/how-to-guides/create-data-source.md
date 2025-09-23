# Create a data source in the Management Portal

This step-by-step guide walks through creating a new data source for FoundationaLLM by using the Management Portal. Follow these instructions when you need to register a storage location that agents can index and query.

## Prerequisites

- You have a FoundationaLLM environment with access to the Management Portal.
- You have the data source connection details handy, such as the Azure Storage account, container, and any authentication secrets.
- You are assigned a role that allows managing data sources (**Owner** or **Contributor** on the FoundationaLLM instance).

## Step-by-step instructions

1. **Sign in to the Management Portal.**
   - Open the portal URL for your deployment and sign in with an account that has the required role assignments.
2. **Open the Data Sources workspace.**
   - In the left navigation pane, select **Data Sources** to see the existing connections that agents can use.
3. **Start creating a new data source.**
   - Select **+ Create Data Source**. 
4. **Provide a data source name and description**
   - Under **data source name**, provide a unique name for the data source. Note that you cannot use special characters or spaces and can only use letters and numbers with dashes and underscores.
   - Under **data description**, provide a user friendly description of what this data source represents.
5. **Select the data source type**
   - Under what is the type of data source? select the desired data source type and then continue with instructions appropriate to that type. From this point on, the steps will vary based on the type of data source selected. The main data source types are covered in the sections that follows.

### Creating an Azure Data Lake data source

1. Select **Azure Data Lake** in the data source type drop down.  
2. **Select Authentication Type**
   - For Authentication Type of **Connection String** provide the following:

      - Connection String: the full connection string to the container as retrieved from the Azure Portal.
      - Folder(s): a comma seperated list of container and sub-folder names (e.g., `container\myfolder` or `container\myfolder\subfolder`). 
      - Cost center: provide a cost center value if you'd like to assign this data source to a cost center.
   - For Authentication Type of **Account Key** provide the following:
      - API Key: provide the API key to endpoint.
      - Endpoint: provide the URL to the endpoint.
      - Folder(s): a comma seperated list of folder names underneath the root container to include. 
      - Cost center: provide a cost center value if you'd like to assign this data source to a cost center.
   - For Authentication Type of **Azure Identity** provide the following:
      - Account name: the name of the Azure Data Lake account.
      - Folder(s): a comma seperated list of folder names underneath the root container to include. 
      - Cost center: provide a cost center value if you'd like to assign this data source to a cost center.
3. Select **Create Data Source** to save the new data source. 

### Creating a OneLake data source

1. Select **OneLake** in the data source type drop down.  
2. **Select Authentication Type**
   - For Authentication Type, only **Azure Identity** is supported provide the following:
      - Account name: the name of the Azure Data Lake account (e.g., `onelake`).
      - Workpace(s): a comma seperated list of workspace names in Fabric. 
      - Cost center: provide a cost center value if you'd like to assign this data source to a cost center.
3. Select **Create Data Source** to save the new data source. 

### Creating an Azure SQL Database data source

1. Select **Azure SQL Database** in the data source type drop down.  
2. Provide the following:
      - Connection String: the full connection string to the SQL database.
      - Table Name(s): an optional comma seperated list of table names that tools may choose to reference when generating SQL. 
      - Cost center: provide a cost center value if you'd like to assign this data source to a cost center.
3. Select **Create Data Source** to save the new data source. 

### Creating a SharePoint List data source
1. Follow these steps to create an [Entra ID App Registration and Certificate](create-entra-app-registration.md) and retrieve the values you will need for the next steps.
2. Select **SharePoint List** in the data source type drop down.  
3. Provide the following:
      - App ID: App ID (also referred to as a client ID) created in Entra.
      - Tenant ID: the tenant ID of the Entra tenant. 
      - Certificate Name: The name of the self-signed certificate.
      - Key Vault URL: The key vault URL.
      - Site URL: The SharePoint site URL.
      - Document Library: a comma seperated list of document libraries (used to restrict access to the subset of document libraries). 
      - Cost center: provide a cost center value if you'd like to assign this data source to a cost center.
3. Select **Create Data Source** to save the new data source. 

## Next steps

- Attach the new data source to an agent so that conversations can query its content.
