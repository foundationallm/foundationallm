Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber
Import-Module "./package/common/FoundationaLLM-Agent.psm1" -Force -NoClobber
Import-Module "./package/common/FoundationaLLM-Prompt.psm1" -Force -NoClobber

Export-ModuleMember -Variable InstanceId, CoreAPIBaseUrl, ManagementAPIBaseUrl

Export-ModuleMember -Function Get-ObjectId
Export-ModuleMember -Function Get-ResourceObjectIds

Export-ModuleMember -Function Get-AllAgents
Export-ModuleMember -Function Merge-Agent

Export-ModuleMember -Function Get-AllPrompts
Export-ModuleMember -Function Merge-Prompt