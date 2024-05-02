import type { Message, Session, CompletionPrompt, Agent, OrchestrationRequest } from '@/js/types';

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
		const options = opts;
		options.headers = opts.headers || {};

		// if (options?.query) {
		// 	url += '?' + (new URLSearchParams(options.query)).toString();
		// }

		const bearerToken = await this.getBearerToken();
		options.headers.Authorization = `Bearer ${bearerToken}`;

		return await $fetch(`${this.apiUrl}${url}`, options);
	},

	/**
	 * Retrieves the chat sessions from the API.
	 * @returns {Promise<Array<Session>>} A promise that resolves to an array of sessions.
	 */
	async getSessions() {
		return (await this.fetch(`/sessions`)) as Array<Session>;
	},

	/**
	 * Adds a new chat session.
	 * @returns {Promise<Session>} A promise that resolves to the created session.
	 */
	async addSession() {
		return (await this.fetch(`/sessions`, { method: 'POST' })) as Session;
	},

	/**
	 * Renames a session.
	 * @param sessionId The ID of the session to rename.
	 * @param newChatSessionName The new name for the session.
	 * @returns The renamed session.
	 */
	async renameSession(sessionId: string, newChatSessionName: string) {
		return (await this.fetch(`/sessions/${sessionId}/rename`, {
			method: 'POST',
			params: {
				newChatSessionName,
			},
		})) as Session;
	},

	/**
	 * Summarizes the session name.
	 *
	 * @param sessionId - The ID of the session.
	 * @param text - The text to be summarized.
	 * @returns The summarized text.
	 */
	async summarizeSessionName(sessionId: string, text: string) {
		return (await this.fetch(`/sessions/${sessionId}/summarize-name`, {
			method: 'POST',
			body: JSON.stringify(text),
		})) as { text: string };
	},

	/**
	 * Deletes a session by its ID.
	 * @param sessionId The ID of the session to delete.
	 * @returns A promise that resolves to the deleted session.
	 */
	async deleteSession(sessionId: string) {
		return (await this.fetch(`/sessions/${sessionId}`, { method: 'DELETE' })) as Session;
	},

	/**
	 * Retrieves messages for a given session.
	 * @param sessionId - The ID of the session.
	 * @returns An array of messages.
	 */
	async getMessages(sessionId: string) {
		return (await this.fetch(`/sessions/${sessionId}/messages`)) as Array<Message>;
	},

	/**
	 * Retrieves a specific prompt for a given session.
	 * @param sessionId The ID of the session.
	 * @param promptId The ID of the prompt.
	 * @returns The completion prompt.
	 */
	async getPrompt(sessionId: string, promptId: string) {
		return (await this.fetch(
			`/sessions/${sessionId}/completionprompts/${promptId}`,
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

		return (await this.fetch(`/sessions/${message.sessionId}/message/${message.id}/rate`, {
			method: 'POST',
			params,
		})) as Message;
	},

	/**
	 * Sends a message to the API for a specific session.
	 * @param sessionId The ID of the session.
	 * @param text The text of the message.
	 * @param agent The agent object.
	 * @returns A promise that resolves to a string representing the server response.
	 */
	async sendMessage(sessionId: string, text: string, agent: Agent) {
		const orchestrationRequest: OrchestrationRequest = {
			session_id: sessionId,
			user_prompt: text,
			agent_name: agent.name,
			settings: null,
		};
		return (await this.fetch(`/sessions/${sessionId}/completion`, {
			method: 'POST',
			body: orchestrationRequest,
		})) as string;
	},

	/**
	 * Retrieves the list of agents from the API.
	 * @returns {Promise<Agent[]>} A promise that resolves to an array of Agent objects.
	 */
	async getAllowedAgents() {
		return (await this.fetch('/orchestration/agents')) as Agent[];
	},
};
