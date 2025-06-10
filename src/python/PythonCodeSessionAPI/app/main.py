"""
Main entry-point for the FoundationaLLM LangChainAPI.
"""

import datetime
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

        # pylint: disable=exec-used
        exec(code, {}, namespace)
        # pylint: enable=exec-used

        output = new_stdout.getvalue()
        sys.stdout = old_stdout

        return { 'results': get_json_serializable_dict(namespace), 'output': output }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error executing code: {str(e)}") from e

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
