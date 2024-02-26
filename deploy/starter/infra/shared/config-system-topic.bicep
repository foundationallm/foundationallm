param name string
param location string = resourceGroup().location
param tags object = {}
param appConfigAccountName string

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-08-01-preview' existing = {
  name: appConfigAccountName
}

resource topic 'Microsoft.EventGrid/systemTopics@2023-12-15-preview' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    source: appConfig.id
    topicType: 'Microsoft.AppConfiguration.ConfigurationStores'
  }
}

output name string = topic.name
