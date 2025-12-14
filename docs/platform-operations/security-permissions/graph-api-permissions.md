# Why FoundationaLLM needs Graph API permissions

## Summary
A fully functional, robust, and scalable Role-Based Access Control (RBAC) implementation is critical for the success of any enterprise software platform. This is especially true with Generative AI platforms where the new ways in which data is transported, processed, and transformed must be secured and governed. 

A fully functional RBAC implementation relies on capabilities provided by the underlying identity management platform like: 
- Enumeration of users, groups, and security principals
- Mapping of object identifiers to their display names
- Identification of security groups membership.
  
The RBAC implementation provided by FoundationaLLM is designed from the ground up to provide the necessary security layers required by a Generative AI platform deployed in the enterprise. For transparency and auditing purposes, FoundationaLLM keeps all its source code out in the open (in a public GitHub repository) allowing all of the claims made about the behavior of the code to be validated and confirmed by any interested party.

## Context
Graph API permissions are required for the managed identities of the two surface APIs of the platform: 
- Core API 
- Management API

These APIs are involved in authenticating their clients (the FoundationaLLM portals and any other actor calling them).
They are also involved in enforcing and managing the underlying Role-Based Access Control model of the platform.

## Rationale behind the architectural decision
When authenticating against Graph API, any custom-built API has two choices: 
- Using an Entra ID application registration with a client secret 
- or using a managed identity

In both cases, the underlying service principal (the enterprise application associated with the application registration or the managed identity itself) will need the proper permissions to the Graph API to fulfill its tasks.
[According to Microsoft's official documentation](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview):
`"Managed identities provide an automatically managed identity in Microsoft Entra ID for applications to use when connecting to resources that support Microsoft Entra ID authentication. Applications can use managed identities to obtain Microsoft Entra ID tokens without having to manage any credentials."`

The recommended path forward is using managed identities as opposed to any approach that involves using secrets that need to be managed (plain or certificates).

This is the underlying justification for our architectural decision to assign permissions to the managed identities.

## Justification of the need
Any standard Role-Based Access Control capability requires the following core features (none of them being optional):
- Based on the object ID of a security principal, retrieve the security groups it belongs to (either directly or transitively).
- Based on a list of object IDs of security principals, retrieve their display names (to display meaningful information in the RBAC management experience).
- Select users, groups, or service principals that are targets for role assignments in the RBAC model.

> [!NOTE]
> "Entry-level" approach is to ask for group membership in access tokens. This approach has significant limitations as it is limited to a small number of groups which, if exceeded, will result in a Graph API reference added to the access token, forcing the code to call into Graph API. This limit is hit with almost certainty in enterprise scenarios, hance our decision to use the robust approach (directly calling Graph API) by default.

Based on all of the above, the justification for the requested roles is as follows:
- `Group.Read.All` - required to get the group membership for security principals authenticated against the APIs which is in turn used to evaluate their role membership and level of access to the capabilities of the platform. Also required to list security groups when FoundationaLLM RBAC roles are assigned to them.
- `User.Read.All` - required for group membership retrieval and to list users when FoundationaLLM RBAC roles are assigned to them.
- `Application.Read.All` - required for group membership retrieval and to list security principals when FoundationaLLM RBAC roles are assigned to them.

> [!NOTE]
> From the Graph API documentation (https://learn.microsoft.com/en-us/graph/permissions-reference):
"In some cases, an app might need extra permissions to read some group properties like member and memberOf. For example, if a group has one or more service principals as members, the app also needs permissions to read service principals, otherwise Microsoft Graph returns an error or limited information."

## Transparency and auditing
The FoundationaLLM source code is fully available in a public GitHub repo: https://github.com/foundationallm/foundationallm.
We took this critical decision precisely for cases like this, where our customers need to understand (and also fully audit if needed) the security implications of the permissions granted to various FoundationaLLM components during deployment.
For the specific case of Graph API interactions, the behavior is fully encapsulated in one [service implementation](https://github.com/foundationallm/foundationallm/blob/main/src/dotnet/Common/Services/Security/MicrosoftGraphIdentityManagementService.cs)
It can be immediately verified that:
- Our code performs only the operations mentioned above.
- Our code uses the smallest possible subset of properties to fulfill its required functionality.