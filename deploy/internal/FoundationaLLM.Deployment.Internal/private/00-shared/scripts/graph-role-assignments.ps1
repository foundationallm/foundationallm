function Add-MicrosoftGraphRolesToPrincipal {
    Param(
        [parameter(Mandatory=$true)][string]$principalId
    )

    $msGraphId = (az ad sp show --id '00000003-0000-0000-c000-000000000000' --output tsv --query 'id')

    $msGraphRoleIds = New-Object -TypeName psobject -Property @{
        'Group.Read.All'='5b567255-7703-4780-807c-7be8301ae99b';
        'User.Read.All'='df021288-bdef-4463-88db-98f22de89214';
        'Application.Read.All'='9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30';
        'Directory.Read.All'='7ab1d382-f21e-4acd-a863-ba3e13f7da61';
    }

    $existingRoleData = (
        az rest `
            --method GET `
            --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$($principalId)/appRoleAssignments" `
            --headers "{'Content-Type': 'application/json'}" `
            -o json)

    $existingRoles = $($($existingRoleData | ConvertFrom-Json).value | Select-Object -ExpandProperty appRoleId)

    $msGraphRoleIds.PSObject.Properties | ForEach-Object {

        Write-Host "Ensuring Microsoft Graph Role [$($_.Name) | $($_.Value)] is assigned to Principal [$($principalId)]"

        if ($null -ne $existingRoles -and $existingRoles.Contains($_.Value)) {
            Write-Host "Role is already assigned!" -ForegroundColor Yellow
        } else {
            Write-Host "Assigning Microsoft Graph Role [$($_.Name) | $($_.Value)] to Principal [$($principalId)]"
            $body ="{'principalId':'$($principalId)','resourceId':'$($msGraphId)','appRoleId':'$($_.Value)'}"

            az rest --method POST `
                --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$($principalId)/appRoleAssignments" `
                --headers 'Content-Type=application/json' `
                --body $body `
            | Out-Null
            Write-Host "Role assigned successfully." -ForegroundColor Green
        }
    }
}