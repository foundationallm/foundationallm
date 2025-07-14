

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
            $scope = Get-ObjectId -Name $roleAssignment[2] -Type $roleAssignment[1]
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
