targetScope = 'subscription'

param createDate string = utcNow('u')
param environmentName string
param location string
param project string
param timestamp string = utcNow()

// Locals
var abbrs = loadJsonContent('./abbreviations.json')
var resourceGroup = namer(abbrs.resourcesResourceGroups, environmentName, location, 'test', project)

var tags = {
  'azd-env-name': environmentName 
  'create-date': createDate
  'iac-type': 'bicep'
  'project-name': project
}

var clientSecrets = [
  {
    name: 'bearer-token'
    value: 'PLACEHOLDER'
  }
]

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

// Resources
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
    name: resourceGroup
    location: location
    tags: tags
}

/** Nested Modules **/
module keyVault './modules/keyVault.bicep' = {
  name: 'keyvault-${timestamp}'
  params: {
    abbrs: abbrs
    environmentName:environmentName
    location: location
    project: project
    tags: tags
    secrets: clientSecrets
    principalId: loadtest.outputs.loadTestIdentity
  }
  scope: rg
}

module loadtest './modules/loadTest.bicep' = {
  name: 'loadtest-${timestamp}'
  params: {
    abbrs: abbrs
    environmentName:environmentName
    location: location
    project: project
    tags: tags
  }
  scope: rg
}

output FLLM_PROJECT string = project
output RESOURCE_GROUP_NAME string = rg.name


