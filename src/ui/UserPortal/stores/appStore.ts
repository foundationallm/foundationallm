import { defineStore } from 'pinia';
import { useAppConfigStore } from './appConfigStore';
import { useAuthStore } from './authStore';
import type { Session, ChatSessionProperties, Message, Agent, ResourceProviderGetResult, Attachment } from '@/js/types';
import api from '@/js/api';
import eventBus from '@/js/eventBus';

export const useAppStore = defineStore('app', {
	state: () => ({
		sessions: [] as Session[],
		currentSession: null as Session | null,
		currentMessages: [] as Message[],
		isSidebarClosed: false as boolean,
		agents: [] as ResourceProviderGetResult<Agent>[],
		selectedAgents: new Map(),
		lastSelectedAgent: null as ResourceProviderGetResult<Agent> | null,
		attachments: [] as Attachment[],
		longRunningOperations: new Map<string, string>(), // sessionId -> operationId
	}),

	getters: {},

	actions: {
		async init(sessionId: string) {
			const appConfigStore = useAppConfigStore();

			// No need to load sessions if in kiosk mode, simply create a new one and skip.
			if (appConfigStore.isKioskMode) {
				const newSession = await api.addSession(this.getDefaultChatSessionProperties());
				await this.changeSession(newSession);
				return;
			}

			await this.getSessions();

			if (this.sessions.length === 0) {
				await this.addSession(this.getDefaultChatSessionProperties());
				await this.changeSession(this.sessions[0]);
			} else {
				const existingSession = this.sessions.find((session: Session) => session.id === sessionId);
				await this.changeSession(existingSession || this.sessions[0]);
			}

			if (this.currentSession) {
				await this.getMessages();
				this.updateSessionAgentFromMessages(this.currentSession);
			}
		},

		getDefaultChatSessionProperties(): ChatSessionProperties {
			const now = new Date();
			// Using the 'sv-SE' locale since it uses the 'YYY-MM-DD' format.
			const formattedNow = now.toLocaleString('sv-SE', {
			year: 'numeric',
			month: '2-digit',
			day: '2-digit',
			hour: '2-digit',
			minute: '2-digit',
			second: '2-digit',
			hour12: false,
			}).replace(' ', 'T').replace('T', ' ');
			return {
				name: formattedNow,
			};
		},

		async getSessions(session?: Session) {
			const sessions = await api.getSessions();

			if (session) {
				// If the passed in session is already in the list, replace it.
				// This is because the passed in session has been updated, most likely renamed.
				// Since there is a slight delay in the backend updating the session name, this
				// ensures the session name is updated in the sidebar immediately.
				const index = sessions.findIndex((s) => s.id === session.id);
				if (index !== -1) {
					sessions.splice(index, 1, session);
				}
				this.sessions = sessions;
			} else {
				this.sessions = sessions;
			}
		},

		async addSession(properties: ChatSessionProperties) {
			if (!properties) {
				properties = this.getDefaultChatSessionProperties();
			}

			const newSession = await api.addSession(properties);
			await this.getSessions(newSession);

			// Only add newSession to the list if it doesn't already exist.
			// We optionally add it because the backend is sometimes slow to update the session list.
			if (!this.sessions.find((session: Session) => session.id === newSession.id)) {
				this.sessions = [newSession, ...this.sessions];
			}

			return newSession;
		},

		async renameSession(sessionToRename: Session, newSessionName: string) {
			const existingSession = this.sessions.find(
				(session: Session) => session.id === sessionToRename.id,
			);

			// Preemptively rename the session for responsiveness, and revert the name if the request fails.
			const previousName = existingSession.name;
			existingSession.name = newSessionName;

			try {
				await api.renameSession(sessionToRename.id, newSessionName);
			} catch (error) {
				existingSession.name = previousName;
			}
		},

		async deleteSession(sessionToDelete: Session) {
			await api.deleteSession(sessionToDelete!.id);
			await this.getSessions();

			this.removeSession(sessionToDelete!.id);

			// Ensure there is at least always 1 session
			if (this.sessions.length === 0) {
				const newSession = await this.addSession(this.getDefaultChatSessionProperties());
				this.removeSession(sessionToDelete!.id);
				await this.changeSession(newSession);
			}

			const firstSession = this.sessions[0];
			if (firstSession) {
				await this.changeSession(firstSession);
			}
		},

		removeSession(sessionId: string) {
			this.sessions = this.sessions.filter(
				(session: Session) => session.id !== sessionId
			);
		},

		async getMessages() {
			const data = await api.getMessages(this.currentSession.id);
			this.currentMessages = data;
		},

		updateSessionAgentFromMessages(session: Session) {
			const lastAssistantMessage = this.currentMessages
				.filter((message) => message.sender.toLowerCase() === 'assistant')
				.pop();
			if (lastAssistantMessage) {
				const agent = this.agents.find(
					(agent) => agent.resource.name === lastAssistantMessage.senderDisplayName,
				);
				if (agent) {
					this.setSessionAgent(session, agent);
				}
			}
		},

		getSessionAgent(session: Session) {
			if (!session) return null;
			let selectedAgent = this.selectedAgents.get(session.id);
			if (!selectedAgent) {
				if (this.lastSelectedAgent) {
					// Default to the last selected agent to make the selection "sticky" across sessions.
					selectedAgent = this.lastSelectedAgent;
				} else {
					// Default to the first agent in the list.
					selectedAgent = this.agents[0];
				}
			}
			return selectedAgent;
		},

		setSessionAgent(session: Session, agent: ResourceProviderGetResult<Agent>) {
			this.lastSelectedAgent = agent;
			return this.selectedAgents.set(session.id, agent);
		},

		/**
		 * Sends a message to the Core API.
		 *
		 * @param text - The text of the message to send.
		 * @returns A Promise that resolves when the message is sent.
		 */
		async sendMessage(text: string) {
			if (!text) return;

			const sessionId = this.currentSession!.id;
			const relevantAttachments = this.attachments.filter(
				(attachment) => attachment.sessionId === sessionId,
			);

			const authStore = useAuthStore();
			const tempUserMessage: Message = {
				completionPromptId: null,
				id: '',
				rating: null,
				sender: 'User',
				senderDisplayName: authStore.currentAccount?.name ?? 'You',
				sessionId: this.currentSession!.id,
				text,
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'Message',
				vector: [],
			};
			this.currentMessages.push(tempUserMessage);

			const tempAssistantMessage: Message = {
				completionPromptId: null,
				id: '',
				rating: null,
				sender: 'Assistant',
				senderDisplayName: 'Assistant',
				sessionId: this.currentSession!.id,
				text: '',
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'LoadingMessage',
				vector: [],
			};
			this.currentMessages.push(tempAssistantMessage);

			const agent = this.getSessionAgent(this.currentSession!).resource;
			if (agent.long_running) {
				// Handle long-running operations
				const operationId = await api.startLongRunningProcess('/completions', {
					session_id: this.currentSession!.id,
					user_prompt: text,
					agent_name: agent.name,
					settings: null,
					attachments: relevantAttachments.map((attachment) => String(attachment.id)),
				});

				this.longRunningOperations.set(this.currentSession!.id, operationId);
				this.pollForCompletion(this.currentSession!.id, operationId);
			} else {
				await api.sendMessage(
					this.currentSession!.id,
					text,
					agent,
					relevantAttachments.map((attachment) => String(attachment.id)),
				);
				await this.getMessages();
				// Get rid of the attachments that were just sent.
				this.attachments = this.attachments.filter((attachment) => { return !relevantAttachments.includes(attachment) });
			}
		},

		/**
		 * Polls for the completion of a long-running operation.
		 *
		 * @param sessionId - The session ID associated with the operation.
		 * @param operationId - The ID of the operation to check for completion.
		 */
		async pollForCompletion(sessionId: string, operationId: string) {
			while (true) {
				const status = await api.checkProcessStatus(operationId);
				if (status.isCompleted) {
					this.longRunningOperations.delete(sessionId);
					eventBus.emit('operation-completed', { sessionId, operationId });
					await this.getMessages();
					break;
				}
				await new Promise((resolve) => setTimeout(resolve, 2000)); // Poll every 2 seconds
			}
		},

		async rateMessage(messageToRate: Message, isLiked: Message['rating']) {
			const existingMessage = this.currentMessages.find(
				(message) => message.id === messageToRate.id,
			);

			// Preemptively rate the message for responsiveness, and revert the rating if the request fails.
			const previousRating = existingMessage.rating;
			existingMessage.rating = isLiked;

			try {
				await api.rateMessage(messageToRate, isLiked);
			} catch (error) {
				existingMessage.rating = previousRating;
			}
		},

		async changeSession(newSession: Session) {
			const nuxtApp = useNuxtApp();
			const appConfigStore = useAppConfigStore();

			if (appConfigStore.isKioskMode) {
				nuxtApp.$router.push({ query: {} });
			} else {
				const query = { chat: newSession.id };
				nuxtApp.$router.push({ query });
			}

			this.currentSession = newSession;
			await this.getMessages();
			this.updateSessionAgentFromMessages(newSession);
		},

		toggleSidebar() {
			this.isSidebarClosed = !this.isSidebarClosed;
		},

		async getAgents() {
			this.agents = await api.getAllowedAgents();
			return this.agents;
		},

		async uploadAttachment(file: FormData, sessionId: string) {
			const id = await api.uploadAttachment(file);
			const fileName = file.get('file')?.name;
			const newAttachment = { id, fileName, sessionId };

			this.attachments.push(newAttachment);

			return id;
		},
	},
});
