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
module loadtest './core/load-test/loadTest.bicep' = {
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


