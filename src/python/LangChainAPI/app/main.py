"""
Main entry-point for the FoundationaLLM LangChain API.
"""
import datetime
import json
import logging

from fastapi import FastAPI, Request
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from app.lifespan_manager import lifespan
from app.routers import completions, status, management

from foundationallm.telemetry import Telemetry

app = FastAPI(
    lifespan=lifespan,
    title='FoundationaLLM LangChainAPI',
    summary='API for interacting with large language models using the LangChain orchestrator.',
    description="""The FoundationaLLM LangChainAPI is a wrapper around LangChain functionality
                contained in the foundationallm.core Python SDK.""",
    version='1.0.0',
    contact={
        'name':'FoundationaLLM, Inc.',
        'email':'contact@foundationallm.ai',
        'url':'https://foundationallm.ai/'
    },
    openapi_url='/swagger/v1/swagger.json',
    docs_url='/swagger',
    redoc_url=None,
    license_info={
        'name': 'FoundationaLLM Software License',
        'url': 'https://www.foundationallm.ai/license',
    }
)

NO_LOGGING_ROUTES = { '/status' }

class SuppressAccessLogFilter(logging.Filter):
    '''Filter to suppress access logs for specific routes.'''
    # pylint: disable=R0903
    def filter(self, record: logging.LogRecord) -> bool:
        # record.args[2] is the request path in uvicorn access logs
        # pylint: disable=W0718
        try:
            path = record.args[2]
            return path not in NO_LOGGING_ROUTES
        except Exception:
            return True  # fallback to allow logging

# Apply the filter to uvicorn.access logger
logging.getLogger("uvicorn.access").addFilter(SuppressAccessLogFilter())
logger = Telemetry.get_logger(__name__)

app.include_router(completions.router)
app.include_router(status.router)
app.include_router(management.router)

FastAPIInstrumentor.instrument_app(app)

@app.get('/')
async def root():
    """
    Root path of the API.

    Returns
    -------
    str
        Returns a JSON object containing a message and value.
    """
    return { 'message': 'FoundationaLLM LangChainAPI' }

COMPLETION_REQUESTS_TRACING_HEADER = \
    'x-trace-completion-request'
USER_IDENTITY_HEADER = 'x-user-identity'

@app.middleware("http")
async def log_requests(
    request: Request,
    call_next):
    """Middleware to log incoming requests and their processing time."""

    if COMPLETION_REQUESTS_TRACING_HEADER in request.headers \
        and request.headers[COMPLETION_REQUESTS_TRACING_HEADER].lower() == 'true' \
        and USER_IDENTITY_HEADER in request.headers \
        and request.headers[USER_IDENTITY_HEADER]:

        try:
            user_identity_dict = json.loads(
                request.headers[USER_IDENTITY_HEADER])
            upn = user_identity_dict.get('upn', None)

            if upn:
                # Read and store the raw body
                body = await request.body()

                upn = upn.lower().replace('@', '-').replace('.', '-')
                ref_time = datetime.datetime.now(tz=datetime.timezone.utc)
                file_path = f"{upn}/{ref_time:%Y-%m-%d}/{ref_time:%Y-%m-%d-%H%M%S}-langchain-request-IN.json"

                request.app.state.completion_requests_storage_manager.write_file_content(
                    file_path,
                    body.decode('utf-8'))

                # Rebuild the request stream, so downstream handlers can read it
                async def receive():
                    return {"type": "http.request", "body": body}

                request = Request(request.scope, receive=receive)

        except Exception as e:
            logger.error("Error persisting raw request: %s", e, exc_info=True)

    # Process the request
    response = await call_next(request)
    return response
