#! /usr/bin/pwsh
<#
.SYNOPSIS 
    Creates an Entra ID group using the Azure CLI and adds the current user to the group. 
 
.DESCRIPTION 
    This script creates a new Entra ID group named 'FLLM-Admins' using the Azure CLI command. 
    The user running the script is added to the group after its creation. 
	It checks for errors during execution and outputs the status of group creation. 
 
.PARAMETER groupName 
    Specifies the name of the Entra ID group to be created. 
 
.EXAMPLE 
    ./Create-FllmAdminGroup.ps1 -groupName "FLLM-Admins" 
    This example runs the script to create an Entra ID group named 'FLLM-Admins'. 
#>
 
Param( 
    [parameter(Mandatory = $false)][string]$groupName = "FLLM-Admins"
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"
 
# Try block to handle potential errors during the execution 
try { 
    # Check if the group already exists 
    Write-Host -ForegroundColor Yellow "Checking if the Entra ID group '$groupName' already exists..." 
    az ad group show --group $groupName 
 
    if ($LASTEXITCODE -eq 0) { 
        Write-Host -ForegroundColor Red "The Entra ID group '$groupName' already exists. Script execution stopped." 
        exit 
    } 
 
    # Command to create the Entra ID group using Azure CLI 
    Write-Host -ForegroundColor Yellow "Creating Entra ID group '$groupName'..." 
    az ad group create --display-name $groupName --mail-nickname $groupName 
    
    if ($LASTEXITCODE -ne 0) { 
        throw "Failed to create group ${message} (code: ${LASTEXITCODE})" 
    } 
 
    Write-Host -ForegroundColor Blue "Waiting for group creation to complete..." 
    Start-Sleep 10 
  
    # Get the ID of the of the user running the script & add the user to the group 
    $currentUserId = (az ad signed-in-user show --query id -o tsv).Trim() 
    Write-Host -ForegroundColor Yellow "Adding users to the group..." 

    az ad group member add --group $groupName --member-id $currentUserId 
    $memberObjectIds = @($currentUserId)
    if (Test-Path ../config/adminGroupMembers.json -PathType Leaf) {
        $accounts = Get-Content ../config/adminGroupMembers.json -Raw | ConvertFrom-Json
        foreach ($account in $accounts) {
            $objectId = $(az ad user show --id $account --query "id" -o tsv)
            if ($objectId) {
                az ad group member add --group $groupName --member-id $objectId 
            }
        }
    }

    az ad group owner add --group $groupName --owner-object-id $currentUserId
    Write-Host -ForegroundColor Yellow "Setting owners of the group..." 
    $adminObjectIds = @($currentUserId)
    if (Test-Path ../config/admins.json -PathType Leaf) {
        $accounts = Get-Content ../config/admins.json -Raw | ConvertFrom-Json
        foreach ($account in $accounts) {
            $objectId = $(az ad user show --id $account --query "id" -o tsv)
            if ($objectId) {
                az ad group owner add --group $groupName --owner-object-id $objectId 
            }
        }
    }
    
    # If the command executes successfully, output the result 
    Write-Host -ForegroundColor Blue "Entra ID group '$groupName' created successfully, and added current user with ID $currentUserId to the group." 
}  
catch { 
    # Catch block to handle and report any errors that occur during the execution 
    Write-Host -ForegroundColor Red "Failed to create Entra ID group. Error: $($_.Exception.Message)" 
}
