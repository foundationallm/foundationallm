function Merge-PluginPackage {
    param (
        [string]$PackageName,
        [string]$NuGetPackageName,
        [string]$NuGetPackageVersion,
        [string]$PackagePath
    )

    if (-not $PackagePath) {
        $nugetUrl = "https://api.nuget.org/v3-flatcontainer/$NuGetPackageName/$NuGetPackageVersion/$NuGetPackageName.$NuGetPackageVersion.nupkg"
        $PackagePath = Join-Path "$([System.IO.Path]::GetTempPath())" "$NuGetPackageName.$NuGetPackageVersion.nupkg"

        Invoke-RestMethod -Uri $nugetUrl -OutFile $PackagePath
    }

    $form = @{
        file = Get-Item -Path $PackagePath
        resource = "{`"type`": `"plugin-package`",`"name`": `"$PackageName`"}"
    }

    Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Plugin/pluginPackages/$($PackageName)" `
        -Form $form
}