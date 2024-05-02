Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$AZCOPY_VERSION = "10.24.0"

try {
    if ($IsWindows) {
        $url = "https://aka.ms/downloadazcopy-v10-windows"
        $os = "windows"
        $ext = "zip"
    }
    elseif ($IsMacOS) {
        $url = "https://aka.ms/downloadazcopy-v10-mac"
        $os = "mac"
        $ext = "zip"
    }
    elseif ($IsLinux) {
        $url = "https://aka.ms/downloadazcopy-v10-linux"
        $os = "linux"
        $ext = "tar.gz"
    }

    $outputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./tools/azcopy.${ext}")
    $destinationPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./tools")
    $toolPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("./tools/azcopy_${os}_amd64_${AZCOPY_VERSION}")

    if (Test-Path -Path "./tools/azcopy_${os}_amd64_${AZCOPY_VERSION}") {
        Write-Host "azcopy_${os}_amd64_${AZCOPY_VERSION} already exists."
    }
    else {
        Invoke-WebRequest -Uri $url -OutFile $outputPath
        if ($IsLinux) {
            tar -xvzf $outputPath -C $destinationPath
        }
        else {
            Expand-Archive -Path $outputPath -DestinationPath $destinationPath
        }
    }

    Push-Location $toolPath
    if ($IsLinux) {
        chmod +x ./azcopy
        ./azcopy login
    }
    else {
        & ./azcopy.exe login
    }
}
catch {
    Write-Error -Message $_
}
finally {
    Pop-Location
}
