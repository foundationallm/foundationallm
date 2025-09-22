function Merge-PluginPackage {
    param (
        [string]$Platform,
        [string]$PackageName,
        [string]$PublishedPackageName,
        [string]$PublishedPackageVersion,
        [string]$PackagePath
    )

    if (-not $PackagePath) {

        if ($Platform -eq "python") {
            
            $packageMetadata = Invoke-RestMethod -Uri "https://pypi.org/pypi/$PublishedPackageName/$PublishedPackageVersion/json"
            $wheelEntry = $packageMetadata.urls |
                Where-Object { $_.packagetype -eq 'bdist_wheel' } |
                Select-Object -First 1

            $PackagePath = Join-Path ([System.IO.Path]::GetTempPath()) (Split-Path $wheelEntry.url -Leaf)
            Invoke-RestMethod -Uri $wheelEntry.url -OutFile $PackagePath

        } elseif ($Platform -eq "dotnet") {
            $nugetUrl = "https://api.nuget.org/v3-flatcontainer/$PublishedPackageName/$PublishedPackageVersion/$PublishedPackageName.$PublishedPackageVersion.nupkg"
            $PackagePath = Join-Path "$([System.IO.Path]::GetTempPath())" "$PublishedPackageName.$PublishedPackageVersion.nupkg"

            Invoke-RestMethod -Uri $nugetUrl -OutFile $PackagePath

        } else {
            throw "Unsupported platform: $Platform"
        }
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