<template>
	<div class="chat-thread">
		<button
			label="Export Chat to PDF"
			icon="pi pi-file-pdf"
			@click="exportChatToPDF"
		/>
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

			<template v-else>
				<!-- Messages -->
				<template v-if="messages.length !== 0">
					<ChatMessage
						v-for="(message, index) in messages.slice().reverse()"
						:id="`message-${getMessageOrderFromReversedIndex(index)}`"
						:key="message.renderId || message.id"
						:message="message"
						:show-word-animation="index === 0 && message.sender !== 'User'"
						role="log"
						:aria-flowto="
							index === 0 ? null : `message-${getMessageOrderFromReversedIndex(index) + 1}`
						"
						@rate="handleRateMessage($event.message)"
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
			<ChatInput ref="chatInput" :disabled="isLoading || isMessagePending" @send="handleSend" />
		</div>

		<!-- Footer -->
		<!-- eslint-disable-next-line vue/no-v-html -->
		<footer
			v-if="$appConfigStore.footerText"
			class="chat-thread__footer"
			v-html="$appConfigStore.footerText"
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
import type { Message, Session } from '@/js/types';
import jsPDF from "jspdf";

export default {
	name: 'ChatThread',

	props: {
		isDragging: Boolean,
	},

	emits: ['session-updated'],

	data() {
		return {
			isLoading: true,
			userSentMessage: false,
			isMessagePending: false,
			welcomeMessage: '',
		};
	},

	computed: {
		currentSession() {
			return this.$appStore.currentSession;
		},

		pollingSession() {
			return this.$appStore.pollingSession;
		},

		lastSelectedAgent() {
			return this.$appStore.lastSelectedAgent;
		},

		messages() {
			return this.$appStore.currentMessages;
		},
	},

	watch: {
		async currentSession(newSession: Session, oldSession: Session) {
			const isReplacementForTempSession = oldSession?.is_temp && this.messages.length > 0;
			if (newSession.id === oldSession?.id || isReplacementForTempSession) return;
			this.isMessagePending = false;
			this.isLoading = true;
			this.userSentMessage = false;

			await this.$appStore.getMessages();
			await this.$appStore.ensureAgentsLoaded();

			this.$appStore.updateSessionAgentFromMessages(newSession);
			const sessionAgent = this.$appStore.getSessionAgent(newSession);
			this.welcomeMessage = this.getWelcomeMessage(sessionAgent);
			this.isLoading = false;
		},

		lastSelectedAgent(newAgent, oldAgent) {
			if (newAgent === oldAgent) return;
			this.welcomeMessage = this.getWelcomeMessage(newAgent);
		},

		pollingSession(newPollingSession, oldPollingSession) {
			if (newPollingSession === oldPollingSession) return;
			if (newPollingSession === this.currentSession.id) {
				this.isMessagePending = true;
			} else {
				this.isMessagePending = false;
			}
		},
	},

	created() {
		if (!this.$appConfigStore.showLastConversionOnStartup && this.currentSession?.is_temp) {
			this.isLoading = false;
			const sessionAgent = this.$appStore.getSessionAgent(this.currentSession);
			this.welcomeMessage = this.getWelcomeMessage(sessionAgent);
		}
	},

	methods: {
		getWelcomeMessage(agent) {
			const welcomeMessage = agent?.resource?.properties?.welcome_message;
			return welcomeMessage && welcomeMessage.trim() !== ''
				? welcomeMessage
				: (this.$appConfigStore.defaultAgentWelcomeMessage ??
						'Start the conversation using the text box below.');
		},

		getMessageOrderFromReversedIndex(index) {
			return this.messages.length - 1 - index;
		},

		async handleRateMessage(message: Message) {
			await this.$appStore.rateMessage(message);
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

			const agent = this.$appStore.getSessionAgent(this.currentSession)?.resource;

			// Display an error toast message if agent is null or undefined.
			if (!agent) {
				this.$toast.add({
					severity: 'info',
					summary: 'Could not send message',
					detail:
						'Please select an agent and try again. If no agents are available, refresh the page.',
					life: this.$appStore.autoHideToasts ? 8000 : null,
				});
				this.isMessagePending = false;
				return;
			}

			// if (agent.long_running) {
			// 	// Handle long-running operations
			// 	const operationId = await this.$appStore.startLongRunningProcess('/async-completions', {
			// 		session_id: this.currentSession.id,
			// 		user_prompt: text,
			// 		agent_name: agent.name,
			// 		settings: null,
			// 		attachments: this.$appStore.attachments.map((attachment) => String(attachment.id)),
			// 	});

			// 	this.longRunningOperations.set(this.currentSession.id, true);
			// 	await this.pollForCompletion(this.currentSession.id, operationId);
			// } else {
			await this.$appStore.sendMessage(text);
			// console.log(message);
			// await this.$appStore.getMessages();
			// }

			// this.isMessagePending = false;
		},

		async exportChatToPDF() {
		// 	const doc = new jsPDF({ unit: "mm", format: "a4" });
		// 	let y = 15;

		// 	const pageHeight = 297;
		// 	const margin = 10;
		// 	const maxWidth = 130;
		// 	const userBgColor = [173, 216, 230];
		// 	const agentBgColor = [230, 230, 230];

		// 	doc.setFont("helvetica", "bold");
		// 	doc.setFontSize(16);
		// 	doc.text("Chat Conversation", 105, y, { align: "center" });
		// 	y += 10;

		// 	for (const message of this.messages) {
		// 		const isUser = message.sender === "User";
		// 		const sender = isUser ? "You" : message.senderDisplayName || "Agent";
		// 		const timestamp = new Date(message.timeStamp).toLocaleString();
		// 		const textContent = message.text || message.content?.map(c => c.value).join("\n") || "[No content]";
		// 		const wrappedText = doc.splitTextToSize(textContent, maxWidth);

		// 		const boxX = isUser ? 210 - maxWidth - margin : margin;
		// 		const boxWidth = maxWidth + 5;
		// 		const boxHeight = wrappedText.length * 5 + 8;

		// 		if (y > pageHeight - margin) {
		// 			doc.addPage();
		// 			y = margin;
		// 		}

		// 		doc.setFontSize(8);
		// 		doc.setTextColor(100, 100, 100);
		// 		doc.text(`${timestamp} - ${sender}`, isUser ? 210 - margin : margin, y);

		// 		y += 4;

		// 		doc.setFillColor(...(isUser ? userBgColor : agentBgColor));
        //     	doc.roundedRect(boxX, y, boxWidth, boxHeight, 2, 2, "F");

		// 		doc.setFontSize(10);
		// 		doc.setTextColor(0, 0, 0);
		// 		wrappedText.forEach((line, index) => {
		// 			doc.text(line, boxX + 3, y + 7 + index * 5);
		// 		});

		// 		y += boxHeight + 5;
		// 	}

		// 	const pageCount = doc.internal.getNumberOfPages();
		// 	for (let i = 1; i <= pageCount; i++) {
		// 		doc.setPage(i);
		// 		doc.setFontSize(8);
		// 		doc.text(`Page ${i} of ${pageCount}`, 105, 290, { align: "center" });
		// 	}

		// 	doc.save("Improved_Chat_Conversation.pdf");
		// }
			console.log(this.$appConfigStore.primaryColor);

			const doc = new jsPDF({ unit: "mm", format: "a4" });
			const pageHeight = 297; // A4 height in mm
			const margin = 10; // Page margins
			const messageMargin = 6; // Message bubble padding
			let y = 15; // Start vertical position

			const maxWidth = 160;
			const userBgColor = '#FFFFFF'; // Background color for user messages
			const userTextColor = '#000000'; // Text color for user messages
			const agentBgColor = this.$appConfigStore.primaryColor; // Background color for agent messages
			const agentTextColor = this.$appConfigStore.primaryText; // Text colors for agent messages

			doc.setFont("helvetica");

			// Title
			doc.setFontSize(16);
			doc.text("Chat Conversation", 105, y, { align: "center" });
			y += 10;

			// Process messages
			for (const message of this.messages) {
				const isUser = message.sender === "User";
				const sender = isUser ? "You" : message.senderDisplayName || "Agent";
				const timestamp = new Date(message.timeStamp).toLocaleString();
				const textContent = message.text || message.content?.map(c => c.value).join("\n") || "[No content]";
				const wrappedText = doc.splitTextToSize(textContent, maxWidth);

				// Dynamic height calculation
				const boxHeight = (wrappedText.length * 5) + (messageMargin); // Text height + padding + margin

				// Handle page overflow before drawing
				if (y + boxHeight + 10 > pageHeight - margin) {
					doc.addPage();
					y = margin;
				}

				// Timestamp
				doc.setFontSize(8);
				doc.setTextColor(100, 100, 100);
				const textWidth = doc.getTextWidth(`${timestamp} - ${sender}`);
				const adjustedX = isUser ? 210 - margin - textWidth : margin;
				doc.text(`${timestamp} - ${sender}`, adjustedX, y);
				y += 4;

				// Message bubble
				const bubbleX = isUser ? 210 - maxWidth - (messageMargin * 2) - margin : margin; // Right or left alignment
				doc.setFillColor(isUser ? userBgColor : agentBgColor);
				doc.rect(bubbleX, y, maxWidth + (messageMargin * 2), boxHeight, "F");

				// Message text
				doc.setFontSize(10);
				doc.setTextColor(isUser ? userTextColor : agentTextColor);
				wrappedText.forEach((line, index) => {
					doc.text(line, bubbleX + messageMargin, y + messageMargin + index * 5);
				});

				y += boxHeight + 5; // Move to the next position
			}

			// Page numbers
			const totalPages = doc.internal.getNumberOfPages();
			for (let i = 1; i <= totalPages; i++) {
				doc.setPage(i);
				doc.setFontSize(8);
				doc.text(`Page ${i} of ${totalPages}`, 105, 290, { align: "center" });
			}

			// Save PDF
			doc.save("Chat_Conversation_Improved.pdf");
		}
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
	flex-direction: column-reverse;
	flex: 1;
	overflow-y: auto;
	overscroll-behavior: auto;
	scrollbar-gutter: stable;
	padding: 24px 32px;
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
