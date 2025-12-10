<template>
	<div class="chat-thread">
		<!-- Message list -->
		<div
			ref="messageContainer"
			class="chat-thread__messages printable-section"
			:class="messages.length === 0 && 'empty'"
		>
			<template v-if="isLoading">
				<div class="chat-thread__loading" role="status">
					<i
						class="pi pi-spin pi-spinner"
						style="font-size: 2rem"
						role="img"
						aria-label="Loading"
					></i>
				</div>
			</template>

			<template v-else-if="loadingFailed">
				<div class="chat-thread__loading" role="status">
					<i
						class="pi pi-exclamation-triangle"
						style="font-size: 1rem; margin-right: 8px"
						role="img"
						aria-label="Error"
					></i>
					<span>Failed to load session messages.</span>
				</div>
			</template>

			<template v-else>
				<!-- Messages -->
				<template v-if="messages.length !== 0">
					<ChatMessage
						v-for="(message, index) in messages.slice()"
						:id="`message-${getMessageOrderFromReversedIndex(index)}`"
						:key="message.renderId || message.id"
						:message="message"
						:show-word-animation="index === messages.length - 1 && message.sender !== 'User'"
						role="log"
						@rate="handleRateMessage($event.message)"
						@scroll-to-bottom="scrollToBottom($event)"
					/>
				</template>

				<!-- New chat alert -->
				<div v-else class="new-chat-alert">
					<div class="alert-body">
						<!-- eslint-disable-next-line vue/no-v-html -->
						<div class="alert-body-text" v-html="welcomeMessage"></div>
					</div>
				</div>
			</template>
		</div>

		<!-- Chat input -->
		<div class="chat-thread__input">
			<ChatInput
				ref="chatInput"
				:disabled="
					isLoading || loadingFailed || isMessagePending || appStore.sessionMessagePending
				"
				@send="handleSend"
			/>
		</div>

		<!-- Footer -->
		<!-- eslint-disable vue/no-v-html -->
		<footer
			v-if="appConfigStore.footerText"
			class="chat-thread__footer"
			v-html="appConfigStore.footerText"
		/>

		<!-- File drag and drop -->
		<div v-if="isDragging" ref="dropZone" class="drop-files-here-container">
			<div class="drop-files-here">
				<i class="pi pi-upload" style="font-size: 2rem"></i>
				<div>Drop files here to upload</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { Message, Session, ResourceProviderGetResult, Agent } from '@/js/types';
import { useAppStore } from '@/stores/appStore';
import { useAppConfigStore } from '@/stores/appConfigStore';

export default {
	name: 'ChatThread',

	props: {
		isDragging: Boolean,
	},

	emits: ['session-updated'],

	setup() {
		const appStore = useAppStore();
		const appConfigStore = useAppConfigStore();
		return { appStore, appConfigStore };
	},

	data() {
		return {
			isLoading: true as boolean,
			loadingFailed: false as boolean,
			userSentMessage: false as boolean,
			isMessagePending: false as boolean,
			welcomeMessage: '' as string,
		};
	},

	computed: {
		currentSession() {
			return this.appStore.currentSession;
		},

		pollingSession() {
			return this.appStore.pollingSession;
		},

		lastSelectedAgent() {
			return this.appStore.lastSelectedAgent;
		},

		messages() {
			return this.appStore.currentMessages;
		},
	},

	watch: {
		async currentSession(newSession: Session, oldSession: Session) {
			// Skip if app is still initializing (init() will handle initial message loading)
    		if (!this.appStore.isInitialized) return;

			const isReplacementForTempSession = oldSession?.is_temp && this.messages.length > 0;
			if (newSession.sessionId === oldSession?.sessionId || isReplacementForTempSession) return;
			this.isMessagePending = false;
			this.isLoading = true;
			this.loadingFailed = false;
			this.userSentMessage = false;

			try {
				await this.appStore.getMessages();
			} catch (error) {
				this.loadingFailed = true;
			}
			await this.appStore.ensureAgentsLoaded();

			this.appStore.updateSessionAgentFromMessages(newSession);
			const sessionAgent = this.appStore.getSessionAgent(newSession);
			this.welcomeMessage = this.getWelcomeMessage(sessionAgent);
			this.isLoading = false;
			this.scrollToBottom(0, true);
		},

		lastSelectedAgent(newAgent, oldAgent) {
			if (newAgent === oldAgent) return;
			this.welcomeMessage = this.getWelcomeMessage(newAgent);
		},

		pollingSession(newPollingSession, oldPollingSession) {
			if (newPollingSession === oldPollingSession) return;
			if (newPollingSession === this.currentSession.sessionId) {
				this.isMessagePending = true;
			} else {
				// When polling stops (newPollingSession becomes null), enable the input
				this.isMessagePending = false;
			}
		},

		messages: {
			handler() {
				this.scrollToBottom();
			},
			deep: true,
		},
	},

	created() {
		// Wait for app initialization before setting welcome message
    	const unwatchInitialized = this.$watch(
			() => this.appStore.isInitialized,
			(isInitialized) => {
				if (isInitialized) {
					if (
						!this.appConfigStore.showLastConversionOnStartup &&
						(this.currentSession as Session)?.is_temp
					) {
						this.isLoading = false;
						const sessionAgent = this.appStore.getSessionAgent(this.currentSession as Session);
						if (sessionAgent) {
							this.welcomeMessage = this.getWelcomeMessage(sessionAgent);
						} else {
							// If no agent is selected yet, wait for agents to load
							this.$watch(
								() => this.appStore.getSessionAgent(this.currentSession as Session),
								(newAgent: ResourceProviderGetResult<Agent> | null) => {
									this.isLoading = false;
									this.welcomeMessage = this.getWelcomeMessage(newAgent);
								},
								{ immediate: true },
							);
						}
					}
					unwatchInitialized(); // Stop watching after initialization
				}
			},
			{ immediate: true } // Check immediately in case already initialized
		);
	},

	methods: {
		getWelcomeMessage(agent) {
			// If no agent is provided, show the no agents message
			if (!agent) {
				return 'Enable at least one agent in Settings -> Agents so you can start new conversations.';
			}

			// Check if the agent is accessible to the user
			const userProfile = this.appStore.userProfile;
			const enabledAgentIds = userProfile?.agents || [];

			// If user has no enabled agents, show no agents message
			if (enabledAgentIds.length === 0) {
				return 'Enable at least one agent in Settings -> Agents so you can start new conversations.';
			}

			// Check if current agent is in user's enabled agents
			const foundAgent = enabledAgentIds.find((agentId: any) =>
				agentId == agent.resource.object_id
			);

			if (!foundAgent) {
				return 'Enable at least one agent in Settings -> Agents so you can start new conversations.';
			}

			// Agent is accessible, show its welcome message
			const welcomeMessage = agent?.resource?.properties?.welcome_message?.trim();
			return (
				welcomeMessage ||
				this.appConfigStore.defaultAgentWelcomeMessage ||
				'Start the conversation using the text box below.'
			);
		},

		getMessageOrderFromReversedIndex(index) {
			return this.messages.length - 1 - index;
		},

		async handleRateMessage(message: Message) {
			await this.appStore.rateMessage(message);
		},

		handleParentDrop(event) {
			event.preventDefault();
			const files = Array.from(event.dataTransfer?.files || []);
			this.$refs.chatInput.handleDrop(files);
		},

		async handleSend(text: string) {
			if (!text) return;

			this.isMessagePending = true;
			this.userSentMessage = true;

			const agent = this.appStore.getSessionAgent(this.currentSession)?.resource;

			// Display an error toast message if agent is null or undefined.
			if (!agent) {
				this.appStore.addToast({
					severity: 'info',
					summary: 'Could not send message',
					detail:
						'Please select an agent and try again. If no agents are available, refresh the page.',
					life: 8000,
				});
				this.isMessagePending = false;
				return;
			}

			// if (agent.long_running) {
			// 	// Handle long-running operations
			// 	const operationId = await this.appStore.startLongRunningProcess('/async-completions', {
			// 		session_id: this.currentSession.sessionId,
			// 		user_prompt: text,
			// 		agent_name: agent.name,
			// 		settings: null,
			// 		attachments: this.appStore.attachments.map((attachment) => String(attachment.id)),
			// 	});

			// 	this.longRunningOperations.set(this.currentSession.sessionId, true);
			// 	await this.pollForCompletion(this.currentSession.sessionId, operationId);
			// } else {
			const waitForPolling = await this.appStore.sendMessage(text);

			if (!waitForPolling) {
				this.isMessagePending = false;
			}
			// console.log(message);
			// await this.appStore.getMessages();
			// }

			// this.isMessagePending = false;
		},

		scrollToBottom(contentGrowth = 0, force = false) {
			const container = this.$refs.messageContainer;
			if (!container) return;

			const previousScrollHeight = container.scrollHeight;
			const previousScrollTop = container.scrollTop;
			const isNearBottom =
				previousScrollTop + container.clientHeight + contentGrowth >= previousScrollHeight - 100;

			this.$nextTick(() => {
				const newScrollHeight = container.scrollHeight;

				if (isNearBottom || force) {
					container.scrollTop = newScrollHeight;
				}
			});
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-thread {
	height: 100%;
	max-width: 100%;
	display: flex;
	flex-direction: column;
	position: relative;
	flex: 1;
}

.chat-thread__header {
	height: 70px;
	padding: 24px;
	border-bottom: 1px solid #eaeaea;
	background-color: var(--accent-color);
}

.chat-thread__loading {
	width: 100%;
	height: 100%;
	display: flex;
	justify-content: center;
	align-items: center;
}

.chat-thread__messages {
	display: flex;
	flex-direction: column;
	flex: 1;
	overflow-y: auto;
	overscroll-behavior: auto;
	scrollbar-gutter: stable;
	padding: 24px 32px;
	scrollbar-color: var(--thread-scrollbar-default) transparent;

	&:hover {
		scrollbar-color: var(--thread-scrollbar-focused) transparent;
	}
}

.chat-thread__input {
	display: flex;
	margin: 0px 24px 8px 24px;
	// box-shadow: 0 -5px 10px 0 rgba(27, 29, 33, 0.1);
}

.chat-thread__footer {
	text-align: right;
	font-size: 0.85rem;
	padding-right: 24px;
	margin-bottom: 12px;

	:first-child {
		margin-top: 0px;
	}

	:last-child {
		margin-bottom: 0px;
	}
}

.empty {
	flex-direction: column;
}

.new-chat-alert {
	background-color: #fafafa;
	margin: 10px;
	margin-left: auto;
	margin-right: auto;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
	padding: 10px;
	border-radius: 6px;
	width: 55%;
}

.alert-header,
.alert-header > i {
	display: flex;
	align-items: center;
	font-size: 1.5rem;
}

.alert-header-text {
	font-weight: 500;
	margin-left: 8px;
}

.alert-body-text {
	color: #000;
	margin-left: auto;
	margin-right: auto;
	padding: 10px 14px 10px 14px;
	// text-align: center;
	// font-style: italic;
}

.drop-files-here-container {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background-color: rgba(255, 255, 255, 0.8);
	z-index: 9999;
	display: flex;
	justify-content: center;
	align-items: center;
}

.drop-files-here {
	display: flex;
	flex-direction: column;
	align-items: center;
	border-radius: 6px;
	gap: 2rem;
}
</style>
