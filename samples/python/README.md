# FoundationaLLM Python Samples

These samples demonstrate how to call the FoundationaLLM Management API from Python. The samples are designed to be run in a local development environment, such as Visual Studio Code, and require the Azure CLI for authentication.

## Prerequisites

To run the samples, you need to have the following prerequisites:

1. Python 3.11 or later installed on your machine.
2. The [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed with version 2.71.0 or later.
3. The [FoundationaLLM GitHub repository](https://github.com/foundationallm/foundationallm) cloned to your local machine. You can clone the repository using the following command:

   ```cmd
   git clone https://github.com/foundationallm/foundationallm.git
   ```

> [!NOTE]
> Cloning the repository is optional. As an alternative, you can just download the contents of the `samples/python` directory. In this case, all the references to the `samples/python` directory in this document should be replaced with the path to the downloaded directory.

4. Optional: [Visual Studio Code](https://code.visualstudio.com/download) installed.

> [!NOTE]
> The instructions in this document assume you are using Visual Studio Code as you IDE.

## Running the samples

To run the sample, follow these steps:

1. Open the FoundationaLLM repository in Visual Studio Code.

2. Open a terminal in Visual Studio Code by selecting **Terminal** > **New Terminal** from the menu bar. Ensure the terminal is set to the `samples/python` directory.

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

7. Create a `.env` file in the `samples/python` directory based on the template `.env.example`.

> [!NOTE]
> The values of the variables in the `.env` file should be provided by your FoundationaLLM administrator.

8. Open any of the sample `.py` files in Visual Studio Code.

9. Modify the file to specify the values for the constants located at the beginning of the file.

> [!NOTE]
> When providing names for FoundationaLLM resources, they must be unique to the FoundationaLLM instance. They must also meet the requirements for valid FoundationaLLM resource names (must start with a letter and contain only letters, numbers, hyphens, or underscores).

10.  Select the **Run and Debug** section, select **Python: Vectorization Pipeline** to run the file.
