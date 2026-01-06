# Agents

Foundationa**LLM** (FLLM) agents are the core of the solution. They are responsible for providing users with a customized experience based on its configuration.

## Agent Configuration

- [Agents and Workflows](agents_workflows.md) - Configure agent workflows and tools
- [Knowledge Management Agents](knowledge-management-agent.md) - RAG-enabled agents

## Prompts

Prompts are an important aspect for agents. A prompt defines the persona, instructions and guardrails provided to the large language model (LLM) so that it formulates accurate responses in desired formats.

- [Prompt resources](prompt-resource.md)

## Agent Capabilities

- [Image Description](image-description.md) - LLM-generated image descriptions for multimodal agents
- [Private Storage](private-storage.md) - Private knowledge sources for custom agent owners

## Access Tokens

Access tokens are used to authenticate and authorize access to agents without the need for Entra ID credentials. This is useful for scenarios where you want to provide access to an agent without requiring users to log in with their Entra ID credentials.

- [Agent Access Token](Agent_AccessToken.md)

## Self-Service Agents

End users can create and manage their own agents:

- [Self-Service Agent Creation](../user-portal/self-service-agent-creation.md) - Create custom agents in the User Portal
- [Agent Sharing Model](../user-portal/agent-sharing-model.md) - Share agents with owners, collaborators, and users
