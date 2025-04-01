Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber

$global:RoleDefinitionIds = @{}

function Test-RoleDefinitionIds {
    if ($global:RoleDefinitionIds.Count -eq 0) {
        (Get-RoleDefinitions) | ForEach-Object {
            $roleDefinitionIds[$_.display_name] = $_.object_id
        }
    }
}

function Get-RoleDefinitions {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Authorization/roleDefinitions"
}

function Get-EntraUserId {
    param (
        [string]$UPN
    )

    $id = az ad user show `
        --id $UPN `
        --output  tsv `
        --query id

    return $id
}

function Get-EntraSecurityGroupId {
    param (
        [string]$Name
    )

    $id = az ad group show `
        --g $Name `
        --output  tsv `
        --query id

    return $id
}

function Merge-RoleAssignment {
    param (
        [hashtable]$RoleAssignment
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Authorization/roleAssignments/$($RoleAssignment.name)" `
        -Body $RoleAssignment
}
