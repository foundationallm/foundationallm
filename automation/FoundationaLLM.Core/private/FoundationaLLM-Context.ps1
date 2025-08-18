function Merge-KnowledgeUnit {
    param (
        [hashtable]$KnowledgeUnit
    )

    $KnowledgeUnit['vector_database_object_id'] = (Get-ObjectId `
        -Name $KnowledgeUnit['vector_database_object_id']['name'] `
        -Type $KnowledgeUnit['vector_database_object_id']['type'])

    if ($KnowledgeUnit['knowledge_graph_vector_database_object_id']) {
        $KnowledgeUnit['knowledge_graph_vector_database_object_id'] = (Get-ObjectId `
            -Name $KnowledgeUnit['knowledge_graph_vector_database_object_id']['name'] `
            -Type $KnowledgeUnit['knowledge_graph_vector_database_object_id']['type'])
    }

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Context/knowledgeUnits/$($KnowledgeUnit['name'])" `
        -Body $KnowledgeUnit
}

function Merge-KnowledgeSource {
    param (
        [hashtable]$KnowledgeSource
    )

    $KnowledgeSource['knowledge_unit_object_ids'] = $KnowledgeSource['knowledge_unit_object_ids'] | ForEach-Object {
        Get-ObjectId -Name $_['name'] -Type $_['type']
    }

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Context/knowledgeSources/$($KnowledgeSource['name'])" `
        -Body $KnowledgeSource
}