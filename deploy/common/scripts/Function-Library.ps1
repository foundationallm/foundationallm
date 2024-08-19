function Invoke-CLICommand {
    <#
    .SYNOPSIS
    Invoke a CLI Command and allow all output to print to the terminal.  Does not check for return values or pass the output to the caller.
    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }
}

function Get-AbsolutePath {
    <#
    .SYNOPSIS
    Get the absolute path of a file or directory. Relative path does not need to exist.
    #>
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
        [string]$RelatviePath
    )

    return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($RelatviePath)
}

function Get-ResourceGroups {
    Param(
        [parameter(Mandatory = $true)][string]$subscriptionId,
        [parameter(Mandatory = $true)][string]$azdEnvName
    )

    $script:subscriptionRgs = $null
    Invoke-CliCommand "List all resource groups in the subscription" {
        $script:subscriptionRgs = az group list `
            --subscription $subscriptionId `
            --output json | `
            ConvertFrom-Json -AsHashtable
    }

    # Is subscriptionRgs null or empty?
    if (-not $script:subscriptionRgs -or $script:subscriptionRgs.Count -eq 0) {
        Write-Host "No resource groups found in the subscription."
        return @{}
    }

    # Filter for the resource groups where the tags property exists
    $resourceGroups = @{}
    foreach ($rg in $script:subscriptionRgs) {
        if (-not ($rg.tags -is [hashtable] -and $rg.tags.ContainsKey('azd-env-name'))) {
            continue
        }

        if ($rg.tags.'azd-env-name' -eq $azdEnvName) {
            Write-Host "Found resource group: $($rg.name)"
            $resourceGroups[$rg.name] += $rg.id
        }
    }

    return $resourceGroups
}