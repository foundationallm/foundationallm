# Deployment Manifest Setup

The Deployment Manifest is a JSON file that defines the configuration for a Foundationa**LLM** Standard deployment.  You will find an empty Deployment Manifest template in `foundationallm/deploy/standard/Deployment-Manifest.template.json`.  Once you have filled in the required values, you can use the Deployment Manifest to deploy the solution to your Azure subscription using the provided deployment scripts.

## Create the Deployment Manifest

To create the Deployment Manifest, first copy the template file to a new file, for example `Deployment-Manifest.json`.  Then, fill in the required values for your deployment.  The following sections describe the different parts of the Deployment Manifest and the values you need to provide.

> [!NOTE]
> You may create several deployment manifests for different environments, such as development, testing, and production.  Each manifest should have the appropriate values for the environment in which you are deploying the solution.  `Deployment-Manifest.json` is the default name expected by scripts.  You can name your manifest files as you see fit and pass the file name as an argument to the deployment scripts.

## Property Values

## General

The root section of the Deployment Manifest defines the general properties of the deployment.   The following table describes each property and provides an example value.

| Name               | Description                                                          | Value                | Example                                |
| ------------------ | -------------------------------------------------------------------- | -------------------- | -------------------------------------- |
| `adminObjectId`    | The Azure AD Group's Object ID designated as the deployment's admin. | Entra Group ID       | `995a549b-067e-4fe3-9f90-98d78b9ed086` |
| `baseDomain`       | The base domain for the deployment.                                  | Internet Domain Name | `example.com`                          |
| `createVpnGateway` | Whether to create a VPN Gateway for the deployment.                  | Boolean              | `true`                                 |
| `environment`      | A token for naming deployment resources in the environment.          | String               | `dev`, `test`, `prod`                  |
| `instanceId`       | The unique ID for the deployment instance.                           | GUID                 | `5d40d2ee-aeb5-4391-95a0-1fd9045d7720` |
| `k8sNamespace`     | The Kubernetes namespace for the FLLM Helm deployments.              | String               | `fllm`                                 |
| `letsEncryptEmail` | The email address for Let's Encrypt notifications.                   | Email Address        | `admin@example.com`                    |
| `location`         | The Azure region where the deployment resources will be created.     | Azure Region         | `eastus2`, `francecentral`             |
| `networkName`      | The name of the network pre-provisioned before the deployment.       | String               | `fllm-network`                         |
| `project`          | A token for naming deployment resources in the environment.          | String               | `ai`, `fllm`, `rd`, `fred`, `sally`    |
| `subscription`     | The Azure subscription ID for the deployment.                        | GUID                 | `ad82622e-458a-4a48-8023-6b18eed1cf79` |

### Notes

- `createVpnGateway` is a boolean value that determines whether a VPN Gateway should be created as part of the deployment.  Set this value to `true` if you want to create a VPN Gateway.  You do not need to create a VPN gateway if your networking environment already has a VPN gateway that you want to use or a similar solution like Express Route.
- `instanceId` is a GUID that uniquely identifies the deployment instance.  You can generate a GUID using PowerShell or other tools.  Each deployment instance should have a unique `instanceId`, this value is used by the authorization system when determining access to resources.  This is similar to the subscription ID in Azure.
- `letsEncryptEmail` is the email address that will be used for Let's Encrypt notifications.  Let's Encrypt is used to generate SSL certificates for the deployment.  You do not need to provide this value unless you plan to use the optional pre-deployment script to generate certificates.  If you already have certificates, or you plan to use a different certificate provider, you can leave this value blank.  The deployment instructions will cover how to provide certificates during deployment.
- `location` is the Azure region where the deployment resources will be created.  You should choose a region that supports OpenAI and the models needed by Foundationa**LLM**.  The standard deployment supports automatically deploying the following models, not all models are available in every region, the template will configure the models supported in the specified location.  Consult the [Azure documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#model-summary-table-and-region-availability) and choose a region supporting the models you would like to use:
  - gpt-35-turbo (0613)
  - gpt-35-turbo (1106)
  - gpt-4 (1106-Preview)
  - gpt-4o (2024-05-13)
  - text-embedding-ada-002 (2)
  - text-embedding-3-large
  - text-embedding-3-small
- `networkName` is the name of the network pre-provisioned before the deployment.  The deployment will create the requird subnets and other networking resources in this network.  If you do not have a pre-provisioned network, the template will create one for you.  The network should be created in the networking resource group described later in the manifest.

## Entra Client IDs

The `entraClientIds` section of the Deployment Manifest defines the client IDs for the different parts of the Foundationa**LLM** system.  These client IDs are used by the authentication system to determine access to resources.  The client IDs are unique to each deployment and should be kept secure. See the [Authentication setup document](../authentication-authorization/index.md) for more information on the authentication system.

| Name               | Description                              | Value | See Also                                                                                                                                         |
| ------------------ | ---------------------------------------- | ----- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| `authorization`    | The client ID for the authorization API. | GUID  | [API Application Setup](../authentication-authorization/authorization-setup-entra.md#create-the-client-application)                              |
| `chat`             | The client ID for the chat service.      | GUID  | [Client Application Setup](../authentication-authorization/core-authentication-setup-entra.md#create-the-client-application)                     |
| `core`             | The client ID for the core API.          | GUID  | [API Application Setup](../authentication-authorization/core-authentication-setup-entra.md#create-the-api-application)                           |
| `managementapi`    | The client ID for the management API.    | GUID  | [Management API Application Setup](../authentication-authorization/management-authentication-setup-entra.md#create-the-api-application)          |
| `managementui`     | The client ID for the management UI.     | GUID  | [Management UI Application Setup](../authentication-authorization/management-authentication-setup-entra.md#create-the-client-application)        |
| `vectorizationapi` | The client ID for the vectorization API. | GUID  | [Vectorization API Application Setup](../authentication-authorization/vectorization-authentication-setup-entra.md#create-the-client-application) |

## Entra Client Secrets

The `entraClientSecrets` section of the Deployment Manifest provides the secrets use for authorization.

| Name            | Description                                                                 | Value                                               |
| --------------- | --------------------------------------------------------------------------- | --------------------------------------------------- |
| `authorization` | The client secret (password) for the authorization Application Registration | A client secret value generated in the Entra portal |

## Entra Instances

The `entraInstances` section of the Deployment Manifest defines the cloud that can be used for authorization.  In most cases this value will be `https://login.microsoftonline.com/`.

| Name            | Description                       | Value                                |
| --------------- | --------------------------------- | ------------------------------------ |
| `authorization` | The login URL for the Entra cloud | `https://login.microsoftonline.com/` |

## Entra Scopes

The `entraScopes` section of the Deployment Manifest defines the scopes for the different parts of the Foundationa**LLM** system.  These scopes are used by the authentication system to determine access to resources. See the [Authentication setup document](../authentication-authorization/index.md) for more information on the authentication system.

| Name               | Description                         | Example                                            | See Also                                                                                                                                                              |
| ------------------ | ----------------------------------- | -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `authorization`    | The scope for the authorization API | `api://FoundationaLLM-Authorization-Auth`          | [API Application Setup](../authentication-authorization/authorization-setup-entra.md#expose-an-api-for-the-api-application)                                           |
| `chat`             | The scope for the chat service      | `api://FoundationaLLM-Auth/Data.Read`              | [Client Application Setup](../authentication-authorization/core-authentication-setup-entra.md#register-the-client-application-in-the-microsoft-entra-id-admin-center) |
| `core`             | The scope for the core API          | `Data.Read`                                        | [API Application Setup](../authentication-authorization/core-authentication-setup-entra.md#register-the-api-application-in-the-microsoft-entra-id-admin-center)       |
| `managementapi`    | The scope for the management API    | `Data.Manage`                                      | [Management API Application Setup](../authentication-authorization/management-authentication-setup-entra.md#create-the-api-application)                               |
| `managementui`     | The scope for the management UI     | `api://FoundationaLLM-Management-Auth/Data.Manage` | [Management UI Application Setup](../authentication-authorization/management-authentication-setup-entra.md#create-the-client-application)                             |
| `vectorizationapi` | The scope for the vectorization API | `Data.Manage`                                      | [Vectorization API Application Setup](../authentication-authorization/vectorization-authentication-setup-entra.md#create-the-client-application)                      |

## Ingress Configuration

The `ingress` section of the Deployment Manifest defines the configuration for the Ingress resources that route traffic to the different parts of the Foundationa**LLM** system.  The Ingress resources are used to route traffic from the internet to the different services in the deployment.  The following table describes the properties of the `ingress` section.

 | Section           | Service            | Name          | Description                                                       | Example                              |
 | ----------------- | ------------------ | ------------- | ----------------------------------------------------------------- | ------------------------------------ |
 | `apiIngress`      |                    |               | Ingress Configuration for the APIs in the backend AKS cluster     |                                      |
 |                   | `coreapi`          |               | The Ingress configuration for the Core API                        |                                      |
 |                   |                    | `host`        | The host name for the Ingress resource                            | `api.fllm.example.com`               |
 |                   |                    | `path`        | The path for the Ingress resource                                 | `/core/`                             |
 |                   |                    | `pathType`    | The path type for the Ingress resource                            | `ImplementationSpecific`             |
 |                   |                    | `serviceName` | The name of the service that the Ingress routes to                | `core-api`                           |
 |                   |                    | `sslCert`     | The SSL certificate to use for the Ingress resource               | `coreapi`                            |
 |                   | `managementapi`    |               | The Ingress configuration for the Management API                  |                                      |
 |                   |                    | `host`        | The host name for the Ingress resource                            | `management-api.fllm.example.com`    |
 |                   |                    | `path`        | The path for the Ingress resource                                 | `/management/`                       |
 |                   |                    | `pathType`    | The path type for the Ingress resource                            | `ImplementationSpecific`             |
 |                   |                    | `serviceName` | The name of the service that the Ingress routes to                | `management-api`                     |
 |                   |                    | `sslCert`     | The SSL certificate to use for the Ingress resource               | `managementapi`                      |
 |                   | `vectorizationapi` |               | The Ingress configuration for the Vectorization API               |                                      |
 |                   |                    | `host`        | The host name for the Ingress resource                            | `vectorization-api.fllm.example.com` |
 |                   |                    | `path`        | The path for the Ingress resource                                 | `/vectorization/`                    |
 |                   |                    | `pathType`    | The path type for the Ingress resource                            | `ImplementationSpecific`             |
 |                   |                    | `serviceName` | The name of the service that the Ingress routes to                | `vectorization-api`                  |
 |                   |                    | `sslCert`     | The SSL certificate to use for the Ingress resource               | `vectorizationapi`                   |
 | `frontendIngress` |                    |               | Ingress Configuration for the portals in the frontend AKS cluster |                                      |
 |                   | `chatui`           |               | The Ingress configuration for the Chat UI                         |                                      |
 |                   |                    | `host`        | The host name for the Ingress resource                            | `chat.fllm.example.com`              |
 |                   |                    | `path`        | The path for the Ingress resource                                 | `/`                                  |
 |                   |                    | `pathType`    | The path type for the Ingress resource                            | `ImplementationSpecific`             |
 |                   |                    | `serviceName` | The name of the service that the Ingress routes to                | `chat-ui`                            |
 |                   |                    | `sslCert`     | The SSL certificate to use for the Ingress resource               | `chatui`                             |
 |                   | `managementui`     |               | The Ingress configuration for the Management UI                   |                                      |
 |                   |                    | `host`        | The host name for the Ingress resource                            | `management.fllm.example.com`        |
 |                   |                    | `path`        | The path for the Ingress resource                                 | `/`                                  |
 |                   |                    | `pathType`    | The path type for the Ingress resource                            | `ImplementationSpecific`             |
 |                   |                    | `serviceName` | The name of the service that the Ingress routes to                | `management-ui`                      |
 |                   |                    | `sslCert`     | The SSL certificate to use for the Ingress resource               | `managementui`                       |

## Resource Group Configuration

The `resourceGroups` section of the Deployment Manifest defines the names of the resource groups that will be created as part of the deployment.  The following table describes the properties of the `resourceGroups` section.

| Name      | Description                                                     | Example                     |
| --------- | --------------------------------------------------------------- | --------------------------- |
| `app`     | The resource group for the application hosting resources (AKS). | `rg-ai-dev-eastus2-app`     |
| `auth`    | The resource group for the authorization API storage resources. | `rg-ai-dev-eastus2-auth`    |
| `data`    | The resource group for the customer source data resources.      | `rg-ai-dev-eastus2-data`    |
| `dns`     | The resource group for the Private DNS resources.               | `rg-ai-dev-eastus2-dns`     |
| `jbx`     | The resource group for the Jumpbox resources.                   | `rg-ai-dev-eastus2-jbx`     |
| `net`     | The resource group for the networking resources.                | `rg-ai-dev-eastus2-net`     |
| `oai`     | The resource group for the OpenAI resources.                    | `rg-ai-dev-eastus2-oai`     |
| `ops`     | The resource group for the operations resources.                | `rg-ai-dev-eastus2-ops`     |
| `storage` | The resource group for the FLLM internal storage resources.     | `rg-ai-dev-eastus2-storage` |
| `vec`     | The resource group for the vectorization resources.             | `rg-ai-dev-eastus2-vec`     |

## External Resource Group Configuration

The `externalResourceGroups` section of the Deployment Manifest defines the names of the resource groups that contain resources that are external to the deployment.  When pre-provisioning resources for FLLM, be sure to remove the corresponding entry from the [`resourceGroups`](./manifest.md#resource-group-configuration) section.  The following table describes the properties of the `externalResourceGroups` section.

| Name  | Description                                                          | Example                 |
| ----- | -------------------------------------------------------------------- | ----------------------- |
| `dns` | The resource group containing pre-provisioned Private DNS resources. | `rg-ai-shared-eastus2-dns` |


## Next

Return to the [standard deployment steps](../deployment-standard.md#deployment-steps).