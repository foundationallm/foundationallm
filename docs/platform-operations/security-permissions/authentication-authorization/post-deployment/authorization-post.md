## Post Deployment
Follow the instruction below to complete the setup of Microsoft Entra ID authentication for the Authorization API application after the deployment is complete.

### Update App Configuration settings

1. Sign in to the [Azure portal](https://portal.azure.com/) as at least a Contributor.
2. Navigate to the resource group that was created as part of the deployment.
3. Select the **App Configuration** resource and select **Configuration explorer** to view the values.
4. Enter `authorization` in the search box to filter the results.
5. Check the box next to **Key** in the header to select all items.
6. Find the key for `FoundationaLLM:APIs:AuthorizationAPI:APIScope` and click on edit.
7. Replace the value with the value from the scope we created earlier, as `api://FoundationaLLM-Authorization` 
8. Select **Apply** to save the changes.

## Next steps

Now that Entra authorization is fully configured, navigate to your Entra ID management console and make sure you completed all app registrations for all the other apps mentioned in the deployment documentation.