"""
Main entry-point for the FoundationaLLM LangChainAPI.
"""

import io
import sys
from fastapi import (
    FastAPI,
    HTTPException,
    UploadFile
)

app = FastAPI(
    title='FoundationaLLM Code Session API',
    summary='API for managing code sessions content and code execution.',
    description='The FoundationaLLM Code Session API exposes code session capabilities required by the FoundationaLLM custom Python container.',
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
        namespace = {}
        old_stdout = sys.stdout
        new_stdout = io.StringIO()
        sys.stdout = new_stdout

        # pylint: disable=exec-used
        exec(code, {}, namespace)
        # pylint: enable=exec-used

        output = new_stdout.getvalue()
        sys.stdout = old_stdout

        return { 'results': namespace, 'output': output }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error executing code.") from e

@app.post('files/upload')
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
        contents = await file.read()
        # Save the file to the code session
        with open(file.filename, 'wb') as f:
            f.write(contents)

        return { 'status': 'success', 'filename': file.filename }
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error uploading file.") from e