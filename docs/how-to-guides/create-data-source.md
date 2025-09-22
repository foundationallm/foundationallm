# Create a data source in the Management Portal

This step-by-step guide walks through creating a new data source for FoundationaLLM by using the Management Portal. Follow these instructions when you need to register a storage location that agents can index and query.

## Prerequisites

- You have a FoundationaLLM environment with access to the Management Portal.
- You have the data source connection details handy, such as the Azure Storage account, container, and any authentication secrets.
- You are assigned a role that allows managing data sources (for example, **FoundationaLLM Owner** or **FoundationaLLM Administrator**).

## Step-by-step instructions

1. **Sign in to the Management Portal.**
   - Open the portal URL for your deployment and sign in with an account that has the required role assignments.
2. **Open the Data Sources workspace.**
   - In the left navigation pane, select **Data Sources** to see the existing connections that agents can use.
3. **Start creating a new data source.**
   - Select **+ New data source**. The portal opens the creation wizard with the **Basics** tab selected.
4. **Populate the Basics tab.**
   - Enter a descriptive **Name** and optional **Description** so other administrators can recognize the data source.
   - Choose the **Resource type** that matches the data you plan to connect (for example, *Azure Data Lake Storage Gen2* or *Azure Blob Storage*).
   - If the resource is hosted in another tenant, enable **Cross-tenant resource** and supply the tenant ID.
   - Select **Next** when all required fields are complete.
5. **Configure the connection settings.**
   - Provide the resource-specific details. For storage accounts, this typically includes the **Subscription**, **Resource group**, **Storage account**, and target **Container** or **File system**.
   - Decide how FoundationaLLM should authenticate. Choose between **Managed Identity**, **Shared access signature**, or **Access key** based on your security requirements.
   - If you select a secret-based method, paste the credential value or reference a stored secret from Azure Key Vault.
   - Select **Next** to continue.
6. **Set indexing preferences.**
   - Choose the **Ingestion profile** or configure custom options that control how often the portal indexes the data.
   - Specify any file path filters, file type exclusions, or metadata extraction preferences required for your scenario.
   - Select **Next** when finished.
7. **Review and create.**
   - Confirm the summary information on the **Review + create** tab. If you need to make changes, use the breadcrumb to revisit earlier tabs.
   - When everything looks correct, select **Create** to register the data source.
8. **Monitor the provisioning status.**
   - The portal displays a notification while it validates access and saves the configuration. Wait for the status to change to **Succeeded**.
9. **Verify ingestion.**
   - After creation, open the data source detail page and confirm the **Ingestion status** reports **Healthy**. You can trigger a manual refresh if you want to validate that the connection succeeds immediately.

## Next steps

- Attach the new data source to an agent so that conversations can query its content.
- Use the portal's monitoring views to track ingestion runs and troubleshoot any failures.
- Review security policies regularly to ensure credentials and role assignments remain compliant.
