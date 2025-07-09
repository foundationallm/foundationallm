function Update-PluginPackage {
    param (
        [string]$PackageName,
        [string]$NuGetPackageName,
        [string]$NuGetPackageVersion
    )

    $nugetUrl = "https://api.nuget.org/v3-flatcontainer/$NuGetPackageName/$NuGetPackageVersion/$NuGetPackageName.$NuGetPackageVersion.nupkg"
    $packagePath = Join-Path "$([System.IO.Path]::GetTempPath())" "$NuGetPackageName.$NuGetPackageVersion.nupkg"

    Invoke-RestMethod -Uri $nugetUrl -OutFile $packagePath

    $form = @{
        file = Get-Item -Path $packagePath
        resource = "{`"type`": `"plugin-package`",`"name`": `"$PackageName`"}"
    }

    Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Plugin/pluginPackages/$($PackageName)" `
        -Form $form
}