#! /usr/bin/pwsh

# Token replace (https://gist.github.com/eiximenis/55361a2f60722f123ec49febb1399004)

Param(
    [parameter(Mandatory = $false, ValueFromPipeline = $true)][string]$content = "",    
    [parameter(Mandatory = $false)][string]$inputFile = "",    
    [parameter(Mandatory = $false)][string]$outputFile = "",
    [parameter(Mandatory = $true)][hashtable]$tokens
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrEmpty($content)) {
    if ([string]::IsNullOrEmpty($inputFile)) {
        Write-Host "Must enter -inputFile if content is not piped" -ForegroundColor Red
        exit 1
    }
    $content = Get-Content -Raw $inputFile
}

Write-Host "Replacing tokens..." -ForegroundColor Yellow
$tokens.Keys | ForEach-Object {
    Write-Host "Replacing $_" -ForegroundColor Yellow
    $content = $content -replace "{{$_}}", $tokens[$_]
}

if ([string]::IsNullOrEmpty($outputFile)) {
    Write-Output $content
}
else {
    Set-Content -Path $outputFile -Value $content
}
