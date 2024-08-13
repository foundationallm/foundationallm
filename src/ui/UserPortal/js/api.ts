import type {
	Message,
	Session,
	ChatSessionProperties,
	CompletionPrompt,
	Agent,
	CompletionRequest,
	ResourceProviderGetResult,
	ResourceProviderUpsertResult,
	ResourceProviderDeleteResult,
	ResourceProviderDeleteResults
} from '@/js/types';

export default {
	apiUrl: null as string | null,

	setApiUrl(url: string) {
		// Set the api url and remove a trailing slash if there is one.
		this.apiUrl = url.replace(/\/$/, '');
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
	bearerToken: null as string | null,
	async getBearerToken() {
		if (this.bearerToken) return this.bearerToken;

		const token = await useNuxtApp().$authStore.getToken();
		this.bearerToken = token.accessToken;
		return this.bearerToken;
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
	async fetch(url: string, opts: any = {}) {
		const response = await this.fetchDirect(`${this.apiUrl}${url}`, opts);
		return response;
	},

	async fetchDirect(url: string, opts: any = {}) {
		const options = opts;
		options.headers = opts.headers || {};

		const bearerToken = await this.getBearerToken();
		options.headers.Authorization = `Bearer ${bearerToken}`;

		try {
			const response = await $fetch(url, options);
			if (response.status >= 400) {
				throw response;
			}
			return response;
		} catch (error) {
			// If the error is an HTTP error, extract the message directly.
			const errorMessage = formatError(error);
			throw new Error(errorMessage);
		}
	},

	/**
	 * Starts a long-running process by making a POST request to the specified URL with the given request body.
	 * @param url - The URL to send the POST request to.
	 * @param requestBody - The request body to send with the POST request.
	 * @returns A Promise that resolves to the operation ID if the process is successfully started.
	 * @throws An error if the process fails to start.
	 */
	async startLongRunningProcess(url: string, requestBody: any) {
		const options = {
			method: 'POST',
			body: JSON.stringify(requestBody),
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${await this.getBearerToken()}`,
			},
		};

		try {
			const response = await $fetch(`${this.apiUrl}${url}`, options);
			if (response.status === 202) {
				return response.operationId;
			} else {
				throw new Error('Failed to start process');
			}
		} catch (error) {
			throw new Error(this.formatError(error));
		}
	},

	/**
	 * Checks the status of a process operation.
	 * @param operationId - The ID of the operation to check.
	 * @returns A Promise that resolves to the response from the server.
	 * @throws If an error occurs during the API call.
	 */
	async checkProcessStatus(operationId: string) {
		try {
			const response = await $fetch(
				`/instances/${this.instanceId}/async-completions/${operationId}/status`,
				{
					method: 'GET',
					headers: {
						Authorization: `Bearer ${await this.getBearerToken()}`,
					},
				},
			);

			return response;
		} catch (error) {
			throw new Error(this.formatError(error));
		}
	},

	/**
	 * Polls for the completion of an operation.
	 * @param operationId - The ID of the operation to poll for completion.
	 * @returns A promise that resolves to the result of the operation when it is completed.
	 */
	async pollForCompletion(operationId: string) {
		while (true) {
			const status = await this.checkProcessStatus(operationId);
			if (status.isCompleted) {
				return status.result;
			}
			await new Promise((resolve) => setTimeout(resolve, 2000)); // Poll every 2 seconds
		}
	},

	/**
	 * Retrieves the chat sessions from the API.
	 * @returns {Promise<Array<Session>>} A promise that resolves to an array of sessions.
	 */
	async getSessions() {
		return (await this.fetch(`/instances/${this.instanceId}/sessions`)) as Array<Session>;
	},

	/**
	 * Adds a new chat session.
	 * @returns {Promise<Session>} A promise that resolves to the created session.
	 */
	async addSession(properties: ChatSessionProperties) {
		return (await this.fetch(`/instances/${this.instanceId}/sessions`, {
			method: 'POST',
			body: properties,
		})) as Session;
	},

	/**
	 * Renames a session.
	 * @param sessionId The ID of the session to rename.
	 * @param newChatSessionName The new name for the session.
	 * @returns The renamed session.
	 */
	async renameSession(sessionId: string, newChatSessionName: string) {
		const properties: ChatSessionProperties = { name: newChatSessionName };
		return (await this.fetch(`/instances/${this.instanceId}/sessions/${sessionId}/rename`, {
			method: 'POST',
			body: properties,
		})) as Session;
	},

	/**
	 * Deletes a session by its ID.
	 * @param sessionId The ID of the session to delete.
	 * @returns A promise that resolves to the deleted session.
	 */
	async deleteSession(sessionId: string) {
		return (await this.fetch(`/instances/${this.instanceId}/sessions/${sessionId}`, {
			method: 'DELETE',
		})) as Session;
	},

	/**
	 * Retrieves messages for a given session.
	 * @param sessionId - The ID of the session.
	 * @returns An array of messages.
	 */
	async getMessages(sessionId: string) {
		return (await this.fetch(
			`/instances/${this.instanceId}/sessions/${sessionId}/messages`,
		)) as Array<Message>;
	},

	/**
	 * Retrieves a specific prompt for a given session.
	 * @param sessionId The ID of the session.
	 * @param promptId The ID of the prompt.
	 * @returns The completion prompt.
	 */
	async getPrompt(sessionId: string, promptId: string) {
		return (await this.fetch(
			`/instances/${this.instanceId}/sessions/${sessionId}/completionprompts/${promptId}`,
		)) as CompletionPrompt;
	},

	/**
	 * Rates a message.
	 * @param message - The message to be rated.
	 * @param rating - The rating value for the message.
	 * @returns The rated message.
	 */
	async rateMessage(message: Message, rating: Message['rating']) {
		const params: {
			rating?: Message['rating'];
		} = {};
		if (rating !== null) params.rating = rating;

		return (await this.fetch(
			`/instances/${this.instanceId}/sessions/${message.sessionId}/message/${message.id}/rate`,
			{
				method: 'POST',
				params,
			},
		)) as Message;
	},

	/**
	 * Sends a message to the API for a specific session.
	 * @param sessionId The ID of the session.
	 * @param text The text of the message.
	 * @param agent The agent object.
	 * @returns A promise that resolves to a string representing the server response.
	 */
	async sendMessage(sessionId: string, text: string, agent: Agent, attachments: string[] = []) {
		const orchestrationRequest: CompletionRequest = {
			session_id: sessionId,
			user_prompt: text,
			agent_name: agent.name,
			settings: null,
			attachments,
		};

		if (agent.long_running) {
			const operationId = await this.startLongRunningProcess(
				`/instances/${this.instanceId}/async-completions`,
				orchestrationRequest,
			);
			return this.pollForCompletion(operationId);
		} else {
			return (await this.fetch(`/instances/${this.instanceId}/completions`, {
				method: 'POST',
				body: orchestrationRequest,
			})) as string;
		}
	},

	/**
	 * Retrieves the list of agents from the API.
	 * @returns {Promise<Agent[]>} A promise that resolves to an array of Agent objects.
	 */
	async getAllowedAgents() {
		const agents = (await this.fetch(
			`/instances/${this.instanceId}/completions/agents`,
		)) as ResourceProviderGetResult<Agent>[];
		agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));
		return agents;
	},

	/**
	 * Uploads attachment to the API.
	 * @param file The file formData to upload.
	 * @returns The ObjectID of the uploaded attachment.
	 */
	async uploadAttachment(file: FormData, agentName: string, progressCallback: Function) {
		const response: ResourceProviderUpsertResult = await new Promise(async (resolve, reject) => {
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

			xhr.onerror = (error) => {
				reject('Error during file upload.');
			};

			xhr.open('POST', `${this.apiUrl}/instances/${this.instanceId}/files/upload?agentName=${agentName}`, true);

			const bearerToken = await this.getBearerToken();
			xhr.setRequestHeader("Authorization", `Bearer ${bearerToken}`);

			xhr.send(file);
		}) as ResourceProviderUpsertResult;

		return response;
	},

	/**
	 * Deletes attachments from the server.
	 * @param attachments - An array of attachment names to be deleted.
	 * @returns A promise that resolves to the delete results.
	 */
	async deleteAttachments(attachments: string[]) {
		return await this.fetch(`/instances/${this.instanceId}/files/delete`, {
			method: 'POST',
			body: JSON.stringify(attachments),
		}) as ResourceProviderDeleteResults;
	}
};

function formatError(error: any): string {
	if (error.errors || error.data?.errors) {
		const errors = error.errors || error.data.errors;
		// Flatten the error messages and join them into a single string
		return Object.values(errors).flat().join(' ');
	}
	if (error.data) {
		return error.data.message || error.data || 'An unknown error occurred';
	}
	if (error.message) {
		return error.message;
	}
	if (typeof error === 'string') {
		return error;
	}
	return 'An unknown error occurred';
}
