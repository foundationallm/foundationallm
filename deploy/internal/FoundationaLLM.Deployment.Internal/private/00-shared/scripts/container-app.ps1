function Initialize-ContainerApp {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$ContainerAppName,
        [string]$ContainerAppIdentity,
        [string[]]$EnvironmentVariables,
        [string[]]$Secrets,
        [string]$ContainerImage,
        [int]$MinReplicas,
        [int]$MaxReplicas,
        [decimal]$CPUCores,
        [int]$Memory
    )

    if ((az containerapp list --resource-group $ResourceGroupName --query "[?name=='$ContainerAppName']" -o tsv).Count -eq 0) {

        Write-Host "Creating container app $ContainerAppName in resource group $ResourceGroupName..."

        az containerapp create `
            --name $ContainerAppName `
            --resource-group $ResourceGroupName `
            --environment $ResourceNames.ContainerAppsEnvironment `
            --image $ContainerImage `
            --user-assigned $(az identity show --name $ContainerAppIdentity --resource-group $ResourceGroupName --query id -o tsv) `
            --ingress external `
            --target-port 80 `
            --secrets @Secrets `
            --env-vars @EnvironmentVariables `
            --min-replicas $MinReplicas `
            --max-replicas $MaxReplicas `
            --workload-profile-name "Warm" `
            --cpu $CPUCores `
            --memory "${Memory}Gi" `
            | Out-Null

        Write-Host "Container app $ContainerAppName created successfully."
    } else {
        Write-Host "Container app $ContainerAppName already exists."
    }
}

function Restart-ContainerApp {
    param (
        [string]$ResourceGroupName,
        [string]$ContainerAppName
    )

    Write-Host "Retrieving revision for container app: $ContainerAppName"
    $revision = (az containerapp revision list -n $ContainerAppName -g $ResourceGroupName --query "[0].name" --output tsv)
    Write-Host "Restarting container app: $ContainerAppName with revision: $revision"
    az containerapp revision restart --revision $revision -g $ResourceGroupName | Out-Null
}