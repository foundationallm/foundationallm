param environmentName string
param resourceSuffix string
param project string

var name = 'ag-${resourceSuffix}'

output id string = main.id

/*
  Resource representing an Azure Monitor Action Group.
  This resource is used to define an action group that can be used for alert notifications.
*/
resource main 'microsoft.insights/actionGroups@2023-01-01' = {
  location: 'Global'
  name: name

  properties: {
    enabled: true
    groupShortName: 'do-nothing'
  }

  tags: {
    Environment: environmentName
    IaC: 'Bicep'
    Project: project
    Purpose: 'DevOps'
  }
}
