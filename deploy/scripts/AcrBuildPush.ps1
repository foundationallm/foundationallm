#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$acrName,
    [parameter(Mandatory = $false)][string]$dockerTag = "latest"
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function EnsureSuccess($message) {
    if ($LASTEXITCODE -ne 0) {
        Write-Host $message -ForegroundColor Red
        Pop-Location
        exit $LASTEXITCODE
    }    
}

Push-Location $($MyInvocation.InvocationName | Split-Path)
$sourceFolder = $(./Join-Path-Recursively.ps1 -pathParts .., scripts)

$services = @(
    [pscustomobject]@{
        Name='agent-factory-api';
        Context='../../src';
        Dockerfile='../../src/dotnet/AgentFactoryAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='agent-hub-api';
        Context='../../src/python';
        Dockerfile='../../src/python/AgentHubAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='core-api';
        Context='../../src';
        Dockerfile='../../src/dotnet/CoreAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='core-job';
        Context='../../src';
        Dockerfile='../../src/dotnet/CoreWorkerService/Dockerfile'
    }
    [pscustomobject]@{
        Name='data-source-hub-api';
        Context='../../src/python';
        Dockerfile='../../src/python/DataSourceHubAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='gatekeeper-api';
        Context='../../src';
        Dockerfile='../../src/dotnet/GatekeeperAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='langchain-api';
        Context='../../src/python';
        Dockerfile='../../src/python/LangChainAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='prompt-hub-api';
        Context='../../src/python';
        Dockerfile='../../src/python/PromptHubAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='semantic-kernel-api';
        Context='../../src';
        Dockerfile='../../src/dotnet/SemanticKernelAPI/Dockerfile'
    }
    [pscustomobject]@{
        Name='chat-ui';
        Context='../../src/ui/UserPortal';
        Dockerfile='../../src/ui/UserPortal/Dockerfile'
    }
)

$message = @"
---------------------------------------------------
---------------------------------------------------
Getting info from ACR $resourceGroup/$acrName
---------------------------------------------------
"@
Write-Host $message -ForegroundColor Yellow

$acr = $(az acr show -g $resourceGroup -n $acrName -o json | ConvertFrom-Json)
EnsureSuccess "ACR $acrName not found"

$acrLoginServer = $acr.loginServer

$message = @"
---------------------------------------------------
Using ACR to build & tag images.
Images will be named as $acrLoginServer/imageName:$dockerTag
---------------------------------------------------
"@
Write-Host $message -ForegroundColor Yellow


$agentPool = $null
$agentPools=$(az acr agentpool list -r $acrName -g $resourceGroup -o json | ConvertFrom-Json)
if ($agentPools.count -gt 0)
{
    $agentPool = $agentPools[0].name
    Write-Host "Using agent pool $agentPool..." -ForegroundColor Yellow
}

for ( $idx = 0; $idx -lt $services.count; $idx++ )
{
    if ([string]::IsNullOrEmpty($agentPool))
    {
        az acr build -r $acrName -g $resourceGroup -t $services[$idx].Name -f $services[$idx].Dockerfile $services[$idx].Context
    }
    else
    {
        az acr build -r $acrName -g $resourceGroup -t $services[$idx].Name -f $services[$idx].Dockerfile --agent-pool $agentPool $services[$idx].Context
    }

    EnsureSuccess "ACR build failed."
}

Pop-Location