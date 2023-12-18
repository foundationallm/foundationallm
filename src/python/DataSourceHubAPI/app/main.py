"""
Main entry-point for the FoundationaLLM DataSourceHubAPI.
Runs web server exposing the API.
"""
import uvicorn
from fastapi import FastAPI
from app.dependencies import get_config
from app.routers import resolve, status
#from azure.monitor.opentelemetry import configure_azure_monitor

config = get_config()

# configure_azure_monitor(
#     connection_string=
#       config.get_value('FoundationaLLM:APIs:DataSourceHubAPI:AppInsightsConnectionString'),
#     disable_offline_storage=True
# )

app = FastAPI(
    title='FoundationaLLM DataSourceHubAPI',
    summary='API for retrieving DataSource metadata',
    description="""The FoundationaLLM DataSourceHubAPI is a wrapper around DataSourceHub
                functionality contained in the foundationallm.core Python SDK.""",
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

app.include_router(resolve.router)
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
    return { 'message': 'FoundationaLLM DataSourceHubAPI' }

if __name__ == '__main__':
    uvicorn.run('main:app', host='0.0.0.0', port=8842,
                reload=True, forwarded_allow_ips='*', proxy_headers=True)
