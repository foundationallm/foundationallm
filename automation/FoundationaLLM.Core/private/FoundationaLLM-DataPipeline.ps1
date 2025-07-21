function Resolve-PluginObjectIdRecursive {
    param (
        [hashtable]$Object,
        [string]$PropertyName = 'plugin_object_id',
        [string]$ResourceType = 'FoundationaLLM.Plugin/plugins'
    )
    foreach ($key in @($Object.Keys)) {
        if ($key -eq $PropertyName -and $Object[$key] -is [string]) {
            $Object[$key] = Get-ObjectId `
                -Name $Object[$key] `
                -Type $ResourceType
        } elseif ($Object[$key] -is [hashtable]) {
            Resolve-PluginObjectIdRecursive -Object $Object[$key] -PropertyName $PropertyName -ResourceType $ResourceType
        } elseif ($Object[$key] -is [System.Collections.IEnumerable] -and
                 -not ($Object[$key] -is [string])) {
            foreach ($item in $Object[$key]) {
                if ($item -is [hashtable]) {
                    Resolve-PluginObjectIdRecursive -Object $item -PropertyName $PropertyName -ResourceType $ResourceType
                }
            }
        }
    }
}

function Merge-DataPipeline {
    param (
        [hashtable]$DataPipeline
    )

    Resolve-PluginObjectIdRecursive -Object $DataPipeline -PropertyName 'plugin_object_id' -ResourceType 'FoundationaLLM.Plugin/plugins'
    Resolve-PluginObjectIdRecursive -Object $DataPipeline -PropertyName 'data_source_object_id' -ResourceType 'FoundationaLLM.DataSource/dataSources'

    Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.DataPipeline/dataPipelines/$($DataPipeline['name'])" `
        -Body $DataPipeline
}

function Start-DataPipeline {
    param (
        [string]$DataPipelineName,
        [hashtable]$TriggerParameters
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.DataPipeline/dataPipelines/$($DataPipelineName)/trigger" `
        -Body $TriggerParameters
}