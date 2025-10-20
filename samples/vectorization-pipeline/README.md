# FoundationaLLM Vectorization Pipeline Sample

This sample demonstrates how to use the FoundationaLLM Vectorization Pipeline to vectorize files from a data source.

## Prerequisites

To run the sample, you need to have the following prerequisites:

1. Python 3.11 or later installed on your machine.
2. The [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed with version 2.71.0 or later.
3. The [FoundationaLLM GitHub repository](https://github.com/foundationallm/foundationallm) cloned to your local machine. You can clone the repository using the following command:

   ```cmd
   git clone https://github.com/foundationallm/foundationallm.git
   ```

> [!NOTE]
> Cloning the repository is optional. As an alternative, you can just download the contents of the `samples/vectorization-pipeline` directory. In this case, all the references to the `samples/vectorization-pipeline` directory in this document should be replaced with the path to the downloaded directory.

4. Optional: [Visual Studio Code](https://code.visualstudio.com/download) installed.

> [!NOTE]
> The instructions in this document assume you are using Visual Studio Code as you IDE.

## Running the sample

To run the sample, follow these steps:

1. Open the FoundationaLLM repository in Visual Studio Code.

2. Open a terminal in Visual Studio Code by selecting **Terminal** > **New Terminal** from the menu bar. Ensure the terminal is set to the `samples/vectorization-pipeline` directory.

3. Create a virtual environment by running the following command:

   ```cmd
   <path_to_python> -m venv .venv
   ```

   where `<path_to_python>` is the path to your Python installation. For example, if you have Python installed in `C:\Python311`, you would run:

   ```cmd
   C:\Python311\python.exe -m venv .venv
   ```

4. Activate the virtual environment by running the following command:

   ```cmd
   .venv\Scripts\activate
   ```

5. Install the required packages by running the following command:

   ```cmd
   python -m pip install -r requirements.txt
   ```

6. Ensure you are logged in to your Azure account using the Azure CLI. You can log in by running the following command:

   ```cmd
   az login
   ```

   This will open a web browser where you can enter your Azure credentials.

7. Create a `.env` file in the `samples/vectorization-pipeline` directory based on the template `.env.example`.

> [!NOTE]
> The values of the variables in the `.env` file should be provided by your FoundationaLLM administrator.

8. Open the `create_vectorization_pipeline.py` file in Visual Studio Code.

9. Modify the `create_vectorization_pipeline.py` file to specify the data source and other parameters for the vectorization pipeline.

> [!NOTE]
> The values for the `DATA_SOURCE_NAME` and `VECTOR_STORE_NAME` variables should be provided by your FoundationaLLM administrator. You are responsible for providing the `VECTORIZATION_PIPELINE_NAME` and `VECTORIZATION_PIPELINE_DESCRIPTION` variable values. The name of the vectorization pipeline must be unique and not already exist in the FoundationaLLM instance. It also must meet the requirements for valid FoundationaLLM resource names (must start with a letter and contain only letters, numbers, hyphens, or underscores).

10. Select the **Run and Debug** section, select **Python: Vectorization Pipeline** to run the script. This will execute the `create_vectorization_pipeline.py` file in the integrated terminal. Once the vectorization pipeline is created, you can execute it to vectorize files from the specified data source.

> [!IMPORTANT]
> The vectorization pipelines are automatically executed in the background. By default, the when the pipeline is created, it will be disabled. By activating the pipeline, you are indicating that you want to run the pipeline.

11. Open the `run_vectorization_pipeline.py` file in Visual Studio Code.
    
12. Modify the `run_vectorization_pipeline.py` file to specify the name of the vectorization pipeline to run.

>[!NOTE]
>The value for the `VECTORIZATION_PIPELINE_NAME` variable should be the name of the pipeline that you created in the previous step.

13. Select the **Run and Debug** section, select **Python: Vectorization Pipeline** to run the script. This will execute the `run_vectorization_pipeline.py` file in the integrated terminal.

>[!WARNING]
>Depending on the number and size of the files in the data source, the vectorization process may take some time to complete. You can always break the cycle of vectorization pipeline execution status checks and resume the process later. To do this, you need to remove the vectorization pipeline activation section from the code.
