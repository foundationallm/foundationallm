# Create a prompt in the Management Portal

This guide explains how to use the Management Portal to author a reusable prompt for FoundationaLLM agents. Follow these steps when you want to define the instructions that agents will use while conversing with users.

## Prerequisites

- You can access your organization's FoundationaLLM Management Portal.
- Your account is assigned a role that allows you to create or edit prompts (for example, **FoundationaLLM Owner** or **FoundationaLLM Contributor**).
- You know the name, description, and prefix text that you want to apply to the prompt.

## Step-by-step instructions

1. **Sign in to the Management Portal.**
   - Open the portal URL for your deployment and authenticate with an account that has permissions to manage prompts.
2. **Open the Prompts workspace.**
   - From the left navigation pane, select **Prompts** to review existing definitions.
3. **Start creating a prompt.**
   - Select **+ Create Prompt** to open the prompt creation form.
4. **Provide a prompt name.**
   - In the **Prompt name** field, enter a unique identifier for the prompt. Only letters, numbers, dashes, and underscores are allowed; spaces and special characters are blocked. The form validates the name and will show an error if the name is already in use.
5. **Describe the prompt.**
   - In the **Description** box, add a short explanation that helps others understand when this prompt should be used.
6. **Choose a category.**
   - Use the **Category** dropdown to select the prompt type. The built-in options are **Agent Workflow**, **Agent Tool**, and **Data Pipeline**. Pick the option that best matches the intended usage so that other users can filter prompts efficiently.
7. **Define the prompt prefix.**
   - In the **Prompt Prefix** text area, enter the instructions you want the agent to follow before any user-specific context is applied. This text usually sets the agent persona, tone, or guardrails. The field supports multi-line content and expands automatically as you type.
8. **Create the prompt.**
   - Review the entered information and select **Create Prompt**. If any required fields are missing, the portal displays validation messages so you can correct the inputs.
9. **Confirm the result.**
   - After the prompt is saved successfully, a confirmation message appears and you are returned to the Prompts list where the new entry is visible.

## Next steps

- Attach the prompt to agents or workflows so that conversations benefit from the new instructions.
- Document naming conventions or prefix templates your team should follow to keep prompts consistent.
