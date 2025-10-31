function Set-FoundationaLLMAdminsAzureRoleAssignments {
    param (
        [string]$SubscriptionId,
        [string]$AdminGroupObjectId,
        [hashtable]$ResourceGroupNames
    )

    $roleAssignments = Get-Content "$PSScriptRoot/../data/azure-role-assignments.json" -Raw | ConvertFrom-Json -AsHashtable
    $adminRoleAssignments = $roleAssignments["FoundationaLLMAdmins"]

    foreach ($adminRoleAssignment in $adminRoleAssignments) {
        $roleName = $adminRoleAssignment[0]
        $scopeResourceGroupType = $adminRoleAssignment[1]
        $scopeResourceGroupName = $ResourceGroupNames[$scopeResourceGroupType]

        Write-Host "Ensuring FoundationaLLM Admins group $AdminGroupObjectId has $roleName assigned on resource group $scopeResourceGroupName..."

        az role assignment create `
            --assignee $AdminGroupObjectId `
            --role "$roleName" `
            --scope "/subscriptions/$SubscriptionId/resourceGroups/$scopeResourceGroupName" `
        | Out-Null
    }
}

function Set-ManagedIdentityAzureRoleAssignments {
    param (
        [string]$SubscriptionId,
        [string]$ManagedIdentityType,
        [hashtable]$ResourceGroupNames,
        [hashtable]$ResourceNames,
        [boolean]$AssignGraphRoles = $false,
        [boolean]$AssignCosmosDBRoles = $false
    )

    $miName = $ResourceNames[$ManagedIdentityType]
    $miClientId = (az identity show `
        --name $miName `
        --resource-group $ResourceGroupNames.Core `
        --query clientId -o tsv)
    $roleAssignments = Get-Content "$PSScriptRoot/../data/azure-role-assignments.json" -Raw | ConvertFrom-Json -AsHashtable
    $miRoleAssignments = $roleAssignments[$ManagedIdentityType]

    foreach ($miRoleAssignment in $miRoleAssignments) {
        $roleName = $miRoleAssignment[0]
        $scopeResourceGroupType = $miRoleAssignment[1]
        $scopeResourceGroupName = $ResourceGroupNames[$scopeResourceGroupType]

        Write-Host "Ensuring managed identity $miName with client id $miClientId has $roleName assigned on resource group $scopeResourceGroupName..."

        az role assignment create `
            --assignee $miClientId `
            --role "$roleName" `
            --scope "/subscriptions/$SubscriptionId/resourceGroups/$scopeResourceGroupName" `
        | Out-Null
    }

    $miObjectId = (az identity show `
        --name $miName `
        --resource-group $ResourceGroupNames.Core `
        --query principalId -o tsv)

    if ($AssignGraphRoles) {
        Add-MicrosoftGraphRolesToPrincipal -principalId $miObjectId
    }

    if ($AssignCosmosDBRoles) {
        Write-Host "Ensuring managed identity $miName has Cosmos DB Built-in Data Contributor role on Cosmos DB account $($ResourceNames.CosmosDBAccount)..."
        az cosmosdb sql role assignment create `
            --resource-group $ResourceGroupNames.Data `
            --account-name $ResourceNames.CosmosDBAccount `
            --role-definition-id "00000000-0000-0000-0000-000000000002" `
            --principal-id $miObjectId `
            --scope "/subscriptions/$($SubscriptionId)/resourceGroups/$($ResourceGroupNames.Data)/providers/Microsoft.DocumentDB/databaseAccounts/$($ResourceNames.CosmosDBAccount)" | Out-Null
    }
}