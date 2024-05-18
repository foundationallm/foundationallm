Param(
    [parameter(Mandatory=$true)][string]$principalId
)

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

Push-Location $($MyInvocation.InvocationName | Split-Path)

$msGraphId = (az ad sp show --id '00000003-0000-0000-c000-000000000000' --output tsv --query 'id')

$msGraphRoleIds = New-Object -TypeName psobject -Property @{
    'Group.Read.All'='5b567255-7703-4780-807c-7be8301ae99b';
    'User.Read.All'='df021288-bdef-4463-88db-98f22de89214';
}

$existingRoleData = (az rest --method GET --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$($principalId)/appRoleAssignments" --output json)

$existingRoles = $($($existingRoleData | ConvertFrom-Json).value | Select-Object -ExpandProperty appRoleId)

$msGraphRoleIds.PSObject.Properties | ForEach-Object {

    Invoke-AndRequireSuccess "Assigning Microsoft Graph Role [$($_.Name) | $($_.Value)] to Principal [$($principalId)]" {

        if ($null -ne $existingRoles -and $existingRoles.Contains($_.Value)) {
            Write-Host "Role is already assigned!" -ForegroundColor Yellow
            return
        }

        $body ="{'principalId':'$($principalId)','resourceId':'$($msGraphId)','appRoleId':'$($_.Value)'}"

        az rest --method POST `
            --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$($principalId)/appRoleAssignments" `
            --headers 'Content-Type=application/json' `
            --body $body
    }
}

Pop-Location
