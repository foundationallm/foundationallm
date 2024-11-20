param deploymentName string = ''
param tags object = {}

resource deploymentTag 'Microsoft.Resources/tags@2024-03-01' = {
  name: 'default'
  properties: {
    tags: union(tags, { 'deployment': deploymentName })
  }
}
