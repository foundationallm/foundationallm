## Which resources in FoundationaLLM are soft-deleted?
In FoundationaLLM, the following resources are soft-deleted:
- Azure OpenAI Resources
- Azure Key Vault Resources
- Azure AI Search Resources
- Azure Content Safety Resources

> [!NOTE]
> If you do not use the `azd down --purge` command when you delete your resources, you will need to do so manually in the portal to purge (delete permenantly) these resources. Otherwise, you will have to make sure to name the resources differently when you redeploy the FoundationaLLM platform. Another concern is that you may exceed the capacity or tokens allowed for your subscription if you do not purge the resources before creating another one. So, be aware of these issues when you deploy and delete resources from your subscription. 