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
        [boolean]$AssignGraphRoles = $false
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

    if ($AssignGraphRoles) {
        $miObjectId = (az identity show `
            --name $miName `
            --resource-group $ResourceGroupNames.Core `
            --query principalId -o tsv)
        Add-MicrosoftGraphRolesToPrincipal -principalId $miObjectId
    }
}