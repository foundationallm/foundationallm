function Resolve-Placeholders {
    param (
        [Parameter(ValueFromPipeline)]
        [string]$Content,
        [hashtable]$Parameters = @{}
    )

    process {
        foreach ($key in $Parameters.Keys) {
            $placeholder = "{{" + $key + "}}"
            $value = $Parameters[$key]
            $Content = $Content -replace $placeholder, $value
        }
    
        $Content
    }
}

function Import-Dotenv {
    param (
        [string]$FilePath
    )

    if (-not (Test-Path -Path $FilePath)) {
        Write-Error "File not found: $FilePath"
        return
    }

    Get-Content -Path $FilePath | ForEach-Object {
        if ($_ -match '^\s*#') { return } # Skip comments
        if ($_ -match '^\s*$') { return } # Skip empty lines

        $parts = $_ -split '=', 2
        if ($parts.Count -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim().Trim('"').Trim("'")
            [System.Environment]::SetEnvironmentVariable($key, $value, [System.EnvironmentVariableTarget]::Process)
        }
    }
}

function Deploy-FoundationaLLMPackage {
    param (
        [string]$PackageRoot,
        [hashtable]$Parameters = @{}
    )

    Write-Host "Deploying package from $($PackageRoot)" -ForegroundColor Blue

    if (Test-Path -Path "$($PackageRoot)/artifacts/prompts.json") {

        Write-Host "Creating prompts..."

        $prompts = Get-Content "$($PackageRoot)/artifacts/prompts.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable
        
        $prompts | ForEach-Object {

            $prompt = $_

            Write-Host "Creating prompt: $($prompt.name)"

            if ($prompt["prefix"].EndsWith(".txt")) {
                # The actual content of the prompt is in a TXT file.
                $promptBody = Get-Content "$($PackageRoot)/artifacts/$($prompt["prefix"])" -Raw -Encoding UTF8 `
                    | Resolve-Placeholders -Parameters $Parameters
                $prompt["prefix"] = $promptBody
            }

            $promptResult = (Merge-Prompt -Prompt $prompt)
            Write-Host "Prompt created: $($promptResult)" -ForegroundColor Green
        }
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/agent.json") {
    
        Write-Host "Updating agent..."

        $agent = Get-Content "$($PackageRoot)/artifacts/agent.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        $workflow = Get-Content "$($PackageRoot)/artifacts/workflow.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable
        
        $resourceObjectIds = (Get-ResourceObjectIds -Resources $workflow.resources)
        $workflow["resource_object_ids"] = $resourceObjectIds
        $workflow.Remove("resources")
        $agent["workflow"] = $workflow
        $agent["tools"] = @()

        $tools = Get-Content "$($PackageRoot)/artifacts/tools.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        $tools | ForEach-Object {
            if ((-not $Parameters.ContainsKey("AGENT_TOOLS")) -or $Parameters["AGENT_TOOLS"].Contains($_.name)) {
                
                $tool = $_
                $toolResourceObjectIds = (Get-ResourceObjectIds -Resources $tool.resources)
                $tool["resource_object_ids"] = $toolResourceObjectIds
                $tool.Remove("resources")
                $agent["tools"] += $tool
            }
        }

        Write-Host "Updating agent: $($agent.name)"
        $agentResult = (Merge-Agent -Agent $agent)
        Write-Host "Agent updated: $($agentResult)" -ForegroundColor Green
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/roleAssignments.json") {
    
        Write-Host "Updating role assignments..."

        Merge-RoleAssignments -PackageRoot $PackageRoot -Parameters $Parameters
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/plugins.json") {

        Write-Host "Updating plugin packages..."

        $plugins = Get-Content "$($PackageRoot)/artifacts/plugins.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        foreach ($plugin in $plugins.dotnet) {

            Write-Host "Updating plugin package: $($plugin[0])"
            $pluginResult = Merge-PluginPackage `
                -PackageName $plugin[0] `
                -NuGetPackageName $plugin[1] `
                -NuGetPackageVersion $plugin[2] `
                -PackagePath $plugin[3]
            Write-Host "Plugin updated: $($pluginResult)" -ForegroundColor Green
        }
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/vectorDatabases.json") {

        Write-Host "Updating vector databases..."

        $vectorDatabases = Get-Content "$($PackageRoot)/artifacts/vectorDatabases.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        foreach ($vectorDatabase in $vectorDatabases) {

            Write-Host "Updating vector database: $($vectorDatabase.name)"
            $vectorDatabaseResult = Merge-VectorDatabase `
                -VectorDatabase $vectorDatabase
            Write-Host "Vector database updated: $($vectorDatabaseResult)" -ForegroundColor Green
        }
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/appConfigurations.json") {

        Write-Host "Updating app configurations..."


        $keyVaultURIResponse = Get-AppConfiguration -Name "FoundationaLLM:Configuration:KeyVaultURI"
        $keyVaultURI = $keyVaultURIResponse.resource.value

        Write-Host "Key Vault URI: $keyVaultURI"

        # Add KeyVaultURI to the parameters dictionary
        $Parameters["{{KEY_VAULT_URI}}"] = $keyVaultURI

        $appConfigurations = Get-Content "$($PackageRoot)/artifacts/appConfigurations.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        foreach ($appConfiguration in $appConfigurations) {

            Write-Host "Updating app configuration: $($appConfiguration.name)"
            $appConfigurationResult = Merge-AppConfiguration -Configuration $appConfiguration
            Write-Host "App configuration updated: $($appConfigurationResult)" -ForegroundColor Green
        }
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/dataSources.json") {

        Write-Host "Updating data sources..."

        $dataSources = Get-Content "$($PackageRoot)/artifacts/dataSources.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        foreach ($dataSource in $dataSources) {

            Write-Host "Updating data source: $($dataSource.name)"
            $dataSourceResult = Merge-DataSource -DataSource $dataSource
            Write-Host "Data source updated: $($dataSourceResult)" -ForegroundColor Green
        }
    }

    if (Test-Path -Path "$($PackageRoot)/artifacts/dataPipelines.json") {

        Write-Host "Updating data pipelines..."

        $dataPipelines = Get-Content "$($PackageRoot)/artifacts/dataPipelines.json" `
            | Resolve-Placeholders -Parameters $Parameters `
            | ConvertFrom-Json -AsHashTable

        foreach ($dataPipeline in $dataPipelines) {

            Write-Host "Updating data pipeline: $($dataPipeline.name)"
            Merge-DataPipeline -DataPipeline $dataPipeline
            Write-Host "Data pipeline updated: $($dataPipeline.name)" -ForegroundColor Green
        }
    }
}
