Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber
Import-Module "./package/common/FoundationaLLM-Agent.psm1" -Force -NoClobber
Import-Module "./package/common/FoundationaLLM-Prompt.psm1" -Force -NoClobber

function Deploy-Package {
    param (
        [string]$PackageRoot
    )

    Write-Host "Deploying package from $($PackageRoot)" -ForegroundColor Blue
    Write-Host "Creating prompts..."

    $prompts = Get-Content "$($PackageRoot)/artifacts/prompts.json" | ConvertFrom-Json -AsHashTable
    $prompts | ForEach-Object {

        $prompt = $_

        Write-Host "Creating prompt: $($prompt.name)"

        $promptBody = (Get-Content "$($PackageRoot)/artifacts/$($prompt.name).txt" -Raw -Encoding UTF8)
        $prompt["prefix"] = $promptBody

        $promptResult = (Merge-Prompt -Prompt $prompt)
        Write-Host "Prompt created: $($promptResult)" -ForegroundColor Green
    }

    $agent = Get-Content "$($PackageRoot)/artifacts/agent.json" | ConvertFrom-Json -AsHashTable

    $workflow = Get-Content "$($PackageRoot)/artifacts/workflow.json" | ConvertFrom-Json -AsHashTable
    $resourceObjectIds = (Get-ResourceObjectIds -Resources $workflow.resources)
    $workflow["resource_object_ids"] = $resourceObjectIds
    $workflow.Remove("resources")
    $agent["workflow"] = $workflow
    $agent["tools"] = @()

    $tools = Get-Content "$($PackageRoot)/artifacts/tools.json" | ConvertFrom-Json -AsHashTable
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