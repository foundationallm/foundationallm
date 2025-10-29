function Update-TemplateContent {
    param (
        [string]$TemplateContent,
        [hashtable]$Placeholders
    )

    $result = $TemplateContent
    foreach ($key in $Placeholders.Keys) {
        $placeholder = "{{$key}}"
        $value = $Placeholders[$key]
        $result = $result -replace [regex]::Escape($placeholder), $value
    }
    return $result
}