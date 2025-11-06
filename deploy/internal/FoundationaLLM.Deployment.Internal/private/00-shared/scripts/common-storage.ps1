function Initialize-CosmosDBContainer {
    param (
        [string]$ResourceGroupName,
        [string]$AccountName,
        [string]$DatabaseName,
        [string]$ContainerName,
        [string]$PartitionKeyPath,
        [bool]$HierarchicalPartitionKey,
        [int]$MaxThroughput,
        [int]$TTL
    )

    Write-Host "Ensuring Cosmos DB container $ContainerName exists in database $DatabaseName..."
    if ((az cosmosdb sql container list `
        --account-name $AccountName `
        --resource-group $ResourceGroupName `
        --database-name $DatabaseName `
        --query "[?name=='$($ContainerName)']" -o tsv).Count -gt 0) {
        
        Write-Host "Cosmos DB container '$ContainerName' already exists."
        return
    }

    Write-Host "Creating Cosmos DB container $ContainerName in database $DatabaseName..."

    if ($HierarchicalPartitionKey) {
        az cosmosdb sql container create `
            --account-name $cosmosDBAccountName `
            --resource-group $resourceGroupName `
            --database-name $DatabaseName `
            --name $ContainerName `
            --partition-key-path $PartitionKeyPath `
            --partition-key-version 2 `
            --max-throughput $MaxThroughput `
            --ttl $TTL| Out-Null
    } else {

        az cosmosdb sql container create `
            --account-name $cosmosDBAccountName `
            --resource-group $resourceGroupName `
            --database-name $DatabaseName `
            --name $ContainerName `
            --partition-key-path $PartitionKeyPath `
            --max-throughput $MaxThroughput `
            --ttl $TTL| Out-Null
    }

    Write-Host "Cosmos DB container '$ContainerName' created."
}

function Initialize-CommonStorage {
    param (
        [string]$UniqueName,
        [string]$Location,
        [string]$InstanceId
    )

    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName
    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $coreStorageAccountName = $resourceNames.CoreStorageAccount
    $contextStorageAccountName = $resourceNames.ContextStorageAccount
    $cosmosDBAccountName = $resourceNames.CosmosDBAccount

    Write-Host "Registering namespaces for common services..."
    az provider register --namespace Microsoft.Storage

    Write-Host "Ensuring storage account $coreStorageAccountName exists in resource group $($resourceGroupNames.Data)..."
    if (-not (az storage account check-name `
        --name $coreStorageAccountName | ConvertFrom-Json).nameAvailable) {
        Write-Host "Storage account $coreStorageAccountName already exists."
    } else {
        az storage account create `
            --name $coreStorageAccountName `
            --resource-group $($resourceGroupNames.Data) `
            --location $Location `
            --sku Standard_LRS `
            --kind StorageV2 `
            --allow-shared-key-access $false `
            --enable-hierarchical-namespace $true `
            --min-tls-version TLS1_2 | Out-Null

        az storage account blob-service-properties update `
            --account-name $coreStorageAccountName `
            --enable-delete-retention $true `
            --delete-retention-days 30 `
            --enable-container-delete-retention $true `
            --container-delete-retention-days 30 | Out-Null
        Write-Host "Storage account $coreStorageAccountName created."
    }

    Write-Host "Ensuring storage account $contextStorageAccountName exists in resource group $($resourceGroupNames.Context)..."
    if (-not (az storage account check-name `
        --name $contextStorageAccountName | ConvertFrom-Json).nameAvailable) {
        Write-Host "Storage account $contextStorageAccountName already exists."
    } else {
        az storage account create `
            --name $contextStorageAccountName `
            --resource-group $($resourceGroupNames.Context) `
            --location $Location `
            --sku Standard_LRS `
            --kind StorageV2 `
            --allow-shared-key-access $false `
            --enable-hierarchical-namespace $true `
            --min-tls-version TLS1_2 | Out-Null

        az storage account blob-service-properties update `
            --account-name $contextStorageAccountName `
            --enable-delete-retention $true `
            --delete-retention-days 30 `
            --enable-container-delete-retention $true `
            --container-delete-retention-days 30 | Out-Null
        Write-Host "Storage account $contextStorageAccountName created."
    }

    Write-Host "Ensuring Cosmos DB account $cosmosDBAccountName exists in resource group $($resourceGroupNames.Data)..."
    if ((az cosmosdb list `
        -g $($resourceGroupNames.Data) `
        --query "[?name=='$($cosmosDBAccountName)']" -o tsv).Count -eq 0) {

        Write-Host "Creating Cosmos DB account $cosmosDBAccountName in resource group $($resourceGroupNames.Data)..."
        az cosmosdb create `
            --name $cosmosDBAccountName `
            --resource-group $($resourceGroupNames.Data) `
            --kind GlobalDocumentDB `
            --default-consistency-level Session `
            --enable-automatic-failover false `
            --capabilities EnableNoSQLVectorSearch | Out-Null

        az resource update `
            --resource-group $($resourceGroupNames.Data) `
            --name $cosmosDBAccountName `
            --resource-type "Microsoft.DocumentDB/databaseAccounts" `
            --set properties.disableLocalAuth=true | Out-Null
        Write-Host "Cosmos DB account '$cosmosDBAccountName' created."
    }

    $storageContainers = @(
        "orchestration-completion-requests",
        "quota",
        "resource-provider"
    )

    foreach ($containerName in $storageContainers) {
        Write-Host "Ensuring storage container $containerName exists in storage account $coreStorageAccountName..."
        if ((az storage container list `
            --account-name $coreStorageAccountName `
            --auth-mode login `
            --query "[?name=='$($containerName)']" -o tsv).Count -eq 0) {
            
            az storage container create `
                --account-name $coreStorageAccountName `
                --auth-mode login `
                --name $containerName | Out-Null
            Write-Host "Storage container '$containerName' created."
        } else {
            Write-Host "Storage container '$containerName' already exists."
        }
    }

    $storageContainers = @(
        $InstanceId
    )

    foreach ($containerName in $storageContainers) {
        Write-Host "Ensuring storage container $containerName exists in storage account $contextStorageAccountName..."
        if ((az storage container list `
            --account-name $contextStorageAccountName `
            --auth-mode login `
            --query "[?name=='$($containerName)']" -o tsv).Count -eq 0) {
            
            az storage container create `
                --account-name $contextStorageAccountName `
                --auth-mode login `
                --name $containerName | Out-Null
            Write-Host "Storage container '$containerName' created."
        } else {
            Write-Host "Storage container '$containerName' already exists."
        }
    }

    $queues = @(
        "frontend-worker",
        "backend-worker"
    )   

   foreach ($queueName in $queues) {
       Write-Host "Ensuring storage queue $queueName exists in storage account $contextStorageAccountName..."
       if ((az storage queue list `
           --account-name $contextStorageAccountName `
           --auth-mode login `
           --query "[?name=='$($queueName)']" -o tsv).Count -eq 0) {

           az storage queue create `
               --account-name $contextStorageAccountName `
               --auth-mode login `
               --name $queueName | Out-Null
           Write-Host "Storage queue '$queueName' created."
       } else {
           Write-Host "Storage queue '$queueName' already exists."
       }
   }

    Write-Host "Ensuring Cosmos DB database $($resourceNames.CosmosDBDatabase) exists in account $cosmosDBAccountName..."
    if ((az cosmosdb sql database list `
        --account-name $cosmosDBAccountName `
        --resource-group $($resourceGroupNames.Data) `
        --query "[?name=='$($resourceNames.CosmosDBDatabase)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Cosmos DB database $($resourceNames.CosmosDBDatabase) in account $cosmosDBAccountName..."
        az cosmosdb sql database create `
            --account-name $cosmosDBAccountName `
            --resource-group $($resourceGroupNames.Data) `
            --name $resourceNames.CosmosDBDatabase | Out-Null
        Write-Host "Cosmos DB database '$($resourceNames.CosmosDBDatabase)' created."
    }

    $cosmosDBContainers = @(
        @{
            Name = "Attachments"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/upn"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "CompletionsCache"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/operationId"
            MaxThroughput = 1000
            TTL = 300
        },
        @{
            Name = "Context"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/upn"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "DataPipelines"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/run_id"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "ExternalResources"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/partitionKey"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "leases"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/id"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "Operations"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/id"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "Sessions"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/sessionId"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "State"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/operation_id"
            MaxThroughput = 1000
            TTL = 604800
        },
        @{
            Name = "UserProfiles"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/upn"
            MaxThroughput = 1000
            TTL = -1
        },
        @{
            Name = "UserSessions"
            HierarchicalPartitionKey = $false
            PartitionKeyPath = "/upn"
            MaxThroughput = 1000
            TTL = -1
        }
    )

    foreach ($container in $cosmosDBContainers) {
        Initialize-CosmosDBContainer `
            -ResourceGroupName $($resourceGroupNames.Data) `
            -AccountName $cosmosDBAccountName `
            -DatabaseName $resourceNames.CosmosDBDatabase `
            -ContainerName $container.Name `
            -PartitionKeyPath $container.PartitionKeyPath `
            -HierarchicalPartitionKey $container.HierarchicalPartitionKey `
            -MaxThroughput $container.MaxThroughput `
            -TTL $container.TTL
    }
}
