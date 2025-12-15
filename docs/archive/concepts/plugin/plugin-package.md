# Plugin Packages

## Managing plugin packages using the FoundationaLLM Management API

The management of plugin packages is done using the `FoundationaLLM.Plugin` resource provider through the Management API.
At this time, only .NET NuGet plugin packages are supported by the resource provider.

### Create or update a plugin package

Management API endpoint: `POST /instances/{instanceId}/providers/FoundationaLLM.Plugin/pluginPackages/{packageName}`

Request body:
- Must be of type `form-data`.
- Must contain a file with the key `file` that represents the plugin package. The file must be a .NET NuGet package.
- Must contain a text with the key `resource` that represents the plugin package resource. The resource must be a valid JSON object with the following content:

    ```json
    {
        "type": "plugin-package",
        "name": "{packageName}"
    }
    ```

>[!NOTE]
> FoundationaLLM currently provides one .NET plugin package, `FoundationaLLM.DataPipelinePlugins`. An example of a file for this package is `FoundationaLLM.DataPipelinePlugins.1.0.0.nupkg`.

>[!IMPORTANT]
> Plugin package names must follow a strict naming convention. The name must be in the format `{platform}-{name}`, where `{platform}` can be one of `Dotnet` or `Python`, and `{name}` can only contain alphanumerical characters, underlines, or hyphens. For example, the package name for the FoundationaLLM data pipeline plugins package is `Dotnet-FoundationaLLMDataPipelinePlugins`.

As part of the create or update flow, the package is inspected and all plugins contained within it are registered in the system.

### List plugin packages

Management API endpoint: `GET /instances/{instanceId}/providers/FoundationaLLM.Plugin/pluginPackages`
