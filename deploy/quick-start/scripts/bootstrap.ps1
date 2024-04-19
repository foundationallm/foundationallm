Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$AZCOPY_VERSION = "10.24.0"


$AZCOPY = @{
    "Windows" = @{
        uri = "https://aka.ms/downloadazcopy-v10-windows"
    }
    "Mac" = @{
        uri = "https://aka.ms/downloadazcopy-v10-mac"
    }
}

try {
    if ($IsWindows) {
        $url = $AZCOPY["Windows"].uri
        $os = "windows"
    } elseif ($IsMac) {
        $url = $AZCOPY["Mac"].uri
        $os = "mac"
    }
    $outputPath = "./tools/azcopy.zip"

    if (Test-Path -Path "./tools/azcopy_${os}_amd64_${AZCOPY_VERSION}") {
        Write-Host "azcopy_${os}_amd64_${AZCOPY_VERSION} already exists."
    } else {
        Invoke-WebRequest -Uri $url -OutFile $outputPath
        Expand-Archive -Path $outputPath -DestinationPath ./tools
    }

    Push-Location "./tools/azcopy_${os}_amd64_${AZCOPY_VERSION}"
    & ./azcopy.exe login
}
catch {
    Write-Error -Message "Unable to install azcopy"
}
finally {
    Pop-Location
}
