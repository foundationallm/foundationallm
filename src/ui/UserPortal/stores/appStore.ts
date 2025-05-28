import { defineStore } from 'pinia';
import type { ToastMessageOptions } from 'primevue/toast';
import { useAppConfigStore } from './appConfigStore';
import { useAuthStore } from './authStore';
import type {
	Session,
	ChatSessionProperties,
	Message,
	UserProfile,
	Agent,
	CoreConfiguration,
	OneDriveWorkSchool,
	ResourceProviderGetResult,
	ResourceProviderUpsertResult,
	ResourceProviderDeleteResults,
	Attachment,
	MessageContent,
	RateLimitError,
} from '@/js/types';
import api from '@/js/api';
import { isAgentExpired } from '@/js/helpers';
// import eventBus from '@/js/eventBus';

const DEFAULT_POLLING_INTERVAL_MS = 5000;

export const useAppStore = defineStore('app', {
	state: () => ({
		sessions: [] as Session[],
		currentSession: null as Session | null,
		newSession: null as Session | null, // Used to store the newly created session. Deleted after the first prompt is sent. This is to prevent an unnecessary fetch of its messages.
		pollingSession: null as string | null, // Contains the ID of a session that is currently being polled for completion.
		renamedSessions: [] as Session[],
		deletedSessions: [] as Session[],
		currentMessages: [] as Message[],
		isSidebarClosed: false as boolean,
		agents: [] as ResourceProviderGetResult<Agent>[],
		selectedAgents: new Map(),
		lastSelectedAgent: null as ResourceProviderGetResult<Agent> | null,
		attachments: [] as Attachment[],
		longRunningOperations: new Map<string, string>(), // sessionId -> operation_id
		coreConfiguration: null as CoreConfiguration | null,
		oneDriveWorkSchool: null as boolean | null,
		userProfiles: null as UserProfile | null,
		autoHideToasts: JSON.parse(sessionStorage.getItem('autoHideToasts') || 'true') as boolean,
		textSize: JSON.parse(sessionStorage.getItem('textSize') || '1') as number,
		highContrastMode: JSON.parse(sessionStorage.getItem('highContrastMode') || 'false') as boolean,
		sessionMessagePending: false,
	}),

	getters: {
		agentShowMessageTokens(): boolean {
			return this.lastSelectedAgent?.resource.show_message_tokens ?? true;
		},

		agentShowMessageRating(): boolean {
			return this.lastSelectedAgent?.resource.show_message_rating ?? true;
		},

		agentShowViewPrompt(): boolean {
			return this.lastSelectedAgent?.resource.show_view_prompt ?? true;
		},

		agentShowFileUpload(): boolean {
			return this.lastSelectedAgent?.resource.show_file_upload ?? true;
		},
	},

	actions: {
		async init(sessionId: string | undefined) {
			const appConfigStore = useAppConfigStore();
			await this.getAgents();

			// Watch for changes in autoHideToasts and update sessionStorage
			watch(
				() => this.autoHideToasts,
				(newValue: boolean) => {
					sessionStorage.setItem('autoHideToasts', JSON.stringify(newValue));
				},
			);

			// Watch for changes in textSize and update sessionStorage
			watch(
				() => this.textSize,
				(newValue: number) => {
					sessionStorage.setItem('textSize', JSON.stringify(newValue));
					document.documentElement.style.setProperty('--app-text-size', `${newValue}rem`);
				},
			);

			// Watch for changes in highContrastMode and update sessionStorage
			watch(
				() => this.highContrastMode,
				(newValue: boolean) => {
					sessionStorage.setItem('highContrastMode', JSON.stringify(newValue));
				},
			);

			// No need to load sessions if in kiosk mode, simply create a new one and skip.
			if (appConfigStore.isKioskMode) {
				const newSession = await api.addSession(this.getDefaultChatSessionProperties());
				this.changeSession(newSession);
				return;
			}

			await this.getSessions();

			const requestedSession = sessionId
				? this.sessions.find((s: Session) => s.sessionId === sessionId)
				: undefined;

			// If there is an existing session matching the one requested in the url, select it.
			// otherwise, if the portal is configured to show the previous session and it exists, select it.
			// otherwise, create a new chat conversation.
			if (requestedSession) {
				this.changeSession(requestedSession);
			} else if (appConfigStore.showLastConversionOnStartup && this.sessions.length > 0) {
				this.changeSession(this.sessions[0]);
			} else {
				// Create a new chat conversation instead of a temporary session
				const newSession = await this.addSession(this.getDefaultChatSessionProperties());
				this.changeSession(newSession);
			}

			await this.getUserProfiles();
		},

		addTemporarySession() {
			const tempSession = {
				...this.getDefaultChatSessionProperties(),
				id: 'new',
				is_temp: true,
				display_name: 'New Chat',
				type: 'chat',
				sessionId: 'temp',
				tokensUsed: 0,
				messages: [],
			} as Session;
			this.sessions.unshift(tempSession);

			// Ensure a default agent is selected for the temporary session
			const defaultAgent =
				this.agents.find((agent) => agent.resource.properties?.default_resource) || this.agents[0];
			if (defaultAgent) {
				this.setSessionAgent(tempSession, defaultAgent);
				this.lastSelectedAgent = defaultAgent;
			}
		},

		getDefaultChatSessionProperties(): ChatSessionProperties {
			const now = new Date();
			// Using the 'sv-SE' locale since it uses the 'YYY-MM-DD' format.
			const formattedNow = now
				.toLocaleString('sv-SE', {
					year: 'numeric',
					month: '2-digit',
					day: '2-digit',
					hour: '2-digit',
					minute: '2-digit',
					second: '2-digit',
					hour12: false,
				})
				.replace(' ', 'T')
				.replace('T', ' ');

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
				const index = sessions.findIndex((s) => s.sessionId === session.sessionId);
				if (index !== -1) {
					sessions.splice(index, 1, session);
				}
				this.sessions = sessions;
			} else {
				this.sessions = sessions;
			}

			// Handle inconsistencies in displaying the renamed session due to potential delays in the backend updating the session name.
			this.renamedSessions.forEach((renamedSession: Session) => {
				const existingSession = this.sessions.find(
					(s: Session) => s.sessionId === renamedSession.sessionId,
				);
				if (existingSession) {
					existingSession.display_name = renamedSession.display_name;
				}
			});

			// Handle inconsistencies in displaying the deleted session due to potential delays in the backend updating the session list.
			this.deletedSessions.forEach((deletedSession: Session) => {
				const existingSession = this.sessions.find(
					(s: Session) => s.sessionId === deletedSession.sessionId,
				);
				if (existingSession) {
					this.removeSession(deletedSession.sessionId);
				}
			});
		},

		async addSession(properties: ChatSessionProperties) {
			if (!properties) {
				properties = this.getDefaultChatSessionProperties();
			}

			const newSession = await api.addSession(properties);
			await this.getSessions(newSession);
			this.newSession = newSession;

			// Only add newSession to the list if it doesn't already exist.
			// We optionally add it because the backend is sometimes slow to update the session list.
			if (!this.sessions.find((session: Session) => session.sessionId === newSession.sessionId)) {
				this.sessions = [newSession, ...this.sessions];
			}

			return newSession;
		},

		async renameSession(sessionToRename: Session, newSessionName: string) {
			const existingSession = this.sessions.find(
				(session: Session) => session.sessionId === sessionToRename.sessionId,
			);

			// Preemptively rename the session for responsiveness, and revert the name if the request fails.
			const previousName = existingSession.display_name;
			existingSession.display_name = newSessionName;

			try {
				await api.renameSession(sessionToRename.sessionId, newSessionName);
				const existingRenamedSession = this.renamedSessions.find(
					(session: Session) => session.sessionId === sessionToRename.sessionId,
				);
				if (existingRenamedSession) {
					existingRenamedSession.display_name = newSessionName;
				} else {
					this.renamedSessions = [
						{ ...sessionToRename, display_name: newSessionName },
						...this.renamedSessions,
					];
				}
			} catch (error) {
				existingSession.display_name = previousName;
			}
		},

		async deleteSession(sessionToDelete: Session) {
			await api.deleteSession(sessionToDelete!.sessionId);
			await this.getSessions();

			this.removeSession(sessionToDelete!.sessionId);

			// Add the deleted session to the list of deleted sessions to handle inconsistencies in the backend updating the session list.
			this.deletedSessions = [sessionToDelete, ...this.deletedSessions];

			// Ensure there is at least always 1 session
			if (this.sessions.length === 0) {
				const newSession = await this.addSession(this.getDefaultChatSessionProperties());
				this.removeSession(sessionToDelete!.sessionId);
				this.changeSession(newSession);
			}

			const firstSession = this.sessions[0];
			if (firstSession) {
				this.changeSession(firstSession);
			}
		},

		removeSession(sessionId: string) {
			this.sessions = this.sessions.filter((session: Session) => session.sessionId !== sessionId);
		},

		initializeMessageContent(content: MessageContent) {
			return reactive({
				...content,
				blobUrl: '',
				loading: true,
				error: false,
			});
		},

		async getMessages() {
			if (
				(this.newSession && this.newSession.sessionId === this.currentSession!.sessionId) ||
				this.currentSession.is_temp
			) {
				// This is a new session, no need to fetch messages.
				this.currentMessages = [];
				return;
			}

			const messagesResponse = await api.getMessages(this.currentSession?.sessionId);

			// Temporarily filter out the duplicate streaming message instances
			// const uniqueMessages = messagesResponse.reduceRight((acc, current) => {
			// 	const isDuplicate = acc.find(item => {
			// 		return item.operation_id === current.operation_id && item.sender === current.sender;
			// 	});

			// 	if (!isDuplicate || current.sender !== 'Agent') {
			// 		acc.push(current);
			// 	}

			// 	return acc;
			// }, []);

			// uniqueMessages.reverse();

			this.currentMessages = messagesResponse.map((message) => ({
				...message,
				content: message.content ? message.content.map(this.initializeMessageContent) : [],
				sender: message.sender?.toLowerCase() === 'user' ? 'User' : 'Agent',
			}));

			// Determine if the latest message needs to be polled
			if (this.currentMessages.length > 0) {
				const latestMessage = this.currentMessages[this.currentMessages.length - 1];

				// For older messages that have a status of "Pending" but no operation id, assume
				// it is complete and do no initiate polling as it will return empty data
				if (
					latestMessage.operation_id &&
					(latestMessage.status === 'InProgress' || latestMessage.status === 'Pending')
				) {
					this.startPolling(latestMessage, this.currentSession?.sessionId);
				}
			}

			this.calculateMessageProcessingTime();
		},

		calculateMessageProcessingTime() {
			// Calculate the processing time for each message
			this.currentMessages.forEach((message, index) => {
				if (message.sender === 'Agent' && this.currentMessages[index - 1]?.sender === 'User') {
					const previousMessageTimeStamp = new Date(
						this.currentMessages[index - 1].timeStamp,
					).getTime();
					const currentMessageTimeStamp = new Date(message.timeStamp).getTime();
					message.processingTime = currentMessageTimeStamp - previousMessageTimeStamp;
				}
			});
		},

		// async getMessage(messageId: string) {
		// 	const data = await api.getMessage(messageId);
		// 	const existingMessageIndex = this.currentMessages.findIndex(
		// 		(message) => message.id === messageId,
		// 	);

		// 	if (existingMessageIndex !== -1) {
		// 		this.currentMessages[existingMessageIndex] = data;
		// 		return data;
		// 	}

		// 	this.currentMessages.push(data);
		// 	return data;
		// },

		updateSessionAgentFromMessages(session: Session) {
			const lastAssistantMessage = this.currentMessages
				.filter((message) => message.sender.toLowerCase() === 'agent')
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
			let selectedAgent = this.selectedAgents.get(session.sessionId);

			if (!selectedAgent) {
				if (this.lastSelectedAgent && !isAgentExpired(this.lastSelectedAgent)) {
					// Default to the last selected agent to make the selection "sticky" across sessions.
					selectedAgent = this.lastSelectedAgent;
				} else {
					// Find an agent in the list that is configured as the default agent.
					selectedAgent = this.agents.find((agent) => agent.resource.properties?.default_resource);
					if (!selectedAgent || selectedAgent.resource.properties?.default_resource !== 'true') {
						// Default to the first agent in the list.
						selectedAgent = this.agents[0];
					}
				}
			}
			return selectedAgent;
		},

		setSessionAgent(session: Session, agent: ResourceProviderGetResult<Agent>) {
			this.lastSelectedAgent = agent;
			return this.selectedAgents.set(session.sessionId, agent);
		},

		/**
		 * Sends a message to the Core API.
		 *
		 * @param text - The text of the message to send.
		 * @returns A boolean indicating whether to wait for a polling operation to complete.
		 */
		async sendMessage(text: string): Promise<boolean> {
			let waitForPolling = false;
			if (!text) return waitForPolling;

			const agent = this.getSessionAgent(this.currentSession!).resource;
			const sessionId = this.currentSession!.sessionId;
			const relevantAttachments = this.attachments.filter(
				(attachment) => attachment.sessionId === sessionId,
			);

			const attachmentDetails = relevantAttachments.map((attachment) => ({
				objectId: attachment.id,
				displayName: attachment.fileName,
				contentType: attachment.contentType,
			}));

			const authStore = useAuthStore();
			const tempUserMessage: Message = {
				completionPromptId: null,
				id: null,
				rating: null,
				sender: 'User',
				senderDisplayName: authStore.currentAccount?.name ?? 'You',
				sessionId: this.currentSession!.sessionId,
				text,
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'Message',
				vector: [],
				attachmentDetails,
				renderId: Math.random(),
			};
			this.currentMessages.push(tempUserMessage);

			const tempAssistantMessage: Message = {
				completionPromptId: null,
				id: '',
				rating: null,
				sender: 'Agent',
				senderDisplayName: agent.name,
				sessionId: this.currentSession!.sessionId,
				text: '',
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'LoadingMessage',
				vector: [],
				status: 'Loading',
				renderId: Math.random(),
			};
			this.currentMessages.push(tempAssistantMessage);

			if (this.currentSession.is_temp) {
				const newSession = await this.addSession(this.getDefaultChatSessionProperties());
				this.changeSession(newSession);
			}

			const initialSession = this.currentSession?.sessionId;
			
			try {
				const message = await api.sendMessage(
					this.currentSession!.sessionId,
					text,
					agent,
					relevantAttachments.map((attachment) => String(attachment.id)),
				);

				if (message.status === 'Completed') {
					// The endpoint likely returned the final message, so we can update the last message in the list.
					const completedMessage = message.result as Message;
					// Replace the last message with the completed message.
					this.currentMessages[this.currentMessages.length - 1] = completedMessage;
					this.calculateMessageProcessingTime();
					return waitForPolling;
				}

				this.currentMessages[this.currentMessages.length - 1] = {
					...tempAssistantMessage,
					...message,
					sender: 'Agent',
					type: 'Message',
					text: message.status_message,
				};

				this.attachments = this.attachments.filter(
					(attachment) => attachment.sessionId !== sessionId,
				);

				// If the session has changed before above completes we need to prevent polling
				if (initialSession !== this.currentSession?.sessionId) return waitForPolling;

				// If the operation failed to start prevent polling
				if (message.status === 'Failed') return waitForPolling;

				waitForPolling = true;

				// For older messages that have a status of "Pending" but no operation id, assume
				// it is complete and do no initiate polling as it will return empty data
				if (message.operation_id) this.startPolling(message, this.currentSession?.sessionId);

				// Remove the new session if matches this one, now that we have sent the first message.
				if (this.newSession && this.newSession.sessionId === initialSession) {
					this.newSession = null;
				}

				return waitForPolling;
			} catch (error: any) {
				// Remove the temporary messages we added
				this.currentMessages = this.currentMessages.slice(0, -2);
				
				// Check if the error is a rate limit error
				if (error.data?.quota_exceeded) {
					const rateLimitError = error.data as RateLimitError;
					const waitTime = rateLimitError.time_until_retry_seconds;
					
					// Format the wait time in a more natural way
					let timeString;
					if (waitTime < 60) {
						timeString = `${waitTime} second${waitTime !== 1 ? 's' : ''}`;
					} else {
						const minutes = Math.floor(waitTime / 60);
						const seconds = waitTime % 60;
						if (seconds === 0) {
							timeString = `${minutes} minute${minutes !== 1 ? 's' : ''}`;
						} else {
							timeString = `${minutes} minute${minutes !== 1 ? 's' : ''} and ${seconds} second${seconds !== 1 ? 's' : ''}`;
						}
					}
					
					this.addToast({
						severity: 'warn',
						summary: 'Rate Limited',
						detail: `You have sent too many messages in a short period of time. Please wait ${timeString} before trying again.`,
						life: 5000,
					});
				} else {
					// Handle different types of errors
					let errorMessage = 'Failed to send message. Please try again.';
					
					if (error.message?.includes('Failed to fetch')) {
						errorMessage = 'Unable to connect to the server. Please check your internet connection and try again.';
					} else if (error.message?.includes('NetworkError')) {
						errorMessage = 'Network error occurred. Please check your connection and try again.';
					} else if (error.message?.includes('timeout')) {
						errorMessage = 'The request timed out. Please try again.';
					} else if (error.message) {
						// If there's a specific error message, use it but clean it up
						errorMessage = error.message
							.replace(/^https?:\/\/[^/]+\//, '') // Remove URL prefix
							.replace(/<no response>/g, '') // Remove "no response" text
							.trim();
					}
					
					this.addToast({
						severity: 'error',
						summary: 'Error',
						detail: errorMessage,
						life: 5000,
					});
				}
				
				return false;
			}
		},

		getPollingRateMS() {
			return (
				this.coreConfiguration?.completionResponsePollingIntervalSeconds * 1000 ||
				DEFAULT_POLLING_INTERVAL_MS
			);
		},

		startPolling(message, sessionId: string) {
			if (this.pollingInterval) return;

			// Indicate that a message in this session is being polled for completion.
			this.pollingSession = sessionId;

			this.pollingInterval = setInterval(async () => {
				try {
					const statusResponse = await api.checkProcessStatus(message.operation_id);
					const updatedMessage = statusResponse.result ?? {};
					this.currentMessages[this.currentMessages.length - 1] = {
						...updatedMessage,
						renderId: this.currentMessages[this.currentMessages.length - 1].renderId,
						sender: 'Agent',
					};

					const userMessage = this.currentMessages[this.currentMessages.length - 2];
					if (
						userMessage &&
						statusResponse.prompt_tokens &&
						userMessage.tokens !== statusResponse.prompt_tokens
					) {
						userMessage.tokens = statusResponse.prompt_tokens;
					}

					this.calculateMessageProcessingTime();

					if (updatedMessage.status === 'Completed' || updatedMessage.status === 'Failed') {
						this.stopPolling(sessionId);
					}
				} catch (error) {
					console.error(error);
					this.stopPolling(sessionId);
				}
			}, this.getPollingRateMS());
		},

		stopPolling(/* sessionId: string */) {
			clearInterval(this.pollingInterval);
			this.pollingInterval = null;
			this.pollingSession = null;
			this.sessionMessagePending = false;
		},

		/**
		 * Polls for the completion of a long-running operation.
		 *
		 * @param sessionId - The session ID associated with the operation.
		 * @param operationId - The ID of the operation to check for completion.
		 */
		// async pollForCompletion(sessionId: string, operationId: string) {
		// 	while (true) {
		// 		const status = await api.checkProcessStatus(operationId);
		// 		if (status.isCompleted) {
		// 			this.longRunningOperations.delete(sessionId);
		// 			eventBus.emit('operation-completed', { sessionId, operationId });
		// 			await this.getMessages();
		// 			break;
		// 		}
		// 		await new Promise((resolve) => setTimeout(resolve, 2000)); // Poll every 2 seconds
		// 	}
		// },

		async rateMessage(messageToRate: Message) {
			const existingMessage = this.currentMessages.find(
				(message) => message.id === messageToRate.id,
			);

			// Preemptively rate the message for responsiveness, and revert the rating if the request fails.
			const previousRating = existingMessage.rating;
			const previousRatingComments = existingMessage.ratingComments;
			existingMessage.rating = messageToRate.rating;
			existingMessage.ratingComments = messageToRate.ratingComments;

			try {
				await api.rateMessage(messageToRate);
			} catch (error) {
				existingMessage.rating = previousRating;
				existingMessage.ratingComments = previousRatingComments;
			}
		},

		changeSession(newSession: Session) {
			this.stopPolling(newSession.sessionId);

			const nuxtApp = useNuxtApp();
			const appConfigStore = useAppConfigStore();

			if (appConfigStore.isKioskMode || newSession.is_temp) {
				nuxtApp.$router.push({ query: {} });
			} else {
				const query = { chat: newSession.sessionId };
				nuxtApp.$router.push({ query });
			}

			this.currentSession = newSession;
			// await this.getMessages();
			// this.updateSessionAgentFromMessages(newSession);
		},

		toggleSidebar() {
			this.isSidebarClosed = !this.isSidebarClosed;
		},

		async getAgents() {
			this.agents = await api.getAllowedAgents();
			return this.agents;
		},

		mapAgentDisplayName(agentName: string) {
			const agent = this.agents.find((a) => a.resource.name === agentName);
			return agent?.resource.display_name?.trim() ? agent.resource.display_name : agentName;
		},

		async ensureAgentsLoaded() {
			let retryCount = 0;
			while (this.agents?.length === 0 && retryCount < 10) {
				await new Promise((resolve) => setTimeout(resolve, 500));
				retryCount += 1;
			}
		},

		async getCoreConfiguration() {
			this.coreConfiguration = await api.getCoreConfiguration();
			return this.coreConfiguration;
		},

		async oneDriveWorkSchoolConnect() {
			await api.oneDriveWorkSchoolConnect();
			this.oneDriveWorkSchool = true;
		},

		async oneDriveWorkSchoolDisconnect() {
			await api.oneDriveWorkSchoolDisconnect();
			this.oneDriveWorkSchool = false;
		},

		async getUserProfiles() {
			this.userProfiles = await api.getUserProfile();
			this.oneDriveWorkSchool = this.userProfiles?.flags.oneDriveWorkSchoolEnabled;
			return this.userProfiles;
		},

		async oneDriveWorkSchoolDownload(sessionId: string, oneDriveWorkSchool: OneDriveWorkSchool) {
			const agent = this.getSessionAgent(this.currentSession!).resource;
			// If the agent is not found, do not upload the attachment and display an error message.
			if (!agent) {
				throw new Error('No agent selected.');
			}

			const item = (await api.oneDriveWorkSchoolDownload(
				sessionId,
				agent.name,
				oneDriveWorkSchool,
			)) as OneDriveWorkSchool;
			const newAttachment: Attachment = {
				id: item.objectId!,
				fileName: item.name!,
				sessionId,
				contentType: item.mimeType!,
				source: 'OneDrive Work/School',
			};

			this.attachments.push(newAttachment);

			return item.objectId;
		},

		async uploadAttachment(file: FormData, sessionId: string, progressCallback: Function) {
			const agent = this.getSessionAgent(this.currentSession!).resource;
			// If the agent is not found, do not upload the attachment and display an error message.
			if (!agent) {
				throw new Error('No agent selected.');
			}

			const upsertResult = (await api.uploadAttachment(
				file,
				sessionId,
				agent.name,
				progressCallback,
			)) as ResourceProviderUpsertResult;
			const fileName = file.get('file')?.name;
			const contentType = file.get('file')?.type;
			const newAttachment: Attachment = {
				id: upsertResult.objectId,
				fileName,
				sessionId,
				contentType,
				source: 'Local Computer',
			};

			this.attachments.push(newAttachment);

			return upsertResult.objectId;
		},

		async deleteAttachment(attachment: Attachment) {
			const deleteResults: ResourceProviderDeleteResults = await api.deleteAttachments([
				attachment.id,
			]);
			Object.entries(deleteResults).forEach(([key, value]) => {
				if (key === attachment.id) {
					if (value.deleted) {
						this.attachments = this.attachments.filter((a) => a.id !== attachment.id);
					} else {
						throw new Error(`Could not delete the attachment: ${value.reason}`);
					}
				}
			});
		},

		async getVirtualUser() {
			return await api.getVirtualUser();
		},

		addToast(toastProperties: ToastMessageOptions) {
			const lifeSeconds = toastProperties?.life ?? 5000;

			useNuxtApp().vueApp.config.globalProperties.$toast.add({
				...toastProperties,
				life: this.autoHideToasts ? lifeSeconds : undefined,
			});
		},
	},
});
