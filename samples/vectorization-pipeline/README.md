# FoundationaLLM Vectorization Pipeline Sample

This sample demonstrates how to use the FoundationaLLM Vectorization Pipeline to vectorize files from a data source.

To run the sample, you need to have the following prerequisites:

1. Python 3.11 or later installed on your machine.
2. The [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed with version 2.71.0 or later.
3. The [FoundationaLLM GitHub repository](https://github.com/solliancenet/foundationallm) cloned to your local machine. You can clone the repository using the following command:

   ```cmd
   git clone https://github.com/solliancenet/foundationallm.git
   ```
4. Optional: [Visual Studio Code](https://code.visualstudio.com/download) installed.

>[!NOTE]
>The instructions in this document assume you are using Visual Studio Code as you IDE.

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
6. Create a `.env` file in the `samples/vectorization-pipeline` directory based on the template `.env.example`.
7. Open the `create_vectorization_pipeline.py` file in Visual Studio Code.
8. Modify the `create_vectorization_pipeline.py` file to specify the data source and other parameters for the vectorization pipeline. You can refer to the comments in the code for guidance on how to configure the pipeline.
9. Select the **Run and Debug** section, select **Python: Vectorization Pipeline** to run the script. This will execute the `create_vectorization_pipeline.py` file in the integrated terminal.