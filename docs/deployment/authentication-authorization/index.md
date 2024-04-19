# Authentication

FoundationaLLM uses the [Microsoft Entra ID](https://learn.microsoft.com/entra/fundamentals/whatis) service to authenticate users and applications. Check back for additional authentication providers in the future.

## Microsoft Entra ID

> [!IMPORTANT]
> The following steps are required to set up authentication and authorization for the solution. You will need to create app registrations in the Entra ID tenant in the Azure portal.  There are currently **five** app registrations required for the solution as listed below.  After you complete the 5 app registrations, you will need to finish the deployment process of the solution and revisit these app registrations to fill in some missing values that are generated during the deployment itself.

- [Core API and user portal authentication setup - Microsoft Entra ID](core-authentication-setup-entra.md)
- [Management API and portal authentication setup - Microsoft Entra ID](management-authentication-setup-entra.md)
- [Authorization setup - Microsoft Entra ID](authorization-setup-entra.md)
