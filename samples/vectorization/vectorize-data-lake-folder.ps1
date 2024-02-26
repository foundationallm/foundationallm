# ---------------------------------------------------------
# FoundationaLLM Samples
# This sample demonstrates how to vectorize all files in a folder of an Azure Data Lake Storage account.
# ---------------------------------------------------------

# Indicates whether the script is running in a development environment or not.
$developmentEnvironment = $false
# The name of the subscription where the FoundationaLLM instance is deployed.
$subscriptioName = "..."
# The name of the App Configuration service that stores configuration settings for the FoundationaLLM instance.
$appConfigName = "..."
# The name of the Key Vault that stores secrets associated with App Configuration service.
$keyVaultName = "..."

# The names of configuration settings storing the URL of the Vectorization API and the API key.
# NOTE: These values should not be changed.
$vectorizationAPIUrlConfigName = "FoundationaLLM:APIs:VectorizationAPI:APIUrl"
$vectorizationAPIKeySecretName = "foundationallm-apis-vectorizationapi-apikey"
# $vectorizationApplicationIdUri = "api://FoundationaLLM-Vectorization"

# The name of the storage account where the input files are stored.
$vectorizationInputStorageAccountName = "..."
# The name of the container where the input files are stored.
# NOTE: Can be a different container then the standard "vectorization-input" container.
$vectorizaitonInputContainerName = "vectorization-input"
# The path to the root folder containing the input files.
$vectorizationInputFolderPath = "..."
# Indicates whether the script should retrieve files from subfolders of the input folder.
$recursiveFileRetrieval = $true
# The name of the root folder where the vectorization states will be stored.
$vectorizationCanonicalRootPath = "..."

# The name of the content source used for vectorization.
$contentSourceProfile = "..."
# The name of the text partitioning profile used for vectorization.
$textPartitionProfile = "..."
# The name of the text embedding profile used for vectorization.
$textEmbeddingProfile = "..."
# The name of the indexing profile used for vectorization.
$indexingProfile = "..."

az login
az account set --subscription $subscriptioName

# $accessToken = (az account get-access-token --resource $vectorizationApplicationIdUri --query accessToken -o tsv)

if ($developmentEnvironment) {
    $vectorizationAPIUrl = "https://localhost:7047"
} else {
    $vectorizationAPIUrl = (az appconfig kv show --name $appConfigName --key $vectorizationAPIUrlConfigName --query value -o tsv)
}
$vectorizationAPIKey = (az keyvault secret show --name $vectorizationAPIKeySecretName --vault-name $keyVaultName --query value -o tsv)


$filesToVectorize = (az storage fs file list `
    -f $vectorizaitonInputContainerName `
    --path $vectorizationInputFolderPath `
    --account-name $vectorizationInputStorageAccountName `
    --auth-mode login `
    --recursive $recursiveFileRetrieval `
    --query "[?!isDirectory].name" -o tsv)

foreach ($filePath in $filesToVectorize) {
    
    $tokens = $filePath.Split("/")
    $fileName = $tokens[-1]
    $fileNameNoExtension = Split-Path -Path $fileName -LeafBase

    $tokens[0] = $vectorizationCanonicalRootPath
    $tokens[-1] = $fileNameNoExtension

    write-host "Vectorizing file: $($filePath)..."

    $vectorizationRequest = @"
    {
        "content_identifier": {
            "content_source_profile_name": "$($contentSourceProfile)",
            "multipart_id": [
                "$($vectorizationInputStorageAccountName).dfs.core.windows.net",
                "$($vectorizaitonInputContainerName)",
                "$($filePath)"
            ],
            "canonical_id": "$($tokens -join '/')"
        },
        "processing_type": "Asynchronous",
        "steps": [
            {
                "id": "extract",
                "parameters": {
                }
            },
            {
                "id": "partition",
                "parameters": {
                    "text_partition_profile_name": "$($textPartitionProfile)"
                }
            },
            {
                "id": "embed",
                "parameters": {
                    "text_embedding_profile_name": "$($textEmbeddingProfile)"
                }
            },
            {
                "id": "index",
                "parameters": {
                    "indexing_profile_name": "$($indexingProfile)"
                }
            }
        ]
    }
"@
    $headers = @{
        "X-API-KEY" = $vectorizationAPIKey
    }
    $body = $vectorizationRequest | ConvertFrom-Json -Depth 100
    $response = Invoke-RestMethod `
        -Uri "$($vectorizationAPIUrl)/vectorizationrequest" `
        -Method Post `
        -Headers $headers `
        -Body $vectorizationRequest `
        -ContentType "application/json"
    write-host "Vectorization response: $($response | ConvertTo-Json -Depth 100)"
}
