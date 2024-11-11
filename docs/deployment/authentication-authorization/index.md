# Authentication

FoundationaLLM uses the [Microsoft Entra ID](https://learn.microsoft.com/entra/fundamentals/whatis) service to authenticate users and applications. Check back for additional authentication providers in the future.

## Microsoft Entra ID

Starting `release 0.8.0` there are 2 options of completing the app registrations for the 6 apps required for FoundationaLLM. Option #1 is to run a script that will register all 6 applications for you. Option #2 is to manually register the 6 applications. The steps for both options are below.

## Option #1 - Run the script to register all 6 applications
The script is available in the `\deploy\common\scripts\` folder. The script is called `Create-FllmEntraIdApps.ps1`. The script will register the 6 required applications in the Entra ID tenant that you are logged into.

![Entra ID app registration script](../media/EntraIDAppsCreation.png)

After the completion of the script execution, you will see the 6 applications registered in the Entra ID tenant under `App registrations`

![Entra ID registered apps in Portal](../media/EntraIDRegisteredAppsPortal.png)

## Option 2 - Manually registering the 6 applications

> [!IMPORTANT]
> The following steps are to set up authentication and authorization for the solution. You will need to create app registrations in the Entra ID tenant in the Azure portal manually if you choose not to run the automatic script for any reason.  There are currently **five** app registrations required for the solution as listed below.  After you complete the 6 app registrations, you will need to finish the deployment process of the solution and revisit these app registrations to fill in some missing values that are generated during the deployment itself.

### Steps to perform before the deployment
- [Core API and user portal authentication pre-deployment - Microsoft Entra ID](core-authentication-setup-entra.md)
- [Management API and portal authentication pre-deployment - Microsoft Entra ID](management-authentication-setup-entra.md)
- [Authorization pre-deployment - Microsoft Entra ID](authorization-setup-entra.md)

### Steps to perform after the deployment
- [Pre-requisites for post-deployment configuration](pre-requisites.md)
- [Core API and user portal authentication post-deployment - Microsoft Entra ID](post-core-deployment.md)
- [Management API and portal authentication post-deployment - Microsoft Entra ID](post-management-deployment.md)
- [Authorization post-deployment - Microsoft Entra ID](post-authorization-deployment.md)

