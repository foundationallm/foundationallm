param name string
param topicName string
param destinationTopicName string
param eventGridName string
param filterPrefix string = ''
param includedEventTypes array
param appResourceGroup string
param timestamp string = utcNow()

resource eventGridNamespace 'Microsoft.EventGrid/namespaces@2023-12-15-preview' existing = {
  name: eventGridName
  scope: resourceGroup(appResourceGroup)
}

resource destinationTopic 'Microsoft.EventGrid/namespaces/topics@2023-12-15-preview' existing = {
  name: destinationTopicName
  parent: eventGridNamespace
}

resource topic 'Microsoft.EventGrid/systemTopics@2023-12-15-preview' existing = {
  name: topicName
}

module eventSendRole 'utility/roleAssignments.bicep' = {
  name: 'stSendRole-${timestamp}'
  scope: resourceGroup(appResourceGroup)
  params: {
    principalId: topic.identity.principalId
    roleDefinitionIds: {
      'EventGrid Data Sender': 'd5a91429-5739-47e2-a06b-3470a27159e7'
    }
  }
}

resource resourceProviderSub 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2023-12-15-preview' = {
  name: name
  parent: topic
  properties: {
    deliveryWithResourceIdentity: {
      identity: {
        type: 'SystemAssigned'
      }
      destination: {
        endpointType: 'NamespaceTopic'
        properties: {
          resourceId: destinationTopic.id
        }
      }  
    }
    filter: {
      subjectBeginsWith: filterPrefix
      includedEventTypes: includedEventTypes
      enableAdvancedFilteringOnArrays: true
    }
    eventDeliverySchema: 'CloudEventSchemaV1_0'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
  dependsOn: [ eventSendRole ]
}
