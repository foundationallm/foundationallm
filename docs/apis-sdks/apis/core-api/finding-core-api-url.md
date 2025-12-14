# Finding Your Core API URL

This guide explains how to locate the Core API URL for your FoundationaLLM deployment.

## Azure Container Apps (ACA) Deployment

If you performed an **Azure Container Apps (ACA)** deployment, follow these steps to obtain the URL for the Core API:

1. Within the Resource Group that was created as part of the deployment, select the **Container App** resource whose name ends with `coreca`.

    ![The Core API container app is selected in the deployed resource group.](../../../setup-guides/media/resource-group-core-aca.png)

2. Within the Overview pane, copy the **Application Url** value. This is the URL for the Core API.

    ![The container app's Application Url is highlighted.](../../../setup-guides/media/aca-core-application-url.png)

## Azure Kubernetes Service (AKS) Deployment

If you performed an **Azure Kubernetes Service (AKS)** deployment, follow these steps to obtain the URL for the Core API:

1. Within the Resource Group that was created as part of the deployment, select the **Kubernetes Service** resource.

    ![The Kubernetes service is selected in the deployed resource group.](../../../setup-guides/media/resource-group-aks.png)

2. Select **Properties** in the left-hand menu and copy the **HTTP application routing domain** value.

    ![The HTTP application routing domain property is highlighted.](../../../setup-guides/media/aks-http-app-routing-domain.png)

    Your Core API URL (for your AKS deployment) is the URL you just copied with `/core` appended to the end of it. For example, if your domain is `https://1cf699fd0d89446eabf2.eastus.aksapp.io/`, then your Core API URL is `https://1cf699fd0d89446eabf2.eastus.aksapp.io/core`.

## Related Topics

- [Core API Overview](index.md)
- [Directly Calling Core API](directly-calling-core-api.md)
- [Local API Access for Standard Deployments](standard-deployment-local-api-access.md)
