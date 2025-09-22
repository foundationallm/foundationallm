# Create a data pipeline in the Management Portal

This guide explains how to build a new data pipeline in the Management Portal. Pipelines orchestrate data source connections, processing stages, and triggers that control when data ingestion jobs run.

## Prerequisites

- You have access to the Management Portal for your FoundationaLLM environment.
- Your account is assigned a role that lets you manage pipelines (for example, **FoundationaLLM Owner** or **FoundationaLLM Contributor**). [TODO: Confirm roles]
- Any data sources, stage plugins, and trigger plugins you plan to use are already registered and available in the portal.

## Step-by-step instructions

1. **Sign in to the Management Portal.**
   - Navigate to your deployment's portal URL and authenticate with an account that has the required permissions.
2. **Open the Pipelines workspace.**
   - From the left navigation pane, select **Data Pipelines** to view existing pipelines.
3. **Start creating a new pipeline.**
   - Select **+ Create Pipeline** to open the pipeline creation wizard.
4. **Name the pipeline.**
   - In **Pipeline name**, enter a unique identifier. Pipeline names must use only letters, numbers, dashes, or underscores—no spaces or special characters.
   - As you type, the portal validates the name and shows whether it is available.
5. **Provide display details.**
   - In **Pipeline display name**, supply a friendly name to show in the portal.
   - In **Pipeline description**, add context about the pipeline's purpose.
6. **Select and configure a data source.**
   - Use the **Select a data source** dropdown to choose an existing data source resource. After selection, complete the additional fields that appear:
     - **Data source name**: Provide a name for how this pipeline will reference the source.
     - **Data source description**: Describe the source connection for downstream users.
     - **Data source plugin**: Pick the plugin that defines how to connect to and read from the source.
     - For each plugin parameter shown, provide the required values. Depending on the parameter type, you may enter text, toggle a boolean, add comma-separated items, or pick from a list of available resources.
7. **Configure pipeline stages.**
   - In the **Configure pipeline stages** section, use **Add Stage** to insert the processing steps the pipeline will run.
   - For each stage card:
     - **Stage Name**: Enter a unique name. The system prevents duplicate names.
     - **Description**: Summarize what the stage does.
     - **Plugin**: Choose a stage plugin to define the work performed.
     - **Parameters**: Supply values for each parameter required by the plugin. Parameter editors adjust automatically to support text, numbers, booleans, lists, or resource selections.
     - **Dependencies** (if offered): If the selected plugin requires other stages, choose the dependency from the dropdown (single dependency) or multi-select list (multiple dependencies). After adding a dependency, fill in any parameter values that appear for the dependent plugin.
   - Use the drag handle to reorder stages as needed. The chevron button collapses or expands a stage, and the trash icon removes it.
8. **Define triggers.**
   - In **Configure pipeline triggers**, set up how and when the pipeline should run. Select **Add Trigger** to create additional triggers.
   - For each trigger:
     - **Trigger Name**: Provide a unique name. The portal tracks name changes to avoid duplicates.
     - **Trigger Type**: Pick **Schedule**, **Event**, or **Manual**.
     - **Cron Schedule** (Schedule triggers only): Enter the cron expression that defines when the pipeline executes (for example, `0 6 * * *`).
     - **Trigger Parameters**: Supply required values for the selected trigger type. The editor supports text, numeric, boolean, list, and resource selection inputs based on each parameter's metadata.
   - Collapse or delete triggers using the chevron and trash icons, respectively.
9. **Create the pipeline.**
   - After supplying all required information, select **Create Pipeline**.

## Next steps

- Monitor the pipeline run history to ensure triggers fire and stages complete as expected.
- Attach pipelines to agents or automation workflows that depend on the processed data.
