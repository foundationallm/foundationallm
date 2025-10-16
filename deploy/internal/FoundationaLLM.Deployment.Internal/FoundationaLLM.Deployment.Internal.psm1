Get-ChildItem -Path "$PSScriptRoot\private" -Filter *.ps1 -Recurse | ForEach-Object {
    . $_.FullName
}