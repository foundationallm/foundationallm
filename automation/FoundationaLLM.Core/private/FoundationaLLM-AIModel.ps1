

function Get-AllAIModels {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.AIModel/aiModels"
}

function Get-AIModel {
    param (
        [string]$AIModelName
    )

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.AIModel/aiModels/$AIModelName"
}

function Merge-AIModel {
    param (
        [hashtable]$AIModel
    )

    $AIModel['endpoint_object_id'] = (Get-ObjectId `
        -Name $AIModel['endpoint_object_id']['name'] `
        -Type $AIModel['endpoint_object_id']['type'])

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.AIModel/aiModels/$($AIModel.name)" `
        -Body $AIModel
}
