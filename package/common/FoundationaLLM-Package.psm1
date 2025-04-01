Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber
Import-Module "./package/common/FoundationaLLM-Agent.psm1" -Force -NoClobber
Import-Module "./package/common/FoundationaLLM-Prompt.psm1" -Force -NoClobber

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

function Deploy-Package {
    param (
        [string]$PackageRoot,
        [hashtable]$Parameters = @{}
    )

    Write-Host "Deploying package from $($PackageRoot)" -ForegroundColor Blue
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
        $tool = $_
        $toolResourceObjectIds = (Get-ResourceObjectIds -Resources $tool.resources)
        $tool["resource_object_ids"] = $toolResourceObjectIds
        $tool.Remove("resources")
        $agent["tools"] += $tool
    }

    Write-Host "Creating agent: $($agent.name)"
    $agentResult = (Merge-Agent -Agent $agent)
    Write-Host "Agent created: $($agentResult)" -ForegroundColor Green
}