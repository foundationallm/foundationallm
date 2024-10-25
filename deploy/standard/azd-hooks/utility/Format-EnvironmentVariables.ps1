#!/usr/bin/env pwsh

function envsubst {
    param([Parameter(ValueFromPipeline)][string]$InputObject)

    $ExecutionContext.InvokeCommand.ExpandString($InputObject)
}

function Format-EnvironmentVariables {
    param(
        [Parameter(Mandatory = $true)][string]$template,
        [Parameter(Mandatory = $true)][string]$render
    )

    $content = Get-Content $template
    $result = @()
    foreach ($line in $content) {
        $result += $line | envsubst
    }

    $result | Out-File $render -Force
}

