param name string
param location string = resourceGroup().location
param tags object = {}
param storageAccountName string

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource topic 'Microsoft.EventGrid/systemTopics@2023-12-15-preview' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    source: storage.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

output name string = topic.name
