"""
Main entry-point for the FoundationaLLM LangChainAPI.
"""
from fastapi import FastAPI
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from app.lifespan_manager import lifespan
from app.routers import completions, status

app = FastAPI(
    lifespan=lifespan,
    title=f'FoundationaLLM LangChainAPI',
    summary='API for interacting with large language models using the LangChain orchestrator.',
    description=f"""The FoundationaLLM LangChainAPI is a wrapper around LangChain functionality
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

app.include_router(completions.router)
app.include_router(status.router)

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
    return { 'message': f'FoundationaLLM LangChainAPI' }
