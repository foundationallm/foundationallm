function Merge-VectorDatabase {
    param (
        [hashtable]$VectorDatabase
    )

    $VectorDatabase['api_endpoint_configuration_object_id'] = (Get-ObjectId `
        -Name $VectorDatabase['api_endpoint_configuration_object_id']['name'] `
        -Type $VectorDatabase['api_endpoint_configuration_object_id']['type'])

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Vector/vectorDatabases/$($VectorDatabase['name'])" `
        -Body $VectorDatabase
}