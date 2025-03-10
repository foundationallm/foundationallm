# Plugins

## Overview

## Managing plugins using the FoundationaLLM Management API

### List plugins

Management API endpoint: `GET /instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins`

Plugins can also be filtered by category name using the following Management API endpoint: `POST /instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/filter`. The request body must contain a JSON object with the following content:

```json
{
    "categories": [
        "Data Source",
        "Data Pipeline Stage"
    ]
}
```
>[!NOTE]
> The following plugin categories are supported for use in the filter: `Data Source`, `Data Pipeline Stage`, `Context Text Extraction`, `Content Text Partitioning`.