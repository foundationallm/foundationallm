
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
