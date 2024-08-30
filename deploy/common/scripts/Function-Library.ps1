function Connect-AksCluster {
    param(
        [parameter(Mandatory = $true)]
        [ValidateSet("frontend", "backend")]
        [string]$clusterRole,

        [parameter(Mandatory = $true)]
        [string]$resourceGroup
    )

    $script:clusterName = $null
    Invoke-CLICommand "Get the $($clusterRole) AKS cluster name" {
        $script:clusterName = az aks list `
            --resource-group $resourceGroup `
            --query "[?contains(name, '$($clusterRole)')].name | [0]" `
            --output tsv
    }

    Invoke-CLICommand "Retrieving credentials for $($script:clusterName)" {
        az aks get-credentials `
            --name $script:clusterName `
            --resource-group $resourceGroup `
            --overwrite-existing
    }
}

function Format-Template {
    param(
        [parameter(Mandatory = $true, Position = 0)][hashtable]$tokens,
        [parameter(Mandatory = $true, Position = 1)][string]$template
    )

    $templatePath = $template | Resolve-Path
    Write-Host "Template: $templatePath" -ForegroundColor Blue

    $content = Get-Content -Raw $templatePath

    Write-Host "Replacing tokens..." -ForegroundColor Yellow
    foreach ($token in $tokens.Keys) {
        Write-Host "Replacing $($token) ..." -ForegroundColor Yellow
        $content = $content -replace "{{$($token)}}", $tokens[$token]
    }

    return $content
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

function Get-AppRegistrationObjectId {
    param(
        [parameter(Mandatory = $true)][string]$displayName
    )

    $script:objectId = $null
    Invoke-CLICommand "Get the object ID for the application registration" {
        $script:objectId = az ad app list `
            --display-name $displayName `
            --query "[].{id:id,displayName:displayName}[?displayName=='$displayName'].id" `
            --output tsv
    }

    return $script:objectId
}

function Get-OAuthCallbackUris {
    param(
        [parameter(Mandatory = $true)][string]$applicationUri
    )

    $script:redirects = $null
    Invoke-CLICommand "Get the redirect URIs for the application" {
        $script:redirects = az rest `
            --method "get" `
            --uri $applicationUri `
            --headers "{'Content-Type': 'application/json'}" `
            --query "spa.redirectUris" `
            -o json | ConvertFrom-Json -AsHashtable
    }

    return $script:redirects
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

function Import-AzdEnvironment {
    if (-not (Test-Path "./.azure" -PathType Container)) {
        throw "The `.azure` folder is missing. Please run this function from an AZD project folder."
    }

    $script:azdEnvironment = $null
    Invoke-CLICommand "Loading AZD environment variables" {
        $script:azdEnvironment = azd env get-values
    }

    foreach ($variable in $script:azdEnvironment) {
        $name, $value = $variable.split('=')
        $value = $value.Trim('"')
        Set-Content env:\$name $value
        Write-Host $name "=" $value
    }
}

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

function Remove-OAuthCallbackUri {
    param(
        [parameter(Mandatory = $true)][string]$applicationId,
        [parameter(Mandatory = $true)][string]$redirectUri
    )

    $applicationUri = "https://graph.microsoft.com/v1.0/applications/$applicationId"
    $script:redirects = Get-OAuthCallbackUris -applicationUri $applicationUri
    if ($null -eq $script:redirects) {
        $script:redirects = @()
    }

    if (-not $script:redirects.Contains($redirectUri)) {
        Write-Host "The redirect URI does not exist. Skipping removal." -ForegroundColor Yellow
        return
    }

    $script:redirects = $script:redirects | Where-Object { $_ -ne $redirectUri }

    Update-RedirectUris `
        -applicationUri $applicationUri `
        -redirectUris $script:redirects
}

function Show-AzdEnvironments {
    if (-not (Test-Path "./.azure" -PathType Container)) {
        throw "The `.azure` folder is missing. Please run this function from an AZD project folder."
    }

    $message = @"
Listing your AZD environments. The following script will operate within the
default environment displayed below
"@
    Invoke-CLICommand $message {
        azd env list
    }
}

function Test-EnvironmentVariables {
    param(
        [parameter(Mandatory = $true)][hashtable]$envVariables
    )

    foreach ($envVariable in $envVariables.GetEnumerator()) {
        $name = $envVariable.Key
        $message = $envVariable.Value

        $value = Get-ChildItem env:$name -ErrorAction SilentlyContinue
        if (-not $value) {
            throw "$message. The '$name' environment variable is not set."
        }
    }
}

function Update-OAuthCallbackUri {
    param(
        [parameter(Mandatory = $true)][string]$applicationId,
        [parameter(Mandatory = $true)][string]$redirectUri
    )

    $applicationUri = "https://graph.microsoft.com/v1.0/applications/$applicationId"
    $script:redirects = Get-OAuthCallbackUris -applicationUri $applicationUri
    if ($null -eq $script:redirects) {
        $script:redirects = @()
    }

    if (-not $script:redirects.Contains($redirectUri)) {
        $script:redirects += $redirectUri
    }

    Update-RedirectUris `
        -applicationUri $applicationUri `
        -redirectUris $script:redirects
}

function Update-RedirectUris {
    param(
        [parameter(Mandatory = $true)][string]$applicationUri,
        [parameter(Mandatory = $true)][string[]]$redirectUris
    )
    $body = @{
        spa = @{
            redirectUris = $redirectUris
        }
    }
    $body = $body | ConvertTo-Json -Compress

    Invoke-CLICommand "Update the redirect URIs for the application" {
        az rest `
            --method "patch" `
            --uri $applicationUri `
            --headers "{'Content-Type': 'application/json'}" `
            --body $body
    }
}