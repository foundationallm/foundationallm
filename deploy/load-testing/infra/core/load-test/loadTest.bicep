param abbrs object
param environmentName string
param location string
param project string
param tags object

// Locals
var loadTestName = namer(abbrs.loadTestServices, environmentName, location, 'test', project)

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

resource loadTestingResource 'Microsoft.LoadTestService/loadTests@2023-12-01-preview' = {
  name: loadTestName
  location: location
  
  identity: {
    type: 'SystemAssigned'
  }
  
  properties: {
    description: 'Azure Load Testing resource created via Bicep'
  }
  tags: tags
}

output loadTestResourceId string = loadTestingResource.id
