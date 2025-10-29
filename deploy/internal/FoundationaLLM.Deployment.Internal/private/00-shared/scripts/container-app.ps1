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
        [int]$MaxReplicas
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
            --workload-profile-name "Warm"
            | Out-Null

        Write-Host "Container app $ContainerAppName created successfully."
    } else {
        Write-Host "Container app $ContainerAppName already exists."
    }
}