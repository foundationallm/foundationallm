#! /usr/bin/pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Get-AbsolutePath {
	<#
    .SYNOPSIS
    Get the absolute path of a file or directory. Relative path does not need to exist.
    #>
	param (
		[Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
		[string]$RelatviePath
	)

	return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($RelatviePath)
}
