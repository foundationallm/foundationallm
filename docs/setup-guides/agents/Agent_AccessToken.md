# Agent Access Token

To allow the flexibility of using an agent without requiring the user to be authenticated using Entra ID credentials, you can create an **Agent Access Token**. This is particularly useful for public applications that want to provide access to the agent without requiring users to log in with their Entra ID credentials.

## How to create an Agent Access Token

1. In the **Security** section of the agent configuration, click on the **Create Access Token** button to create a new access token.
   
   ![Create Access Token](./media/agent_Workflow_6.png)

2. In the **Create Access Token** dialog, enter a description for the access token and an expiration date then click on the **Create Access Token** button.
   
   ![Create Access Token Dialog](./media/agent_Workflow_7.png)

3. The access token will be created and displayed in a dialog, make sure to save it or copy it for future use.
   
   ![Access Token Created](./media/agent_Workflow_8.png)

## Assigning permission to the new Virtual Security Group ID of the agent

1. Copy the GUID of the new Virtual Security Group ID of the agent.

   ![Copy the GUID of the new Virtual Security Group ID of the agent](./media/agent_Workflow_9.png)

2. At the top of the page while editing the agent, click on the **Access Control**.
   
   ![Click on the Access Control](./media/agent_Workflow_10.png)

3. In the **Access Control** page, click on the **Add Role Assignment for this resource** button.
4. Verify that the Scope is set to `providers/FoundationaLLM.Agent/agents/{agent_name}`.
5. Choose **Group** as the Prinicipal Type.
6. Paste the GUID of the new Virtual Security Group ID of the agent in the **Principal ID** field.
7. Choose **Reader** as the Role to assign.

    ![Add Role Assignment for this resource](./media/agent_Workflow_11.png)
