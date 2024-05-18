#!/usr/bin/pwsh
<#
.SYNOPSIS
    Applies MS Graph API roles to the Core API and Management API app registrations. This script is best 
    used after running the AZD provision step to create your Core and Management API services.
    and deploying the appropriate Core API and Management API infrastructure and services.

.DESCRIPTION
    This script applies the MS Graph API roles to the Core API and Management API services required for the Starter deployment.
    It retrieves the object ids of the Core API and Management API services and assigns the MS Graph API roles to
    the associated service MSIs. This script must be run after running azd provision in the ./deploy/quick-start folder.

    For more information on seting up the FLLM EntraID apps: : https://docs.foundationallm.ai/deployment/authentication/index.html
    
.PARAMETERS
Mandatory parameters:

Optional parameters:
    The names of the Core API and Management API FLLM apps. If you are using the Create-FllmEntraApps.ps1 script, then the names of 
    the apps are hardcoded and will not need to be set. If you are not using the Create-FllmEntraApps.ps1 script, then you can use 
    the following parameters to set the names of the apps:
    - fllmCoreApiNAme: The name of the FLLM Core API.
    - fllmMgmtApiName: The name of the FLLM Management API.
    
.EXAMPLE
If you have AZD provision step, then you can run the Apply-MSGraphAPIRoles.ps1 script with no parameters:
    ./Apply-MSGraphAPIRoles.ps1

#>

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Import azd env variables
(azd env get-values) | foreach {
    Write-Host $_
    $name, $value = $_.split('=')
    set-content env:\$name $value
}

# Update app registrations
$apps = @{
    "core-api" = @{
        objectId = $env:SERVICE_CORE_API_MI_OBJECT_ID
    }
    "management-api" = @{
        objectId = $env:SERVICE_MANAGEMENT_API_MI_OBJECT_ID
    }
}

foreach ($app in $apps.GetEnumerator()) {
    & ../../deploy/common/scripts/Assign-MSGraph-Roles.ps1 -principalId $app.Value.objectId
}