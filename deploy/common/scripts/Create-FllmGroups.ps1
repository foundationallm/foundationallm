#! /usr/bin/pwsh
<#
.SYNOPSIS 
    Creates Entra ID groups using the Azure CLI and adds the current user to the group. 
 
.DESCRIPTION 
    This script creates a new Entra ID groups defined in a manifest using the Azure CLI command. 
    The user running the script is added to the groups after creation. 
	It checks for errors during execution and outputs the status of group creation. 
 
.PARAMETER manifest 
    Specifies the path to the manifest of the Entra ID groups to be created. An example of
    a group manifest:

    ```json
    [
        {
            "name": "FLLM-Admins",
            "members": [

            ]
        },
        {
            "name": "FLLM-Users",
            "members": [
                
            ]
        }
    ]
    ```
 
.EXAMPLE 
    ./Create-FllmGroups.ps1 -manifest "path/to/group/manifest" 
    This example runs the script to create an Entra ID groups defined in the group manifest. 
#>
 
Param( 
    [parameter(Mandatory = $false)][string]$manifest = "../config/entraGroups.json"
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Try block to handle potential errors during the execution 
try { 
    $groups = Get-Content -Raw $manifest | ConvertFrom-Json

    $currentUserId = (az ad signed-in-user show --query id -o tsv).Trim() 
 
    foreach ($group in $groups)
    {
        # Check if the group already exists 
        Write-Host -ForegroundColor Yellow "Checking if the Entra ID group '$($group.name)' already exists..." 
        az ad group show --group $($group.name) 
    
        if ($LASTEXITCODE -eq 0) { 
            Write-Host -ForegroundColor Red "The Entra ID group '$($group.name)' already exists."  
        } else {
            # Command to create the Entra ID group using Azure CLI 
            Write-Host -ForegroundColor Yellow "Creating Entra ID group '$($group.name)'..." 
            az ad group create --display-name $($group.name) --mail-nickname $($group.name) 
            
            if ($LASTEXITCODE -ne 0) { 
                throw "Failed to create group ${message} (code: ${LASTEXITCODE})" 
            } 
        
            Write-Host -ForegroundColor Blue "Waiting for group creation to complete..." 
            Start-Sleep 10 
            
            # If the command executes successfully, output the result 
            Write-Host -ForegroundColor Blue "Entra ID group '$($group.name)' created successfully, and added current user with ID $currentUserId to the group."
        }

        # Get the ID of the of the user running the script & add the user to the group 
        Write-Host -ForegroundColor Yellow "Adding users to the group..." 

        az ad group member add --group $($group.name) --member-id $currentUserId 
        foreach ($account in $group.members) {
            $objectId = $(az ad user show --id $account --query "id" -o tsv)
            if ($objectId) {
                az ad group member add --group $($group.name) --member-id $objectId 
            }
        }
    }
}  
catch { 
    # Catch block to handle and report any errors that occur during the execution 
    Write-Host -ForegroundColor Red "Failed to create Entra ID group. Error: $($_.Exception.Message)" 
}
