"""
Main entry-point for the FoundationaLLM LangChainAPI.
"""

import datetime
import ast
import io
import json
import mimetypes
import os
import shutil
import sys

from fastapi import (
    FastAPI,
    HTTPException,
    UploadFile,
)
from fastapi.responses import FileResponse

app = FastAPI(
    title='FoundationaLLM Code Session API',
    summary='API for managing code sessions content and code execution.',
    description="""The FoundationaLLM Code Session API exposes code session capabilities
    required by the FoundationaLLM custom Python container.""",
    version='1.0.0',
    contact={
        'name':'FoundationaLLM, Inc.',
        'email':'contact@foundationallm.ai',
        'url':'https://foundationallm.ai/'
    },
    license_info={
        'name': 'FoundationaLLM Software License',
        'url': 'https://www.foundationallm.ai/license',
    }
)

ROOT_DATA_PATH = '/mnt/data'
ROOT_PATH = '/mnt'

@app.post('/code/execute')
async def execute_code(request_body: dict):
    """
    Execute code in the code session.

    Parameters
    ----------
    request_body : dict
        The request body containing the code to execute.
        The code to execute must be provided in the "code" field.

    Returns
    -------
    dict
        The response containing the result of the code execution.
    """

    code = request_body.get('code', None)
    if code is None:
        raise HTTPException(status_code=400, detail="Code not provided in the request body.")

    try:
        os.chdir(ROOT_DATA_PATH)  # Change the working directory to the data path

        namespace = {}
        old_stdout = sys.stdout
        new_stdout = io.StringIO()
        sys.stdout = new_stdout

        old_stderr = sys.stderr
        new_stderr = io.StringIO()
        sys.stderr = new_stderr

        # Determine which variable names (if any) are referenced by the last expression
        selected_names = _extract_last_variable_names(code)

        # pylint: disable=exec-used
        exec(code, namespace, namespace)
        # pylint: enable=exec-used

        standard_output = new_stdout.getvalue()
        sys.stdout = old_stdout

        standard_error = new_stderr.getvalue()
        sys.stderr = old_stderr

        # Build results: only include variables referenced by the last expression (if any)
        results = {}
        if selected_names:
            results = { name: namespace[name] for name in selected_names if name in namespace }

        # If any selected value is a pandas DataFrame or Series, auto-save to CSV and return serializable metadata
        if results:
            try:
                import pandas as pd  # type: ignore
                for key, value in list(results.items()):
                    if isinstance(value, pd.DataFrame):
                        # Create a safe unique filename
                        timestamp = datetime.datetime.utcnow().strftime('%Y%m%d%H%M%S%f')
                        safe_key = ''.join(c for c in str(key) if c.isalnum() or c in ('_', '-')) or 'df'
                        file_name = f"{safe_key}_{timestamp}.csv"
                        file_path = os.path.join(ROOT_DATA_PATH, file_name)
                        try:
                            value.to_csv(file_path, index=False)
                            # Replace DataFrame with a serializable summary
                            results[key] = {
                                "type": "dataframe",
                                "details": "The dataframe has been saved to a CSV file. A preview of the first ten rows of data has been generated. To view all the data, examine the CSV file that was returned.",
                                "preview": value.head(10).to_dict(orient="records")
                            }
                        except Exception:
                            # If writing fails, fall back to dropping the value (non-serializable)
                            results.pop(key, None)
                    elif isinstance(value, pd.Series):
                        # Handle pandas Series - convert to dict for serialization
                        results[key] = {
                            "type": "series",
                            "details": "The series data is displayed below.",
                            "data": value.to_dict()
                        }
            except Exception:
                # If pandas is not available or any import/runtime error occurs, leave results as-is
                pass

        return {
                'results': get_json_serializable_dict(results),
                'output': standard_output,
                'error': standard_error
         }
    except Exception as e:
        raise HTTPException(status_code=500, detail={'results': '', 'output': '', 'error': str(e)}) from e

@app.post('/files/upload')
async def upload_file(file: UploadFile):
    """
    Upload a file to the code session.

    Parameters
    ----------
    file : UploadFile
        The file to upload.

    Returns
    -------
    dict
        The response containing the file upload status.
    """

    try:
        base_path = os.path.normpath(ROOT_DATA_PATH)  # Define the safe root directory
        full_path = os.path.normpath(os.path.join(base_path, file.filename))
        if not full_path.startswith(base_path):
            raise HTTPException(status_code=400, detail="Invalid file name.")

        contents = await file.read()

        # Save the file to the code session
        with open(full_path, 'wb') as f:
            f.write(contents)

        return { 'status': 'success', 'filename': file.filename }
    except Exception as e:
        raise HTTPException(status_code=500, detail="Error uploading file.") from e

@app.get('/files')
async def list_files():
    """
    List files in the code session.

    Returns
    -------
    dict
        The response containing the list of files.
    """

    try:
        file_details = []
        file_store_root = os.path.normpath(ROOT_DATA_PATH)
        for root, _, files in os.walk(file_store_root):
            for file in files:
                rel_dir = os.path.relpath(root, file_store_root)
                full_path = os.path.join(file_store_root, file)
                mime_type, _ = mimetypes.guess_type(full_path)
                last_modified_ts = os.path.getmtime(full_path)
                last_modified_iso = datetime.datetime.fromtimestamp(last_modified_ts, tz=datetime.timezone.utc).isoformat().replace("+00:00", "Z")
                file_details.append({
                    'name': file,
                    'type': 'file',
                    'contentType': mime_type or 'application/octet-stream',
                    'sizeInBytes': os.path.getsize(full_path),
                    'lastModifiedAt': last_modified_iso,
                    'parentPath': rel_dir if rel_dir != '.' else ''})

        return { 'value': file_details }
    except Exception as e:
        raise HTTPException(status_code=500, detail="Error listing files.") from e

@app.post('/files/download')
async def download_file(request_body: dict):
    """
    Download a file from the code session.

    Parameters
    ----------
    request_body : dict
        The request body containing the filename to download.

    Returns
    -------
    FileResponse
        The response containing the file to download.
    """

    filename = request_body.get('file_name', None)
    if filename is None:
        raise HTTPException(status_code=400,
                            detail="The file name was not provided in the request body.")

    try:
        base_path = os.path.normpath(ROOT_DATA_PATH)  # Define the safe root directory
        fullpath = os.path.normpath(os.path.join(base_path, filename))
        if not fullpath.startswith(base_path) or not os.path.isfile(fullpath):
            raise HTTPException(status_code=404, detail="File not found.")

        headers = {'Content-Disposition': f'attachment; filename="{os.path.basename(fullpath)}"'}
        return FileResponse(fullpath, media_type='application/octet-stream', headers=headers)
    except Exception as e:
        raise HTTPException(status_code=500, detail="Error downloading file.") from e

@app.post('/files/delete')
async def delete_files():
    """
    Delete all files from the code session.

    Parameters
    ----------
    request_body : dict
        The request body.

    Returns
    -------
    dict
        The response containing the file deletion status.
    """

    try:
        os.chdir(ROOT_PATH)
        shutil.rmtree(ROOT_DATA_PATH)
        os.makedirs(ROOT_DATA_PATH, exist_ok=True)  # Recreate the directory after deletion

        return { 'status': 'success' }
    except Exception as e:
        raise HTTPException(status_code=500, detail="Error deleting files.") from e

def get_json_serializable_dict(d: dict) -> dict:
    """
    Remove non-JSON serializable values from a dictionary.

    Parameters
    ----------
    d : dict
        The dictionary to convert.

    Returns
    -------
    dict
        The JSON serializable dictionary.
    """

    result = {}
    for key, value in d.items():
        try:
            json.dumps(value)
            result[key] = value
        except (TypeError, OverflowError):
            pass
    return result

def _extract_last_variable_names(code: str) -> list[str]:
    """
    Extract variable names referenced by the last expression line of the code.

    Returns a list of variable names if the last syntactic unit is an expression consisting
    solely of variable name(s) (single name, tuple, or list of names). Returns an empty list
    in all other cases (including function calls, literals, attribute access, or final statements).
    """
    try:
        tree = ast.parse(code)
    except SyntaxError:
        return []

    if not getattr(tree, 'body', None):
        return []

    last_node = tree.body[-1]

    def names_from(node: ast.AST) -> list[str]:
        if isinstance(node, ast.Name):
            return [node.id]
        if isinstance(node, (ast.Tuple, ast.List)):
            names: list[str] = []
            for elt in node.elts:
                if isinstance(elt, ast.Name):
                    names.append(elt.id)
                else:
                    return []
            return names
        return []

    # Case 1: last node is a bare expression like: x or (x, y)
    if isinstance(last_node, ast.Expr):
        return names_from(last_node.value)

    # Case 2: last node is an assignment like: x = ... or (x, y) = ... or x = y = ...
    if isinstance(last_node, ast.Assign):
        collected: list[str] = []
        for target in last_node.targets:
            target_names = names_from(target)
            if not target_names:
                return []
            collected.extend(target_names)
        return collected

    return []
