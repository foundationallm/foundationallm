import { defineStore } from 'pinia';
import { useAppConfigStore } from './appConfigStore';
import { useAuthStore } from './authStore';
import type { Session, Message } from '@/js/types';
import api from '@/js/api';

export const useAppStore = defineStore('app', {
	state: () => ({
		sessions: [] as Session[],
		currentSession: null as Session | null,
		currentMessages: [] as Message[],
	}),

	getters: {},

	actions: {
		async init(sessionId: string) {
			const appConfigStore = useAppConfigStore();

			// No need to load sessions if in kiosk mode, simply create a new one and skip.
			if (appConfigStore.isKioskMode) {
				const newSession = await api.addSession();
				this.changeSession(newSession);
				return;
			}

			await this.getSessions();

			if (this.sessions.length === 0) {
				await this.addSession();
				this.changeSession(this.sessions[0]);
			} else {
				const existingSession = this.sessions.find((session: Session) => session.id === sessionId);
				this.changeSession(existingSession || this.sessions[0]);
			}
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

		async addSession() {
			const newSession = await api.addSession();
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

			this.sessions = this.sessions.filter(
				(session: Session) => session.id !== sessionToDelete!.id,
			);

			// Ensure there is at least always 1 session
			if (this.sessions.length === 0) {
				const newSession = await this.addSession();
				this.changeSession(newSession);
				return;
			}

			const firstSession = this.sessions[0];
			if (firstSession) {
				this.changeSession(firstSession);
			}
		},

		async getMessages() {
			const data = await api.getMessages(this.currentSession.id);
			this.currentMessages = data;
		},

		async sendMessage(text: string) {
			if (!text) return;

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

			const appConfigStore = useAppConfigStore();
			await api.sendMessage(
				this.currentSession!.id,
				text,
				appConfigStore.selectedAgents.get(this.currentSession.id),
			);
			await this.getMessages();

			// Update the session name based on the message sent.
			if (this.currentMessages.length === 2) {
				const sessionFullText = this.currentMessages.map((message) => message.text).join('\n');
				const { text: newSessionName } = await api.summarizeSessionName(
					this.currentSession!.id,
					sessionFullText,
				);
				await api.renameSession(this.currentSession!.id, newSessionName);
				this.currentSession!.name = newSessionName;
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

		changeSession(newSession: Session) {
			const nuxtApp = useNuxtApp();
			const appConfigStore = useAppConfigStore();

			if (appConfigStore.isKioskMode) {
				nuxtApp.$router.push({ query: {} });
			} else {
				const query = { chat: newSession.id };
				nuxtApp.$router.push({ query });
			}

			this.currentSession = newSession;
		},
	},
});
