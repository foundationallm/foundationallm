import uvicorn
from fastapi import FastAPI
from app.dependencies import get_config
from app.routers import analyze, anonymize, status

config = get_config()

app = FastAPI(
    title='FoundationaLLM GatekeeperIntegrationAPI',
    summary='API for extending the FoundationaLLM GatekeeperAPI',
    description='The FoundationaLLM GatekeeperIntegrationAPI is a service used to extend the FoundationaLLM GatekeeperAPI with extra capabilities',
    version='1.0.0',
    contact={
        'name':'Solliance, Inc.',
        'email':'contact@solliance.net',
        'url':'https://solliance.net/' 
    },
    openapi_url='/swagger/v1/swagger.json',
    docs_url='/swagger',
    redoc_url=None,
    license_info={
        'name': 'FoundationaLLM Software License',
        'url': 'https://www.foundationallm.ai/license',
    }
)

app.include_router(analyze.router)
app.include_router(anonymize.router)
app.include_router(status.router)

@app.get('/')
async def root():
    """
    Root path of the API.
    
    Returns
    -------
    str
        Returns a JSON object containing a message and value.
    """
    return { 'message': 'FoundationaLLM GatekeeperIntegrationAPI' }

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8042, reload=True, forwarded_allow_ips='*', proxy_headers=True)
