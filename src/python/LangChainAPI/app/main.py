"""
Main entry-point for the FoundationaLLM LangChainAPI.
"""
import logging
from fastapi import FastAPI
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from app.lifespan_manager import lifespan
from app.routers import completions, status, management

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
        # record.args[1] is the request path in uvicorn access logs
        # pylint: disable=W0718
        try:
            path = record.args[2]
            return path not in NO_LOGGING_ROUTES
        except Exception:
            return True  # fallback to allow logging

# Apply the filter to uvicorn.access logger
logging.getLogger("uvicorn.access").addFilter(SuppressAccessLogFilter())

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
