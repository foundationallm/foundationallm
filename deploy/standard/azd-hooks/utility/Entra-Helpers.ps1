#!/usr/bin/env pwsh

function Get-Entra-Config {
    param (
        [string]$tenantId = $null,
        [string]$authAppName = 'FoundationaLLM-Authorization-API',
        [string]$coreAppName = 'FoundationaLLM-Core-API',
        [string]$coreClientAppName = 'FoundationaLLM-Core-Portal',
        [string]$mgmtAppName = 'FoundationaLLM-Management-API',
        [string]$mgmtClientAppName = 'FoundationaLLM-Management-Portal',
        [string]$readerAppName = 'FoundationaLLM-Reader'
    )

    # Retrieve the tenant ID from the current Azure account context if not provided
    if (-not $tenantID) {
    $tenantID = (az account show --query 'tenantId' --output tsv)
    if (-not $tenantID) {
        throw "Unable to retrieve tenant ID from the current Azure account context. Please provide a tenant ID."
    }
    }

    # Set the environment values
    Write-Host -ForegroundColor Blue "Please wait while gathering azd environment values for the ${tenantID} EntraID Tenant."

    $adminGroupName = $(azd env get-value FOUNDATIONALLM_ADMIN_GROUP_NAME)
    if ($LastExitCode -ne 0) {
        $adminGroupName = Read-Host "Enter EntraID Admin Group Name (Enter for 'FLLM-Admins'): "
        $adminGroupName = $adminGroupName.Trim()
        if ($adminGroupName -eq "") {
            $adminGroupName = "FLLM-Admins"
            Write-Host "Defaulting to $adminGroupName for Admin Group Name." -ForegroundColor Yellow
        }

        azd env set FOUNDATIONALLM_ADMIN_GROUP_NAME $adminGroupName
    } else {
        $newAdminGroupName = Read-Host "Enter EntraID Admin Group Name (Enter for '$adminGroupName'): "
        $newAdminGroupName = $newAdminGroupName.Trim()
        if ($newAdminGroupName -ne "") {
            $adminGroupName = $newAdminGroupName
            Write-Host "Setting to $adminGroupName for Admin Group Name." -ForegroundColor Yellow
            azd env set FOUNDATIONALLM_ADMIN_GROUP_NAME $adminGroupName
        }
    }

    Write-Host "Get Admin Group ID" -ForegroundColor Blue
    $adminGroupId = az ad group list `
        --filter "displayName eq '$adminGroupName'" `
        --query "[0].id" `
        --output tsv

    $userGroupName = $(azd env get-value FOUNDATIONALLM_USER_GROUP_NAME)
    if ($LastExitCode -ne 0) {
        $userGroupName = Read-Host "Enter EntraID User Group Name (Enter for 'FLLM-Users'): "
        $userGroupName = $userGroupName.Trim()
        if ($userGroupName -eq "") {
            $userGroupName = "FLLM-Users"
            Write-Host "Defaulting to $userGroupName for User Group Name." -ForegroundColor Yellow
        }

        azd env set FOUNDATIONALLM_USER_GROUP_NAME $userGroupName
    } else {
        $newUserGroupName = Read-Host "Enter EntraID User Group Name (Enter for '$userGroupName'): "
        $newUserGroupName = $newUserGroupName.Trim()
        if ($newUserGroupName -ne "") {
            $userGroupName = $newUserGroupName
            Write-Host "Setting to $userGroupName for User Group Name." -ForegroundColor Yellow
            azd env set FOUNDATIONALLM_USER_GROUP_NAME $userGroupName
        }
    }

    Write-Host "Get User Group ID" -ForegroundColor Blue
    $userGroupId = az ad group list `
        --filter "displayName eq '$userGroupName'" `
        --query "[0].id" `
        --output tsv


   $appIds = @{}
   $appNames = @{
      auth             = $authAppName
      coreapi          = $coreAppName
      coreclient       = $coreClientAppName
      managmentapi     = $mgmtAppName
      managementclient = $mgmtClientAppName
      reader           = $readerAppName
   }

   $appData = @{}
   foreach ($app in $appNames.GetEnumerator()) {
        Write-Host "Get App ID for $($app.Value)" -ForegroundColor Yellow

        $data = $(az ad app list `
            --display-name "$($app.Value)" `
            --query '[].{appId:appId,objectId:id}' `
            --output json | ConvertFrom-Json)

        $appData[$app.Key] = $data
   }

   $values = @{
      "ADMIN_GROUP_OBJECT_ID"          = $adminGroupId
      "USER_GROUP_OBJECT_ID"           = $userGroupId
      "ENTRA_AUTH_API_CLIENT_ID"       = $appData["auth"].appId
      "ENTRA_AUTH_API_INSTANCE"        = "https://login.microsoftonline.com/"
      "ENTRA_AUTH_API_SCOPES"          = "api://FoundationaLLM-Authorization"
      "ENTRA_AUTH_API_TENANT_ID"       = $tenantID
      "ENTRA_CHAT_UI_CLIENT_ID"        = $appData["coreclient"].appId
      "ENTRA_CHAT_UI_SCOPES"           = "api://FoundationaLLM-Core/Data.Read"
      "ENTRA_CORE_API_CLIENT_ID"       = $appData["coreapi"].appId
      "ENTRA_CORE_API_SCOPES"          = "Data.Read"
      "ENTRA_MANAGEMENT_API_CLIENT_ID" = $appData["managmentapi"].appId
      "ENTRA_MANAGEMENT_API_SCOPES"    = "Data.Manage"
      "ENTRA_MANAGEMENT_UI_CLIENT_ID"  = $appData["managementclient"].appId
      "ENTRA_MANAGEMENT_UI_SCOPES"     = "api://FoundationaLLM-Management/Data.Manage"
      "ENTRA_READER_CLIENT_ID"         = $appData["reader"].appId
   }

   # Write AZD environment values
   Write-Host -ForegroundColor Yellow "Setting azd environment values for the ${tenantID} EntraID Tenant."
   foreach ($value in $values.GetEnumerator()) {
      Write-Host -ForegroundColor Yellow  "Setting $($value.Name) to $($value.Value)"
      azd env set $value.Name $value.Value
   }
}