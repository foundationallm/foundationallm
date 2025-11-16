import type {
	AgentBase,
	AgentCreationFromTemplateRequest,
	CompletionPrompt,
	CompletionRequest,
	ConversationProperties,
	CoreConfiguration,
	LongRunningOperation,
	Message,
	MessageRatingRequest,
	MessageResponse,
	MultipartPrompt,
	OneDriveWorkSchool,
	ResourceBase,
	ResourceName,
	ResourceNameCheckResult,
	ResourceProviderDeleteResults,
	ResourceProviderGetResult,
	ResourceProviderUpsertResult,
	Session,
	UserProfile,
	RoleDefinition,
	RoleAssignment,
	SecurityPrincipal,
	UserProfileUpdateRequest
} from '@/js/types';

export default {
	apiUrl: null as string | null,
	virtualUser: null as string | null,

	getVirtualUser() {
		return this.virtualUser;
	},

	/**
	 * Checks if the given email is valid.
	 * @param email - The email to validate.
	 * @returns True if the email is valid, false otherwise.
	 */
	isValidEmail(email: string): boolean {
		const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		return emailPattern.test(email);
	},

	setApiUrl(url: string) {
		// Set the api url and remove a trailing slash if there is one.
		this.apiUrl = url.replace(/\/$/, '');
	},

	getApiUrl() {
		return this.apiUrl;
	},

	instanceId: null as string | null,
	setInstanceId(instanceId: string) {
		this.instanceId = instanceId;
	},

	/**
	 * Retrieves the bearer token for authentication.
	 * If the bearer token is already available, it will be returned immediately.
	 * Otherwise, it will acquire a new bearer token using the MSAL instance.
	 * @returns The bearer token.
	 */
	async getBearerToken() {
		const token = await useNuxtApp().$authStore.getApiToken() as { accessToken: string };
		return token.accessToken;
	},

	/**
	 * Retrieves the value of a configuration key.
	 * @param key - The key of the configuration value to retrieve.
	 * @returns A promise that resolves to the configuration value.
	 */
	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key,
			},
		});
	},

	/**
	 * Retrieves the values of multiple configuration keys.
	 * @param filter - The filter used to identify the configuration values to retrieve.
	 * @returns A promise that resolves to the configuration values.
	 */
	async filterConfigValues(filter: string) {
		return await $fetch(`/api/config/`, {
			params: {
				filter,
			},
		});
	},

	/**
	 * Retrieves the UserPortal app configuration set from the management endpoint.
	 * @returns A promise that resolves to the UserPortal app configuration set.
	 */
	async getUserPortalAppConfigurationSet() {
		return await this.fetch<ResourceProviderGetResult<any>[]>(
			`/management/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurationSets/UserPortal`
		);
	},

	/**
	 * Fetches data from the specified URL using the provided options.
	 * @param url The URL to fetch data from.
	 * @param opts The options for the fetch request.
	 * @returns A promise that resolves to the fetched data.
	 */
	async fetch<T>(url: string, opts: any = {}): Promise<T> {
		const response = await this.fetchDirect(`${this.apiUrl}${url}`, opts);
		return response as T;
	},

	async fetchDirect<T>(url: string, opts: any = {}): Promise<T> {
		const options = opts;
		options.headers = opts.headers || {};

		const bearerToken = await this.getBearerToken();
		options.headers.Authorization = `Bearer ${bearerToken}`;

		// Add X-USER-IDENTITY header if virtualUser is set.
		if (!this.virtualUser) {
			const urlParams = new URLSearchParams(window.location.search);
			const virtualUser = urlParams.get('virtual_user');
			this.virtualUser = virtualUser;
		}
		if (this.virtualUser && this.isValidEmail(this.virtualUser)) {
			options.headers['X-USER-IDENTITY'] = JSON.stringify({
				name: this.virtualUser,
				user_name: this.virtualUser,
				upn: this.virtualUser,
				user_id: '00000000-0000-0000-0001-000000000001',
				group_ids: ['00000000-0000-0000-0000-000000000001'],
			});
		}

		try {
			const response = await $fetch(url, options) as T & { status?: number };
			if (response.status && response.status >= 400) {
				throw response;
			}
			return response as T;
		} catch (error: any) {
			// Preserve the original error structure
			if (error.data?.quota_exceeded) {
				throw error;
			}
			// For other errors, format them
			throw new Error(formatError(error));
		}
	},

	/**
	 * Starts a long-running process by making a POST request to the specified URL with the given request body.
	 * @param url - The URL to send the POST request to.
	 * @param requestBody - The request body to send with the POST request.
	 * @returns A Promise that resolves to the operation ID if the process is successfully started.
	 * @throws An error if the process fails to start.
	 */
	async startLongRunningProcess(requestBody: any): Promise<string> {
		try {
			const response = await this.fetch<{ status: number; operationId: string }>(
				`/instances/${this.instanceId}/async-completions`,
				{
					method: 'POST',
					body: requestBody,
				},
			);

			if (response.status === 202) {
				return response.operationId;
			} else {
				throw new Error('Failed to start process');
			}
		} catch (error) {
			throw new Error(formatError(error));
		}
	},

	/**
	 * Checks the status of a process operation.
	 * @param operationId - The ID of the operation to check.
	 * @returns A Promise that resolves to the response from the server.
	 * @throws If an error occurs during the API call.
	 */
	async checkProcessStatus(operationId: string): Promise<LongRunningOperation> {
		return await this.fetch<LongRunningOperation>(
			`/instances/${this.instanceId}/async-completions/${operationId}/status`,
		);
	},

	/**
	 * Polls for the completion of an operation.
	 * @param operationId - The ID of the operation to poll for completion.
	 * @returns A promise that resolves to the result of the operation when it is completed.
	 */
	async pollForCompletion(operationId: string): Promise<Message> {
		while (true) {
			const status = await this.checkProcessStatus(operationId);
			if (status.isCompleted) {
				return status.result as Message;
			}
			await new Promise((resolve) => setTimeout(resolve, 2000)); // Poll every 2 seconds
		}
	},

	/**
	 * Retrieves the chat sessions from the API.
	 * @returns {Promise<Array<Session>>} A promise that resolves to an array of sessions.
	 */
	async getSessions(): Promise<Session[]> {
		const sessions = await this.fetch<Session[]>(`/instances/${this.instanceId}/sessions`);

		// Check if sessions is actually an array
		if (!Array.isArray(sessions)) {
			return [];
		}

		return sessions;
	},

	/**
	 * Adds a new chat session.
	 * @returns {Promise<Session>} A promise that resolves to the created session.
	 */
	async addSession(properties: ConversationProperties): Promise<Session> {
		return await this.fetch<Session>(`/instances/${this.instanceId}/sessions`, {
			method: 'POST',
			body: properties,
		});
	},

	/**
	 * Renames a session.
	 * @param conversationId The identifier of the conversation to update.
	 * @param newConversationName The new name for the session.
	 * @returns The renamed session.
	 */
	async updateConversation(conversationId: string, newConversationName: string, newMetadata: string): Promise<Session> {
		const properties: ConversationProperties = { name: newConversationName, metadata: newMetadata };
		return await this.fetch<Session>(`/instances/${this.instanceId}/sessions/${conversationId}/update`, {
			method: 'POST',
			body: properties,
		});
	},

	/**
	 * Deletes a session by its ID.
	 * @param sessionId The ID of the session to delete.
	 * @returns A promise that resolves to the deleted session.
	 */
	async deleteSession(sessionId: string): Promise<Session> {
		return await this.fetch<Session>(`/instances/${this.instanceId}/sessions/${sessionId}`, {
			method: 'DELETE',
		});
	},

	// Mock polling route
	// async getMessage(messageId: string) {
	// 	return await $fetch(`/api/stream-message/`);
	// },

	/**
	 * Retrieves a specific prompt for a given session.
	 * @param sessionId The ID of the session.
	 * @param promptId The ID of the prompt.
	 * @returns The completion prompt.
	 */
	async getPrompt(sessionId: string, promptId: string): Promise<CompletionPrompt> {
		return await this.fetch<CompletionPrompt>(
			`/instances/${this.instanceId}/sessions/${sessionId}/completionprompts/${promptId}`,
		);
	},

	/**
	 * Retrieves messages for a given session.
	 * @param sessionId - The ID of the session.
	 * @returns An array of messages.
	 */
	async getMessages(sessionId: string): Promise<Message[]> {
		return await this.fetch<Message[]>(
			`/instances/${this.instanceId}/sessions/${sessionId}/messages`,
		);
	},

	/**
	 * Rates a message.
	 * @param message - The message to be rated.
	 * @param rating - The rating value for the message.
	 * @returns The rated message.
	 */
	async rateMessage(message: Message) {
		// Create a new instance of the MessageRatingRequest object
		// and set the rating and comments properties
		const messageRatingRequest: MessageRatingRequest = {
			rating: message.rating,
			comments: message.ratingComments,
		};

		return (await this.fetch(
			`/instances/${this.instanceId}/sessions/${message.sessionId}/message/${message.id}/rate`,
			{
				method: 'POST',
				body: messageRatingRequest,
			},
		)) as Message;
	},

	/**
	 * Sends a message to the API for a specific session.
	 * @param sessionId The ID of the session.
	 * @param text The text of the message.
	 * @param agent The agent object.
	 * @returns A promise that resolves to a MessageResponse or rejects with a RateLimitError.
	 */
	async sendMessage(sessionId: string, text: string, agent: AgentBase, attachments: string[] = [], metadata?: { [key: string]: any }): Promise<MessageResponse> {
		const orchestrationRequest: CompletionRequest = {
			session_id: sessionId,
			user_prompt: text,
			agent_name: agent.name,
			settings: undefined,
			attachments,
			metadata
		};

		return (await this.fetch(`/instances/${this.instanceId}/async-completions`, {
			method: 'POST',
			body: orchestrationRequest,
		})) as MessageResponse;
	},

	/**
	 * Retrieves the list of agents from the API.
	 * @returns {Promise<Agent[]>} A promise that resolves to an array of Agent objects.
	 */
	async getAllowedAgents() {
		const agents = (await this.fetch(
			`/instances/${this.instanceId}/completions/agents`,
		)) as ResourceProviderGetResult<AgentBase>[];

		// Check if agents is actually an array
		if (!Array.isArray(agents)) {
			return [];
		}

		agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));
		return agents;
	},

	async getAgents() {
		try {
			const agents = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents`
			) as ResourceProviderGetResult<AgentBase>[];

			// Check if agents is actually an array
			if (!Array.isArray(agents)) {
				return [];
			}

			agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));

			return agents;
		} catch (error) {
			console.error('Error fetching agents from management endpoint:', error);
			throw error;
		}
	},

	/**
	 * Retrieves a specific agent by name.
	 * @param agentName The name of the agent to retrieve.
	 * @returns A promise that resolves to the agent resource.
	 */
	async getAgent(agentName: string): Promise<ResourceProviderGetResult<AgentBase>> {
		try {
			const [agentGetResult]: ResourceProviderGetResult<AgentBase>[] = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}`
			) as ResourceProviderGetResult<AgentBase>[];

			return agentGetResult;
		} catch (error) {
			console.error(`Error fetching agent '${agentName}' from management endpoint:`, error);
			throw error;
		}
	},

	/**
	 * Gets the scope path for an agent.
	 * @param agentName The name of the agent.
	 * @returns The scope path for the agent.
	 */
	getAgentScope(agentName: string): string {
		return `/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}`;
	},

	/**
	 * Gets the scope identifier for an agent (used in role assignment filtering).
	 * @param agentName The name of the agent.
	 * @returns The scope identifier for the agent.
	 */
	getAgentScopeIdentifier(agentName: string): string {
		return `providers/FoundationaLLM.Agent/agents/${agentName}`;
	},

	/**
	 * Gets the resource type for role assignments.
	 * @returns The resource type string for role assignments.
	 */
	getRoleAssignmentType(): string {
		return "FoundationaLLM.Authorization/roleAssignments";
	},

	/**
	 * Gets the principal type for a security principal.
	 * @param principal The security principal object.
	 * @returns The principal type string.
	 */
	getPrincipalType(principal: any): string {
		// Default to User, but can be extended to detect other types
		return "User";
	},

	/**
	 * Uploads attachment to the API.
	 * @param file The file formData to upload.
	 * @returns The ObjectID of the uploaded attachment.
	 */
	async uploadAttachment(
		file: FormData,
		sessionId: string,
		agentName: string,
		progressCallback: Function,
	) {
		const bearerToken = await this.getBearerToken();

		const response: ResourceProviderUpsertResult = (await new Promise((resolve, reject) => {
			const xhr = new XMLHttpRequest();

			xhr.upload.onprogress = function (event) {
				if (progressCallback) {
					progressCallback(event);
				}
			};

			xhr.onload = () => {
				if (xhr.status >= 200 && xhr.status < 300) {
					resolve(JSON.parse(xhr.response));
				} else {
					reject(xhr.statusText);
				}
			};

			xhr.onerror = () => {
				// eslint-disable-next-line prefer-promise-reject-errors
				reject('Error during file upload.');
			};

			xhr.open(
				'POST',
				`${this.apiUrl}/instances/${this.instanceId}/files/upload?sessionId=${sessionId}&agentName=${agentName}`,
				true,
			);

			xhr.setRequestHeader('Authorization', `Bearer ${bearerToken}`);
			xhr.send(file);
		})) as ResourceProviderUpsertResult;

		return response;
	},

	/**
	 * Deletes attachments from the server.
	 * @param attachments - An array of attachment names to be deleted.
	 * @returns A promise that resolves to the delete results.
	 */
	async deleteAttachments(attachments: string[]) {
		return (await this.fetch(`/instances/${this.instanceId}/files/delete`, {
			method: 'POST',
			body: JSON.stringify(attachments),
		})) as ResourceProviderDeleteResults;
	},

	/**
	 * Retrieves the core configuration for the current instance.
	 *
	 * @returns {Promise<CoreConfiguration>} A promise that resolves to the core configuration.
	 */
	async getCoreConfiguration() {
		return (await this.fetch(`/instances/${this.instanceId}/configuration`)) as CoreConfiguration;
	},

	/**
	 * Connects to user's OneDrive work or school account.
	 * @returns A Promise that resolves to the response from the server.
	 */
	async oneDriveWorkSchoolConnect() {
		return await this.fetch(`/instances/${this.instanceId}/oneDriveWorkSchool/connect`, {
			method: 'POST',
			body: null,
		});
	},

	/**
	 * Disconnect to user's OneDrive work or school account.
	 * @returns A Promise that resolves to the response from the server.
	 */
	async oneDriveWorkSchoolDisconnect() {
		return await this.fetch(`/instances/${this.instanceId}/oneDriveWorkSchool/disconnect`, {
			method: 'POST',
			body: null,
		});
	},

	/**
	 * Retrieves user profile for a given instance.
	 * @returns The user profile.
	 */
	async getUserProfile() {
		return (await this.fetch(`/instances/${this.instanceId}/userProfiles/`)) as UserProfile;
	},

	/**
	 * Adds an agent to the user's profile selection.
	 * @param agentObjectId - The object ID of the agent to add.
	 * @returns A Promise that resolves to the response from the server.
	 */
	async addAgentToUserProfile(agentObjectId: string) {
		const payload: UserProfileUpdateRequest = {
			agent_object_id: agentObjectId
		};
		return await this.fetch(`/instances/${this.instanceId}/userprofiles/add-agent`, {
			method: 'POST',
			body: JSON.stringify(payload),
		});
	},

	/**
	 * Removes an agent from the user's profile selection.
	 * @param agentObjectId - The object ID of the agent to remove.
	 * @returns A Promise that resolves to the response from the server.
	 */
	async removeAgentFromUserProfile(agentObjectId: string) {
		const payload: UserProfileUpdateRequest = {
			agent_object_id: agentObjectId
		};
		return await this.fetch(`/instances/${this.instanceId}/userprofiles/remove-agent`, {
			method: 'POST',
			body: JSON.stringify(payload),
		});
	},

	/**
	 * Downloads a file from the user's connected OneDrive work or school account.
	 * @param sessionId - The session ID from which the file is uploaded.
	 * @param agentName - The agent name.
	 * @param oneDriveWorkSchool - The OneDrive work or school item.
	 * @returns A Promise that resolves to the response from the server.
	 */
	async oneDriveWorkSchoolDownload(
		sessionId: string,
		agentName: string,
		oneDriveWorkSchool: OneDriveWorkSchool,
	) {
		return (await this.fetch(
			`/instances/${this.instanceId}/oneDriveWorkSchool/download?instanceId=${this.instanceId}&sessionId=${sessionId}&agentName=${agentName}`,
			{
				method: 'POST',
				body: oneDriveWorkSchool,
			},
		)) as OneDriveWorkSchool;
	},
	/**
	 * Creates a new agent from the BasicAgentTemplate.
	 * @param templateParameters The parameters for the agent template.
	 * @returns A promise that resolves to the upsert result containing the new agent.
	 */
	async createAgentFromTemplate(templateParameters: AgentCreationFromTemplateRequest): Promise<ResourceProviderUpsertResult & { resource: AgentBase }> {
		const url = `/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agentTemplates/BasicAgentTemplate/create-new`;
		return await this.fetch<ResourceProviderUpsertResult & { resource: AgentBase }>(url, {
			method: 'POST',
			body: {
				template_parameters: templateParameters,
			},
		});
	},

	/**
	 * Retrieves private store files for a given agent from the management endpoint.
	 * Returns an array of ResourceProviderGetResult where each result.resource contains file details.
	 */
	async getAgentPrivateFiles(agentName: string): Promise<ResourceProviderGetResult<any>[]> {
		try {
			const files = await this.fetch<ResourceProviderGetResult<any>[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles`
			);
			return files;
		} catch (error) {
			console.error('Error fetching agent private files:', error);
			throw error;
		}
	},

		/**
	 * Uploads a file to an agent's private storage.
	 * @param agentName - The name of the agent.
	 * @param fileName - The name of the file to upload.
	 * @param file - The FormData containing the file.
	 * @returns A promise that resolves to the upload result.
	 */
	async uploadAgentFile(agentName: string, fileName: string, file: FormData): Promise<any> {
		try {
			const result = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles/${fileName}`,
				{
					method: 'POST',
					body: file,
				}
			);
			return result;
		} catch (error) {
			console.error('Error uploading agent file:', error);
			throw error;
		}
	},

	/**
	 * Deletes a file from an agent's private storage.
	 * @param agentName - The name of the agent.
	 * @param fileId - The unique identifier of the file to delete.
	 * @returns A promise that resolves to the delete result.
	 */
	async deleteAgentFile(agentName: string, fileId: string): Promise<any> {
		try {
			const result = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles/${fileId}`,
				{
					method: 'DELETE',
				}
			);
			return result;
		} catch (error) {
			console.error('Error deleting agent file:', error);
			throw error;
		}
	},

	/**
	 * Associates a file with the Knowledge tool for an agent.
	 * @param agentName - The name of the agent.
	 * @param fileId - The unique identifier of the file to associate.
	 * @returns A promise that resolves to the association result.
	 */
	async associateFileWithKnowledgeTool(agentName: string, fileId: string): Promise<any> {
		try {
			const payload = {
				agent_file_tool_associations: {
					[`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFiles/${fileId}`]: {
						[`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/tools/Code`]: false,
						[`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/tools/Knowledge`]: true
					}
				}
			};

			const result = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentFileToolAssociations/${fileId}`,
				{
					method: 'POST',
					body: JSON.stringify(payload),
					headers: {
						'Content-Type': 'application/json'
					}
				}
			);
			return result;
		} catch (error) {
			console.error('Error associating file with Knowledge tool:', error);
			throw error;
		}
	},

	/**
	 * Retrieves the list of AI models from the management endpoint.
	 * Returns an array of ResourceBase (see aiModel.ts) as required.
	 */
	async getAIModels(): Promise<ResourceBase[]> {
		try {
			const aiModels = await this.fetch<ResourceBase[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels`
			);
			return aiModels;
		} catch (error) {
			console.error('Error fetching AI models:', error);
			throw error;
		}
	},

	/**
	 * Checks if the derived agent resource name is available.
	 * @param name - The derived resource name to check.
	 * @returns Promise resolving to the check response.
	 */
	async checkAgentNameAvailability(name: string): Promise<ResourceNameCheckResult> {
		const payload: ResourceName = {
			type: 'generic-agent',
			name,
		};
		return await this.fetch<ResourceNameCheckResult>(
			`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/checkname`,
			{
				method: 'POST',
				body: payload,
			}
		);
	},

	/**
	 * Retrieves the main system prompt (prefix) for the agent's workflow.
	 * @param agent The agent object.
	 * @returns The main prompt string (prefix) or null if not found.
	 */
	async getAgentMainPrompt(agent: AgentBase): Promise<string | null> {
		if (!agent?.workflow) {
			return null;
		}
		// Find the main_prompt object_id from workflow.resource_object_ids
		const workflow = agent.workflow;
		if (!workflow?.resource_object_ids) return null;
		let mainPromptObjectId: string | null = null;
		for (const [objectId, obj] of Object.entries(workflow.resource_object_ids)) {
			if (obj?.properties?.object_role === 'main_prompt') {
				mainPromptObjectId = obj.object_id;
				break;
			}
		}
		if (!mainPromptObjectId) return null;
		// Extract the prompt name from the object_id (last segment)
		const promptId = mainPromptObjectId.split('/').pop();
		if (!promptId) return null;
		// Fetch the prompt resource
		const result = await this.fetch<ResourceProviderGetResult<MultipartPrompt>[]>(
			`/management/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${promptId}`
		);

		// Check if the result contains the expected prompt
		if (Array.isArray(result) && result.length > 0 && result[0].resource?.prefix) {
			return result[0].resource.prefix;
		}

		return null;
	},

	/**
	 * Updates the main prompt of an agent's workflow.
	 * @param agent The agent object (must have workflow).
	 * @param newPrefix The new prefix value for the main prompt.
	 * @returns The updated MultipartPrompt resource.
	 */
	async updateAgentMainPrompt(agent: AgentBase, newPrefix: string): Promise<MultipartPrompt | null> {

		if (!agent?.workflow) {
			throw new Error('Agent workflow is missing.');
		}

		const workflow = agent.workflow;
		if (!workflow?.resource_object_ids) throw new Error('Workflow resource_object_ids missing.');
		let mainPromptObjectId: string | null = null;
		for (const [objectId, obj] of Object.entries(workflow.resource_object_ids)) {
			if (obj?.properties?.object_role === 'main_prompt') {
				mainPromptObjectId = obj.object_id;
				break;
			}
		}
		if (!mainPromptObjectId) throw new Error('Main prompt object_id not found.');
		const promptId = mainPromptObjectId.split('/').pop();
		if (!promptId) throw new Error('Prompt ID could not be determined.');

		// Fetch the current MultipartPrompt resource
		const result = await this.fetch<ResourceProviderGetResult<MultipartPrompt>[]>(
			`/management/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${promptId}`
		);
		if (!Array.isArray(result) || result.length === 0 || !result[0].resource) {
			throw new Error('Prompt resource not found.');
		}
		const promptResource: MultipartPrompt = { ...result[0].resource, prefix: newPrefix };

		// Update the prompt by POSTing the full MultipartPrompt resource
		const updated = await this.fetch<MultipartPrompt>(
			`/management/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${promptId}`,
			{
				method: 'POST',
				body: promptResource,
			}
		);
		return updated;
	},

       /**
	* Updates the main model of an agent's workflow.
	* Replaces the main model object_id in both the key and value of resource_object_ids.
	* The payload must be the entire KnowledgeManagementAgent (AgentBase) object, as in CP-7.
	* @param agent The full agent model (AgentBase).
	* @param newModelObjectId The new model's object_id to set as main model.
	* @returns The updated agent.
	*/
       async updateAgentMainModel(agent: AgentBase, newModelObjectId: string): Promise<AgentBase> {
	       if (!agent?.workflow || !agent.workflow.resource_object_ids) {
		       throw new Error('Agent workflow or resource_object_ids missing.');
	       }

	       // Find the current main model object_id (by object_role)
	       let mainModelKey: string | null = null;
	       let mainModelObj: any = null;
	       for (const [key, obj] of Object.entries(agent.workflow.resource_object_ids)) {
		       if (obj?.properties?.object_role === 'main_model') {
			       mainModelKey = key;
			       mainModelObj = obj;
			       break;
		       }
	       }
	       if (!mainModelKey || !mainModelObj) {
		       throw new Error('Main model object_id not found in agent.workflow.resource_object_ids.');
	       }

	       // Remove the old main model entry
	       delete agent.workflow.resource_object_ids[mainModelKey];

	       // Add the new main model entry (key and value)
	       agent.workflow.resource_object_ids[newModelObjectId] = {
		       ...mainModelObj,
		       object_id: newModelObjectId,
	       };

	       // POST the full agent model to update (must match KnowledgeManagementAgent/AgentBase shape)
	       const url = `/management/${agent.object_id}`;
	       return await this.fetch<AgentBase>(url, {
		       method: 'POST',
		       body: agent,
	       });
		},

	/**
	 * Retrieves role definitions from the management endpoint.
	 * Returns an array of RoleDefinition resources.
	 */
	async getRoleDefinitions(): Promise<RoleDefinition[]> {
		try {
			const roleDefinitions = await this.fetch<RoleDefinition[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions`
			);
			return roleDefinitions;
		} catch (error) {
			console.error('Error fetching role definitions:', error);
			throw error;
		}
	},

	/**
	 * Retrieves role assignments for a specific scope (e.g., agent).
	 * @param scope - The scope to get role assignments for (e.g., 'providers/FoundationaLLM.Agent/agents/AgentName').
	 * @returns Promise resolving to an array of role assignments.
	 */
	async getRoleAssignments(scope: string): Promise<ResourceProviderGetResult<RoleAssignment>[]> {
		try {
			const assignments = await this.fetch<ResourceProviderGetResult<RoleAssignment>[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter`,
				{
					method: 'POST',
					body: {
						scope: `/instances/${this.instanceId}${scope ? `/${scope}` : ''}`,
					},
				}
			);

			// Add scope name for display purposes
			assignments.forEach((assignment) => {
				if (assignment.resource.scope === `/instances/${this.instanceId}`) {
					assignment.resource.scope_name = scope ? 'Instance (Inherited)' : 'Instance';
				} else if (assignment.resource.scope === `/instances/${this.instanceId}/${scope}`) {
					assignment.resource.scope_name = 'Resource';
				}
			});

			return assignments;
		} catch (error) {
			console.error('Error fetching role assignments:', error);
			throw error;
		}
	},

	/**
	 * Checks if the current user has both Agents and Prompts Contributor roles at the instance level.
	 * This method makes a single API call to fetch all role assignments and checks for both roles.
	 * @returns Promise resolving to an object with both role check results.
	 */
	async checkContributorRoles(): Promise<{ hasAgentsContributorRole: boolean; hasPromptsContributorRole: boolean }> {
		try {
			const assignments = await this.fetch<ResourceProviderGetResult<RoleAssignment>[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter`,
				{
					method: 'POST',
					body: {
						scope: `/instances/${this.instanceId}`,
						security_principal_ids: ["CURRENT_USER_IDS"]
					},
				}
			);

			// Role definition IDs - using the correct format with full path
			const agentsContributorRoleId = '/providers/FoundationaLLM.Authorization/roleDefinitions/3f28aa77-a854-4aa7-ae11-ffda238275c9';
			const promptsContributorRoleId = '/providers/FoundationaLLM.Authorization/roleDefinitions/479e7b36-5965-4a7f-baf7-84e57be854aa';

			// Check for both roles in a single pass
			let hasAgentsContributorRole = false;
			let hasPromptsContributorRole = false;

			for (const assignment of assignments) {
				const roleId = assignment.resource.role_definition_id;
				if (roleId === agentsContributorRoleId) {
					hasAgentsContributorRole = true;
				}
				if (roleId === promptsContributorRoleId) {
					hasPromptsContributorRole = true;
				}
				// Early exit if we found both roles
				if (hasAgentsContributorRole && hasPromptsContributorRole) {
					break;
				}
			}

			return {
				hasAgentsContributorRole,
				hasPromptsContributorRole
			};
		} catch (error) {
			console.error('Error checking contributor roles:', error);
			// Return false for both roles on error to be safe
			return {
				hasAgentsContributorRole: false,
				hasPromptsContributorRole: false
			};
		}
	},

	/**
	 * Checks if the current user has Agents Contributor role at the instance level.
	 * @deprecated Use checkContributorRoles() instead for better performance.
	 * @returns Promise resolving to boolean indicating if user has the role.
	 */
	async hasAgentsContributorRole(): Promise<boolean> {
		const result = await this.checkContributorRoles();
		return result.hasAgentsContributorRole;
	},

	/**
	 * Checks if the current user has Prompts Contributor role at the instance level.
	 * @deprecated Use checkContributorRoles() instead for better performance.
	 * @returns Promise resolving to boolean indicating if user has the role.
	 */
	async hasPromptsContributorRole(): Promise<boolean> {
		const result = await this.checkContributorRoles();
		return result.hasPromptsContributorRole;
	},

	/**
	 * Retrieves security principals (users/groups) by their IDs.
	 * @param ids - Array of principal IDs to retrieve.
	 * @returns Promise resolving to an array of security principals.
	 */
	async getSecurityPrincipals(ids: string[]): Promise<SecurityPrincipal[]> {
		try {
			const principals = await this.fetch<ResourceProviderGetResult<SecurityPrincipal>[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/securityPrincipals/filter`,
				{
					method: 'POST',
					body: { ids },
				}
			);
			// Extract the resource from each ResourceProviderGetResult wrapper
			return principals.map(wrapper => wrapper.resource);
		} catch (error) {
			console.error('Error fetching security principals:', error);
			throw error;
		}
	},

	/**
	 * Retrieves users with optional filtering.
	 * @param params - Optional parameters for filtering users.
	 * @returns Promise resolving to user results.
	 */
	async getUsers(params: { name?: string; ids?: string[]; page_number?: number; page_size?: number | null } = {}): Promise<{ items: SecurityPrincipal[] }> {
		try {
			const defaults = {
				name: '',
				ids: [],
				page_number: 1,
				page_size: null,
			};

			const users = await this.fetch<{ items: SecurityPrincipal[] }>(
				`/management/instances/${this.instanceId}/identity/users/retrieve`,
				{
					method: 'POST',
					body: {
						...defaults,
						...params,
					},
				}
			);
			return users;
		} catch (error) {
			console.error('Error fetching users:', error);
			throw error;
		}
	},

	/**
	 * Filters security principals by name for user search functionality.
	 * @param name - Partial name to search for.
	 * @returns Promise resolving to an array of security principals.
	 */
	async filterSecurityPrincipalsByName(name: string): Promise<SecurityPrincipal[]> {
		try {
			const principals = await this.fetch<ResourceProviderGetResult<SecurityPrincipal>[]>(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/securityPrincipals/filter`,
				{
					method: 'POST',
					body: {
						name: name,
						security_principal_type: 'User'
					},
				}
			);
			// Extract the resource from each ResourceProviderGetResult wrapper
			return principals.map(wrapper => wrapper.resource);
		} catch (error) {
			console.error('Error filtering security principals by name:', error);
			throw error;
		}
	},

	/**
	 * Creates a new role assignment.
	 * @param roleAssignment - The role assignment to create.
	 * @returns Promise resolving to the creation result.
	 */
	async createRoleAssignment(roleAssignment: any): Promise<any> {
		try {
			// Use the correct endpoint format as specified in requirements
			// HTTP POST {{core_base_url}}/management/instances/{{instanceId}}/providers/FoundationaLLM.Authorization/roleAssignments/<role_assignment_id>
			const result = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${roleAssignment.name}`,
				{
					method: 'POST',
					body: roleAssignment,
					headers: {
						'Content-Type': 'application/json'
					}
				}
			);
			return result;
		} catch (error) {
			console.error('Error creating role assignment:', error);
			throw error;
		}
	},

	/**
	 * Sets the primary owner for an agent.
	 * @param instanceId - The FoundationaLLM instance identifier.
	 * @param agentName - The name of the agent.
	 * @param payload - The AgentOwnerUpdateRequest payload.
	 * @returns Promise resolving to the ResourceProviderActionResult.
	 */
	async setAgentPrimaryOwner(instanceId: string, agentName: string, payload: { owner_user_id: string }): Promise<any> {
		try {
			// HTTP POST /management/instances/{{instanceId}}/providers/FoundationaLLM.Agent/agents/{{agentName}}/set-owner
			const result = await this.fetch(
				`/management/instances/${instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/set-owner`,
				{
					method: 'POST',
					body: payload,
					headers: {
						'Content-Type': 'application/json'
					}
				}
			);
			return result;
		} catch (error) {
			console.error('Error setting agent primary owner:', error);
			throw error;
		}
	},
};

function formatError(error: any): string {
	if (error.errors || error.data?.errors) {
		const errors = error.errors || error.data.errors;
		// Flatten the error messages and join them into a single string
		return Object.values(errors).flat().join(' ');
	}
	if (error.data) {
		return error.data.message || error.data.title || error.data || 'An unknown error occurred';
	}
	if (error.message) {
		return error.message;
	}
	if (typeof error === 'string') {
		return error;
	}
	return 'An unknown error occurred';
}
