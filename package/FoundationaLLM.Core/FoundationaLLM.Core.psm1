Get-ChildItem -Path "$PSScriptRoot\private" -Filter *.ps1 | ForEach-Object {
    . $_.FullName
}