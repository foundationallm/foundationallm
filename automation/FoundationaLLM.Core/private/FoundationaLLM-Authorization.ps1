
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

function Get-RoleDefinitionName {
    param (
        [string]$Id
    )

    Test-RoleDefinitionIds

    return $global:RoleDefinitionIds.GetEnumerator() `
        | Where-Object { $_.Value -eq $Id } `
        | Select-Object -ExpandProperty Key
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

function Find-RoleAssignments {
    param (
        [string]$Scope,
        [string[]]$securityPrincipalIds
    )

    $roleAsignmentsFilter = @{
        scope = $Scope
        security_principal_ids = $securityPrincipalIds
    }

    return (Invoke-ManagementAPI `
            -Method POST `
            -RelativeUri "providers/FoundationaLLM.Authorization/roleAssignments/filter" `
            -Body $roleAsignmentsFilter).resource `
        | Select-Object @{Name="RoleAssignmentId";Expression={$_.name}}, @{Name="RoleDefinitionName";Expression={Get-RoleDefinitionName -Id $_.role_definition_id}}
}

function Find-RoleAssignmentId {
    param (
        [string]$Scope,
        [string]$SecurityPrincipalId,
        [string]$RoleName
    )

    return Find-RoleAssignments `
        -Scope $Scope `
        -securityPrincipalIds @($SecurityPrincipalId) `
        | Where-Object { $_.RoleDefinitionName -eq $RoleName }
        | Select-Object -ExpandProperty RoleAssignmentId
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

function Remove-RoleAssignment {
    param (
        [string]$Id
    )

    return Invoke-ManagementAPI `
        -Method DELETE `
        -RelativeUri "providers/FoundationaLLM.Authorization/roleAssignments/$Id"
}

function Merge-RoleAssignments {
    param (
        [string]$PackageRoot,
        [hashtable]$Parameters = @{}
    )

    Test-RoleDefinitionIds

    $roleAssignments = Get-Content "$($PackageRoot)/artifacts/roleAssignments.json" `
        | Resolve-Placeholders -Parameters $Parameters `
        | ConvertFrom-Json -AsHashTable

    $roleAssignments | ForEach-Object {

        $securityPrincipal = $_

        if ($null -ne $securityPrincipal.principal_id){
            $securityPrincipalId = $securityPrincipal.principal_id
        } elseif ($securityPrincipal.principal_type -eq "User") {
            $securityPrincipalId = Get-EntraUserId -UPN $securityPrincipal.principal_name
        } else {
            $securityPrincipalId = Get-EntraSecurityGroupId -Name $securityPrincipal.principal_name
        }

        Write-Host "Assigning roles for [$($securityPrincipal.principal_name)]($($securityPrincipalId))"

        foreach ($roleAssignment in $securityPrincipal.role_assignments) {
            if ($roleAssignment.Length -eq 1) {
                $scope = "/instances/$($global:InstanceId)"
            } else {
                $scope = Get-ObjectId -Name $roleAssignment[2] -Type $roleAssignment[1]
            }
            
            $roleDefinitionId = $global:RoleDefinitionIds[$roleAssignment[0]]
            Write-Host "Assigning role [$($roleAssignment[0])]($($roleDefinitionId)) to $($scope)"

            $roleAssignmentRequest = [ordered]@{
                type = "FoundationaLLM.Authorization/roleAssignments"
                name = (New-Guid).ToString("D")
                description = "$($roleAssignment[0]) role for $($securityPrincipal.principal_name)"
                principal_id = $securityPrincipalId
                principal_type = $securityPrincipal.principal_type
                role_definition_id = $roleDefinitionId
                scope = $scope
            }

            try {
                $roleAssignmentResponse = Merge-RoleAssignment -RoleAssignment $roleAssignmentRequest
                Write-Host "Role assignment created: $($roleAssignmentResponse)" -ForegroundColor Green
            }
            catch [System.Net.Http.HttpRequestException] {
                if ($_.Exception.Response.StatusCode -eq 409) {
                    Write-Host "Role assignment already exists." -ForegroundColor Yellow
                } else {
                    throw $_
                }
            }
        }
    }
}
