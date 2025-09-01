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
	UserProfile
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
		} catch (error) {
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
		return await this.fetch<Session[]>(`/instances/${this.instanceId}/sessions`);
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
		agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));
		return agents;
	},

	async getAgents() {
		try {
			const agents = await this.fetch(
				`/management/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents`
			) as ResourceProviderGetResult<AgentBase>[];

			agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));

			return agents;
		} catch (error) {
			console.error('Error fetching agents from management endpoint:', error);
			throw error;
		}
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
	 * Retrieves user profiles for a given instance.
	 * @returns An array of user profiles.
	 */
	async getUserProfile() {
		return (await this.fetch(`/instances/${this.instanceId}/userProfiles/`)) as Array<UserProfile>;
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
			type: 'knowledge-management',
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
			if (obj?.properties?.role === 'main_prompt') {
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
			if (obj?.properties?.role === 'main_prompt') {
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
