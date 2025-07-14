function Merge-DataSource {
    param (
        [hashtable]$DataSource
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.DataSource/dataSources/$($DataSource['name'])" `
        -Body $DataSource
}