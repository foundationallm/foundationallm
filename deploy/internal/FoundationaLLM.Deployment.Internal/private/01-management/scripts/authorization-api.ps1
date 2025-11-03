function Initialize-AuthorizationAPI {
    param (
        [string]$UniqueName,
        [string]$Location,
        [string]$AdminGroupObjectId,
        [string]$UserGroupObjectId,
        [string]$DeploymentUserObjectId,
        [string]$TenantId,
        [string]$SubscriptionId,
        [string]$InstanceId,
        [string]$ContainerImage
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName
    $appRegistrationNames = Get-EntraIDAppRegistrationNames

    $authResourceGroupName = $resourceGroupNames.Authorization
    $coreResourceGroupName = $resourceGroupNames.Core

    $authStorageAccountName = $resourceNames.AuthStorageAccount
    $authKeyVaultName = $resourceNames.AuthKeyVault

    Write-Host "Ensuring storage account $authStorageAccountName exists in resource group '$authResourceGroupName'..."
    if (-not (az storage account check-name `
        --name $authStorageAccountName | ConvertFrom-Json).nameAvailable) {
        Write-Host "Storage account $authStorageAccountName already exists."
    } else {
        az storage account create `
            --name $authStorageAccountName `
            --resource-group $authResourceGroupName `
            --location $Location `
            --sku Standard_LRS `
            --kind StorageV2 `
            --allow-shared-key-access $false `
            --enable-hierarchical-namespace $true `
            --min-tls-version TLS1_2 | Out-Null

        az storage account blob-service-properties update `
            --account-name $authStorageAccountName `
            --enable-delete-retention $true `
            --delete-retention-days 30 `
            --enable-container-delete-retention $true `
            --container-delete-retention-days 30 | Out-Null
        Write-Host "Storage account $authStorageAccountName created."
    }

    $storageContainers = @(
        "policy-assignments",
        "role-assignments",
        "secret-keys"
    )

    foreach ($containerName in $storageContainers) {
        Write-Host "Ensuring storage container $containerName exists in storage account '$authStorageAccountName'..."
        if ((az storage container list `
            --account-name $authStorageAccountName `
            --auth-mode login `
            --query "[?name=='$($containerName)']" -o tsv).Count -eq 0) {
            
            az storage container create `
                --account-name $authStorageAccountName `
                --auth-mode login `
                --name $containerName | Out-Null
            Write-Host "Storage container $containerName created."
        } else {
            Write-Host "Storage container $containerName already exists."
        }
    }

    Write-Host "Ensuring Key Vault $authKeyVaultName exists in resource group '$authResourceGroupName'..."
    if ((az keyvault list `
        -g $authResourceGroupName `
        --query "[?name=='$($authKeyVaultName)']" -o tsv).Count -eq 0) {

        Write-Host "Creating Key Vault $authKeyVaultName in resource group $authResourceGroupName..."
        az keyvault create `
            --name $authKeyVaultName `
            --resource-group $authResourceGroupName `
            --location $Location | Out-Null
        Write-Host "Key Vault $authKeyVaultName created."
    } else {
        Write-Host "Key Vault $authKeyVaultName already exists."
    }

    Write-Host "Ensuring Authorization API managed identity exists..."
    $managedIdentities = @(
        $resourceNames.AuthorizationAPIManagedIdentity
    )
    Initialize-ManagedIdentities `
        -ManagedIdentityNames $managedIdentities `
        -ResourceGroupName $coreResourceGroupName `
        -Location $Location

    Write-Host "Ensuring FoundationaLLM role assignments for FoundationaLLM Admin group and Management API managed identity exist..."
    if (-not (az storage blob exists `
        --account-name $authStorageAccountName `
        --auth-mode login `
        --container-name "role-assignments" `
        --name "$InstanceId.json" | ConvertFrom-Json).exists) {

        Write-Host "Creating role assignments for FoundationaLLM Admin group and Management API managed identity..."
        $placeholders = @{
            "FOUNDATIONALLM_INSTANCE_ID"                       = $InstanceId
            "ADMIN_GROUP_OBJECT_ID"                            = $AdminGroupObjectId
            "DEPLOYMENT_USER_OBJECT_ID"                        = $DeploymentUserObjectId
            "MANAGEMENT_API_MI_OBJECT_ID"                      = (az identity show `
                                                                --name $resourceNames.ManagementAPIManagedIdentity `
                                                                --resource-group $coreResourceGroupName `
                                                                --query principalId -o tsv)
            "DEPLOY_TIME"                                      = (Get-Date).ToString("o")
            "GUID01"                                           = [guid]::NewGuid().ToString()
            "GUID02"                                           = [guid]::NewGuid().ToString()
            "GUID03"                                           = [guid]::NewGuid().ToString()
            "GUID04"                                           = [guid]::NewGuid().ToString()
            "GUID05"                                           = [guid]::NewGuid().ToString()
        }
        Get-Content "$PSScriptRoot/../data/foundationallm-role-assignments-template.json" -Raw `
            | ForEach-Object { 
                Write-Output "$(Update-TemplateContent -TemplateContent $_ -Placeholders $placeholders)"
            } `
            | az storage blob upload `
                --account-name $authStorageAccountName `
                --auth-mode login `
                --container-name "role-assignments" `
                --name "$InstanceId.json" `
                --data '@-'`
            | Out-Null
        Write-Host "Role assignment for FoundationaLLM Admin group and Management API managed identity created."
    } else {
        Write-Host "Role assignments for FoundationaLLM Admin group and Management API managed identity already exist."
    }

    Write-Host "Ensuring FoundationaLLM policy assignments for FoundationaLLM User group and All Agents Virtual Security Group exist..."
    if (-not (az storage blob exists `
        --account-name $authStorageAccountName `
        --auth-mode login `
        --container-name "policy-assignments" `
        --name "$InstanceId.json" | ConvertFrom-Json).exists) {

        Write-Host "Creating policy assignments for FoundationaLLM User group and All Agents Virtual Security Group..."
        $placeholders = @{
            "FOUNDATIONALLM_INSTANCE_ID"                       = $InstanceId
            "ADMIN_GROUP_OBJECT_ID"                            = $AdminGroupObjectId
            "USER_GROUP_OBJECT_ID"                             = $UserGroupObjectId
            "DEPLOY_TIME"                                      = (Get-Date).ToString("o")
            "GUID01"                                           = [guid]::NewGuid().ToString()
            "GUID02"                                           = [guid]::NewGuid().ToString()
            "GUID03"                                           = [guid]::NewGuid().ToString()
            "GUID04"                                           = [guid]::NewGuid().ToString()
            "GUID05"                                           = [guid]::NewGuid().ToString()
            "GUID06"                                           = [guid]::NewGuid().ToString()
            "GUID07"                                           = [guid]::NewGuid().ToString()
            "GUID08"                                           = [guid]::NewGuid().ToString()
            "GUID09"                                           = [guid]::NewGuid().ToString()
            "GUID10"                                           = [guid]::NewGuid().ToString()
            "GUID11"                                           = [guid]::NewGuid().ToString()
            "GUID12"                                           = [guid]::NewGuid().ToString()
            "GUID13"                                           = [guid]::NewGuid().ToString()
            "GUID14"                                           = [guid]::NewGuid().ToString()
            "GUID15"                                           = [guid]::NewGuid().ToString()
            "GUID16"                                           = [guid]::NewGuid().ToString()
        }
        Get-Content "$PSScriptRoot/../data/foundationallm-policy-assignments-template.json" -Raw `
            | ForEach-Object { 
                Write-Output "$(Update-TemplateContent -TemplateContent $_ -Placeholders $placeholders)"
            } `
            | az storage blob upload `
                --account-name $authStorageAccountName `
                --auth-mode login `
                --container-name "policy-assignments" `
                --name "$InstanceId-policy.json" `
                --data '@-'`
            | Out-Null
        Write-Host "Policy assignments for FoundationaLLM User group and All Agents Virtual Security Group created."
    } else {
        Write-Host "Policy assignments for FoundationaLLM User group and All Agents Virtual Security Group already exist."
    }

    Write-Host "Ensuring Azure role assignments for Authorization API managed identity exist..."
    Set-ManagedIdentityAzureRoleAssignments `
        -SubscriptionId $SubscriptionId `
        -ManagedIdentityType "AuthorizationAPIManagedIdentity" `
        -ResourceGroupNames $resourceGroupNames `
        -ResourceNames $resourceNames

    Write-Host "Ensuring Authorization API key vault secrets exist..."
    $keyVaultSecrets = Get-AuthorizationKeyVaultSecrets `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -InstanceId $InstanceId `
        -EntraIDAppRegistrationName $appRegistrationNames.AuthorizationAPI
    Set-KeyVaultSecrets `
        -KeyVaultName $authKeyVaultName `
        -Secrets $keyVaultSecrets

    Write-Host "Ensuring Authorization API container app exists..."
    $environmentVariables = Get-AuthorizationEnvVars `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppIdentity $resourceNames.AuthorizationAPIManagedIdentity
    
    Initialize-ContainerApp `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppName $resourceNames.AuthorizationAPIContainerApp `
        -ContainerAppIdentity $resourceNames.AuthorizationAPIManagedIdentity `
        -EnvironmentVariables $environmentVariables `
        -ContainerImage $ContainerImage `
        -MinReplicas 1 `
        -MaxReplicas 1
}