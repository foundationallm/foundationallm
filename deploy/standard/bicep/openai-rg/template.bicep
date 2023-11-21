param environment string
param instanceCount int = 2
param location string
param project string
param timestamp string = utcNow()

param capacity object = {
  completions: 1
  embeddings: 1
}

module openai './modules/openai.bicep' = [for x in range(0, instanceCount): {
  name: 'ha-openai-openai-${timestamp}-${x}'
  params: {
    capacity: capacity
    environment: environment
    location: location
    project: project
    instanceId: x
  }
}]
