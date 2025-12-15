import { defineStore } from 'pinia';
import { watch, reactive } from 'vue';
import type { ToastMessageOptions } from 'primevue/toast';
import { useAppConfigStore } from './appConfigStore';
import { useAuthStore } from './authStore';
import type {
	Session,
	ConversationProperties,
	Message,
	UserProfile,
	AgentBase,
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

const DEFAULT_POLLING_INTERVAL_MS = 250;

export const useAppStore = defineStore('app', {
	state: () => ({
		sessions: [] as Session[],
		currentSession: null as Session | null,
		// Indicates whether we have already confirmed that the current session has no messages.
		// In certain cases (e.g., newly created session, session already checked for messages),
		// we know for sure whether the session has messages or not.
		currentSessionConfirmedEmpty: false as boolean,
		newSession: null as Session | null, // Used to store the newly created session. Deleted after the first prompt is sent. This is to prevent an unnecessary fetch of its messages.
		pollingSession: null as string | null, // Contains the ID of a session that is currently being polled for completion.
		renamedSessions: [] as Session[],
		deletedSessions: [] as Session[],
		currentMessages: [] as Message[],
		isSidebarClosed: false as boolean,
		agents: [] as ResourceProviderGetResult<AgentBase>[],
		selectedAgents: new Map(),
		lastSelectedAgent: null as ResourceProviderGetResult<AgentBase> | null,
		attachments: [] as Attachment[],
		longRunningOperations: new Map<string, string>(), // sessionId -> operation_id
		coreConfiguration: null as CoreConfiguration | null,
		oneDriveWorkSchool: null as boolean | null,
		userProfile: null as UserProfile | null,
		autoHideToasts: JSON.parse(sessionStorage.getItem('autoHideToasts') || 'true') as boolean,
		textSize: JSON.parse(sessionStorage.getItem('textSize') || '1') as number,
		highContrastMode: JSON.parse(sessionStorage.getItem('highContrastMode') || 'false') as boolean,
		sessionMessagePending: false,
		settingsModalVisible: false,
		isInitialized: false, // Track if the app initialization is complete

		agentsLoadingPromise: null as Promise<void> | null,
		isAgentsLoaded: false,

		userProfileLoadingPromise: null as Promise<UserProfile | null> | null,
		isUserProfileLoaded: false,

		messagesLoadingPromise: null as Promise<void> | null,
		messagesLoadingSessionId: null as string | null,
	}),

	getters: {
		agentShowMessageTokens(): boolean {
			return !!(this.lastSelectedAgent && this.lastSelectedAgent.resource?.show_message_tokens);
		},

		agentShowMessageRating(): boolean {
			return !!(this.lastSelectedAgent && this.lastSelectedAgent.resource?.show_message_rating);
		},

		agentShowViewPrompt(): boolean {
			return !!(this.lastSelectedAgent && this.lastSelectedAgent.resource?.show_view_prompt);
		},

		agentShowFileUpload(): boolean {
			return !!(this.lastSelectedAgent && this.lastSelectedAgent.resource?.show_file_upload);
		},
	},

	actions: {
		async init(sessionId: string | undefined) {
			const appConfigStore = useAppConfigStore();

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
				await this.getAgents();
				const newSession = await api.addSession(this.getDefaultChatSessionProperties());
				this.changeSession(newSession);
				return;
			}

			// Parallelize independent loads: agents, sessions, and user profile
			const [, , ] = await Promise.all([
				this.getAgents(),
				this.getSessions(),
				this.getUserProfile(),
			]);

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
			} else if (this.sessions.length > 0) {
				// Check if the most recent session is empty
				const mostRecentSession = this.sessions[0];
				const isEmpty = await this.isSessionEmpty(mostRecentSession.sessionId);

				if (isEmpty) {
					// Use the existing empty session
					this.resetSessionAgent(mostRecentSession);
					this.changeSession(mostRecentSession, true);
				} else {
					// Create a new chat conversation
					const newSession = await this.addSession(this.getDefaultChatSessionProperties());
					this.changeSession(newSession, true);
				}
			} else {
				// No sessions exist, create a new chat conversation
				const newSession = await this.addSession(this.getDefaultChatSessionProperties());
				this.changeSession(newSession, true);
			}

			// Restore agent for the chosen currentSession, if any
			if (this.currentSession) {
				const restoredAgent = this.getSessionAgent(this.currentSession);
				this.lastSelectedAgent = restoredAgent;
			}

			// Mark initialization as complete
			this.isInitialized = true;
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

		getDefaultChatSessionProperties(): ConversationProperties {
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
				metadata: '',
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

		async addSession(properties: ConversationProperties) {
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

		async updateConversation(sessionToRename: Session, newConversationName: string, newMetadata: string) {
			const existingSession = this.sessions.find(
				(session: Session) => session.sessionId === sessionToRename.sessionId,
			);

			if (!existingSession) {
				throw new Error('Session not found');
			}

			// Preemptively rename the session for responsiveness, and revert the name if the request fails.
			const previousName = existingSession.display_name;
			const previousMetadata = existingSession.metadata;
			existingSession.display_name = newConversationName;
			existingSession.metadata = newMetadata;

			try {
				await api.updateConversation(sessionToRename.sessionId, newConversationName, newMetadata);
				const existingRenamedSession = this.renamedSessions.find(
					(session: Session) => session.sessionId === sessionToRename.sessionId,
				);
				if (existingRenamedSession) {
					existingRenamedSession.display_name = newConversationName;
					existingRenamedSession.metadata = newMetadata;
				} else {
					this.renamedSessions = [
						{ ...sessionToRename, display_name: newConversationName, metadata: newMetadata },
						...this.renamedSessions,
					];
				}
			} catch (error) {
				existingSession.display_name = previousName;
				existingSession.metadata = previousMetadata;
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

		async isSessionEmpty(sessionId: string): Promise<boolean> {
			try {
				const messagesCount = await api.getMessagesCount(sessionId);
				return !messagesCount || messagesCount === 0;
			} catch (error) {
				return false;
			}
		},

		async getMessages() {
			if (this.currentSessionConfirmedEmpty) {
				// We have already confirmed this session has no messages.
				this.currentMessages = [];
				return;
			}

			const sessionId = this.currentSession?.sessionId;

			if (
				(this.newSession && this.newSession.sessionId === this.currentSession!.sessionId) ||
				this.currentSession.is_temp
			) {
				// This is a new session, no need to fetch messages.
				this.currentMessages = [];
				return;
			}

			// If already loading messages for this same session, return existing promise
			if (this.messagesLoadingPromise && this.messagesLoadingSessionId === sessionId) {
				return this.messagesLoadingPromise;
			}

			// If loading a different session, let it proceed (cancel previous conceptually)
    		this.messagesLoadingSessionId = sessionId;

			this.messagesLoadingPromise = (async () => {
				try {
					const messagesResponse = await api.getMessages(sessionId);

					// Only update if still the current session (prevent stale data from race conditions)
            		if (this.currentSession?.sessionId === sessionId) {
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
								this.startPolling(latestMessage, sessionId);
							}
						}

						this.calculateMessageProcessingTime();
					}
				} finally {
					// Only clear if this is still the active load for this session
					if (this.messagesLoadingSessionId === sessionId) {
						this.messagesLoadingPromise = null;
					}
				}
			})();

			return this.messagesLoadingPromise;
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
				const storedAgentId = localStorage.getItem(`session-agent-${session.sessionId}`);
				if (storedAgentId) {
					selectedAgent = this.agents.find(agent => agent.resource.object_id === storedAgentId);
				}
			}
			// If selected agent is expired, remove it
			if (selectedAgent && isAgentExpired(selectedAgent)) {
				localStorage.removeItem(`session-agent-${session.sessionId}`);
				this.selectedAgents.delete(session.sessionId);
				selectedAgent = null;
			}
			return selectedAgent || null;
		},

		setSessionAgent(session: Session, agent: ResourceProviderGetResult<AgentBase>, shouldPersist: boolean = false) {
			this.lastSelectedAgent = agent;
			this.selectedAgents.set(session.sessionId, agent);
			if (shouldPersist && agent?.resource?.object_id) {
				const key = `session-agent-${session.sessionId}`;
				const value = agent.resource.object_id;
				localStorage.setItem(key, value);
			}
			return this.selectedAgents;
		},

		resetSessionAgent(session: Session) {
			this.lastSelectedAgent = null;
			this.selectedAgents.delete(session.sessionId)
			localStorage.removeItem(`session-agent-${session.sessionId}`);
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

			let metadataObj = null;
			if (this.currentSession?.metadata) {
				try {
					// Attempt to parse the metadata as JSON
					metadataObj = typeof this.currentSession.metadata === 'string'
						? JSON.parse(this.currentSession.metadata)
						: this.currentSession.metadata;
				} catch (e) {
					console.error('Invalid metadata format:', e);
				}
			}

			try {
				const message = await api.sendMessage(
					this.currentSession!.sessionId,
					text,
					agent,
					relevantAttachments.map((attachment) => String(attachment.id)),
					metadataObj
				);

				if (message.status === 'Completed'
					|| message.status === 'Failed'
				) {
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
				this.coreConfiguration?.completionResponsePollingIntervalMilliseconds ||
				DEFAULT_POLLING_INTERVAL_MS
			);
		},

		startPolling(message, sessionId: string) {
			if (this.pollingInterval) return;

			// Indicate that a message in this session is being polled for completion.
			this.pollingSession = sessionId;

			const poll = async () => {
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
						return;
					}
				} catch (error) {
					console.error(error);
					this.stopPolling(sessionId);
					return;
				}

				// Wait for the polling interval before the next call
				this.pollingInterval = setTimeout(poll, this.getPollingRateMS());
			};

			poll();
		},

		stopPolling(/* sessionId: string */) {
			clearTimeout(this.pollingInterval);
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

		changeSession(
			session: Session,
			confirmedEmpty: boolean = false,
		) {
			this.stopPolling(session.sessionId);

			const nuxtApp = useNuxtApp();
			const appConfigStore = useAppConfigStore();

			if (appConfigStore.isKioskMode || session.is_temp) {
				nuxtApp.$router.push({ query: {} });
			} else {
				const query = { chat: session.sessionId };
				nuxtApp.$router.push({ query });
			}

			this.currentSessionConfirmedEmpty = confirmedEmpty;
			this.currentSession = session;
		},

		toggleSidebar() {
			this.isSidebarClosed = !this.isSidebarClosed;
		},

		async getAgents() {
			if (this.isAgentsLoaded) {
				return;
			}

			if (this.agentsLoadingPromise) {
				return this.agentsLoadingPromise;
			}

			this.agentsLoadingPromise = (async () => {
				try {
					this.agents = await api.getAllowedAgents();
					this.isAgentsLoaded = true;
				} finally {
					this.agentsLoadingPromise = null;
				}
			})();

			return this.agentsLoadingPromise;
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

		async getUserProfile() {
			if (this.isUserProfileLoaded) {
				return this.userProfile;
			}

			if (this.userProfileLoadingPromise) {
				return this.userProfileLoadingPromise;
			}

			this.userProfileLoadingPromise = (async () => {
				try {
					
					this.userProfile = await api.getUserProfile();
					this.oneDriveWorkSchool = this.userProfile?.flags.oneDriveWorkSchoolEnabled;
					this.isUserProfileLoaded = true;
				} finally {
					this.userProfileLoadingPromise = null;
				}
			})();

			return this.userProfileLoadingPromise;
		},

		updateUserProfileAgent(agentObjectId: string, enabled: boolean) {
			if (!this.userProfile) {
				return;
			}

			if (!this.userProfile.agents) {
				this.userProfile.agents = [];
			}

			if (enabled) {
				// Add agent if not already present
				if (!this.userProfile.agents.includes(agentObjectId)) {
					this.userProfile.agents.push(agentObjectId);
				}
			} else {
				// Remove agent
				const index = this.userProfile.agents.indexOf(agentObjectId);
				if (index > -1) {
					this.userProfile.agents.splice(index, 1);
				}
			}
		},

		/**
		 * Updates a property on an agent's properties dictionary.
		 * @param agentObjectId The object_id of the agent to update.
		 * @param propertyName The name of the property to set.
		 * @param propertyValue The value to set for the property.
		 */
		updateAgentProperty(agentObjectId: string, propertyName: string, propertyValue: unknown) {
			const agent = this.agents.find((a) => a.resource.object_id === agentObjectId);
			if (agent) {
				if (!agent.properties) {
					agent.properties = {};
				}
				agent.properties[propertyName] = propertyValue;
			}
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
				id: upsertResult.object_id,
				fileName,
				sessionId,
				contentType,
				source: 'Local Computer',
			};

			this.attachments.push(newAttachment);

			return upsertResult.object_id;
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

		async deleteAttachmentsForSession(sessionId: string) {
			// Remove all attachments that belong to the provided sessionId
            this.attachments = this.attachments.filter((a) => a.sessionId !== sessionId);
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
