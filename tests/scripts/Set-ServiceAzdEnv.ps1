#!/usr/bin/pwsh
<#
.SYNOPSIS
    Generates a azd environment values to be used with the Starter deployment. This script is best used after running the 
    ./deploy/common/scripts/entraid/Create-FllmEntraApps.ps1 script to create your FLLM EntraID apps.

.DESCRIPTION
    This script generates a set of azd environment values required for the Starter deployment.
    It retrieves the values of the Application IDs of the EntraID Apps required for the FLLM application and assigns them
    using the azd env command. This script must be run after running azd init in the ./deployment/starter folder.

    For more information on seting up the FLLM EntraID apps: : https://docs.foundationallm.ai/deployment/authentication/index.html
    
.PARAMETERS
Mandatory parameters:
    - tenantID: The Azure EntraID tenant ID.

Optional parameters:
    The names of the FLLM apps. If you are using the Create-FllmEntraApps.ps1 script, then the names of the apps are hardcoded and will not need to be set. 
    If you are not using the Create-FllmEntraApps.ps1 script, then you can use the following parameters to set the names of the apps:
    - fllmApiName: The name of the FLLM Core API.
    - fllmClientName: The name of the FLLM Client UI.
    - fllmMgmtApiName: The name of the FLLM Management API.
    - fllmMgmtClientName: The name of the FLLM Management Client UI.
    - fllmAuthApiName: The name of the FLLM Authorization API.
    
.EXAMPLE
If you have run the Create-FllmEntraApps.ps1 script, then you can run the Set-AzdEnv.ps1 script with the tenant ID as the only parameter:
    ./Set-AzdEnv.ps1 -tenant "12345678-1234-1234-1234-1234567890ab"    

If you have not run the Create-FllmEntraApps.ps1 script, then you can run the Set-AzdEnv.ps1 script with the tenant ID and the names of the apps.
You will need to update the app names with those you created manually in the EntraID portal.
    ./Set-AzdEnv.ps1 -tenant "12345678-1234-1234-1234-1234567890ab" `
                     -fllmApiName "FoundationaLLM" `
                     -fllmClientName "FoundationaLLM-Client" `
                     -fllmMgmtApiName "FoundationaLLM-Management" `
                     -fllmMgmtClientName "FoundationaLLM-ManagementClient" `
                     -fllmAuthApiName "FoundationaLLM-Authorization"

Make sure that your API Scope Names are updated inside of the script as well. These can be found in the EntraID portal under the 
Manage -> "Expose and API" section for the API applications you created manually. The Scope name format is api://<api-name>or<api-guid>/<scope-name>.
#>

Param(
    [parameter(Mandatory = $true)][string]$dockerTag, # Docker image tag
    [parameter(Mandatory = $true)][string]$envKey, # AZD Environment key
    [parameter(Mandatory = $true)][string]$imageName, # Docker image name
    [parameter(Mandatory = $true)][string]$registry # Docker container registry

)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Set the environment values

$values = @(
    @{
        Key = $envKey
        Value = "$($registry)/$($imageName):$($dockerTag)"
    }
)

Write-Host "Setting azd environment values for the service $($imageName)."

foreach ($value in $values) {
    Write-Host "Setting $value"
    azd env set $($value.Key) "$($value.Value)"

    if ($LASTEXITCODE -ne 0) {
        Write-Error("Failed to set $($value.Key).")
        exit 1
    }
}
