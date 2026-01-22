<template>
	<div>
		<div class="message-row" :class="message.sender === 'User' ? 'message--out' : 'message--in'">
			<div class="message" tabindex="0">
				<div class="message__header">
					<!-- Sender -->
					<span class="header__sender">
						<AgentIcon
							v-if="message.sender !== 'User'"
							:src="$appConfigStore.agentIconUrl || '~/assets/FLLM-Agent-Light.svg'"
							alt="Agent avatar"
						/>
						<span>{{ senderDisplayName }}</span>
					</span>

					<!-- Tokens & Timestamp -->
					<span class="message__header--right">
						<Chip
							v-if="$appConfigStore.showMessageTokens && $appStore.agentShowMessageTokens"
							:label="`Tokens: ${message.tokens}`"
							class="token-chip"
							:class="message.sender === 'User' ? 'token-chip--out' : 'token-chip--in'"
							:pt="{
								label: {
									style: {
										color: message.sender === 'User' ? 'var(--accent-text)' : 'var(--primary-text)',
									},
								},
							}"
						/>
						<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
							<span class="time-stamp" tabindex="0" @keydown.esc="hideAllPoppers">
								<TimeAgo :date="new Date(message.timeStamp)" />
							</span>
							<template #popper>
								<div role="tooltip">
									{{ buildTimeStampTooltip(message.timeStamp, message.processingTime) }}
								</div>
							</template>
						</VTooltip>

						<!-- Copy user message button -->
						<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
							<Button
								v-if="message.sender === 'User'"
								class="message__copy"
								size="small"
								text
								icon="pi pi-copy"
								aria-label="Copy Message"
								@click.stop="handleCopyMessageContent"
								@keydown.esc="hideAllPoppers"
							/>
							<template #popper><div role="tooltip">Copy Message</div></template>
						</VTooltip>
					</span>
				</div>

				<!-- Message text -->
				<div class="message__body" @click="handleFileLinkInText">
					<!-- Attachments -->
					<AttachmentList
						v-if="message.sender === 'User'"
						:attachments="message.attachmentDetails ?? []"
						:attachment-ids="message.attachments"
					/>

					<!-- Message loading -->
					<template v-if="message.sender === 'Assistant' && message.type === 'LoadingMessage'">
						<div role="status">
							<i class="pi pi-spin pi-spinner" role="img" aria-label="Loading message"></i>
						</div>
					</template>

					<!-- Render the html content and any vue components within -->
					<!-- <component :is="compiledMarkdownComponent" v-else /> -->

					<div v-for="(content, index) in processedContent" v-else :key="index">
						<template v-if="content.type === 'text'">
							<div v-if="message.sender === 'User'" style="white-space: pre-wrap">
								{{ content.value }}
							</div>

							<ChatMessageTextBlock v-else :value="content.value" />
						</template>

						<ChatMessageContentBlock v-else-if="content.type !== 'file_path'" :value="content" />
					</div>

					<div v-for="(artifact, artifactIndex) in message.contentArtifacts" :key="`${artifact.id}-${artifactIndex}`">
						<ChatMessageContentArtifactBlock v-if="artifact.type === 'image' && $appStore.agentShowContentArtifacts" :value="artifact" />
					</div>

					<!-- Analysis button -->
					<Button
						v-if="message.analysisResults && message.analysisResults.length > 0"
						class="message__button"
						:disabled="message.type === 'LoadingMessage'"
						size="small"
						text
						icon="pi pi-window-maximize"
						label="Analysis"
						@click.stop="isAnalysisModalVisible = true"
					/>
				</div>

				<!-- Assistant message footer -->
				<div v-if="message.sender !== 'User'" class="message__footer">
					<!-- Additional content -->
					<div
						v-if="processedContent.some((content) => content.type === 'file_path' && !content.skip)"
						class="additional-content"
					>
						<div><b>Additional Content: </b></div>
						<div
							v-for="(content, index) in processedContent.filter(
								(content) => content.type === 'file_path',
							)"
							:key="index"
						>
							<ChatMessageContentBlock :value="content" />
						</div>
					</div>

					<!-- Content artifacts -->
					<div v-if="message.contentArtifacts?.length && $appStore.agentShowContentArtifacts" class="content-artifacts">
						<span><b>Content Artifacts: </b></span>
						<span
							v-for="(artifact, artifactIndex) in message.contentArtifacts"
							:key="`${artifact.id}-${artifactIndex}`"
							v-tooltip.top="{ content: 'Click to view content', showDelay: 500, hideDelay: 300 }"
							class="content-artifact"
							@click="selectedContentArtifact = artifact"
						>
							<i class="pi pi-file"></i>
							{{ artifact.title ? artifact.title?.split('/').pop() : '(No Title)' }}
						</span>
					</div>

					<!-- Rating -->
					<span class="ratings">
						<template v-if="$appConfigStore.showMessageRating && $appStore.agentShowMessageRating">
							<!-- Rate message button -->
							<Button
								class="message__button"
								:disabled="message.type === 'LoadingMessage'"
								size="small"
								text
								:icon="
									message.rating === true
										? 'pi pi-thumbs-up-fill'
										: message.rating === false
											? 'pi pi-thumbs-down-fill'
											: 'pi pi-thumbs-up'
								"
								label="Rate Message"
								@click.stop="isRatingModalVisible = true"
							/>
						</template>
					</span>

					<!-- Avg MS Per Word: {{ averageTimePerWordMS }} -->
					<div v-if="messageDisplayStatus" class="loading-shimmer" style="font-weight: 600">
						{{ messageDisplayStatus }}
					</div>

					<!-- Right side buttons -->
					<span>
						<!-- Copy message button -->
						<Button
							:disabled="message.type === 'LoadingMessage'"
							class="message__button"
							size="small"
							text
							icon="pi pi-copy"
							label="Copy"
							@click.stop="handleCopyMessageContent"
						/>

						<!-- View prompt button -->
						<Button
							v-if="$appConfigStore.showViewPrompt && $appStore.agentShowViewPrompt"
							class="message__button"
							:disabled="message.type === 'LoadingMessage'"
							size="small"
							text
							icon="pi pi-book"
							label="View Prompt"
							@click.stop="handleViewPrompt"
						/>

						<!-- Prompt dialog -->
						<Dialog
							v-model:visible="viewPrompt"
							class="prompt-dialog"
							modal
							header="Completion Prompt"
						>
							<p class="prompt-text" tabindex="0">
								{{ prompt.prompt }}
							</p>
							<template #footer>
								<Button
									:style="{
										backgroundColor: $appConfigStore.primaryButtonBg,
										borderColor: $appConfigStore.primaryButtonBg,
										color: $appConfigStore.primaryButtonText,
									}"
									class="prompt-dialog__button"
									label="Close"
									@click="viewPrompt = false"
								/>
							</template>
						</Dialog>
					</span>
				</div>
			</div>
		</div>

		<!-- Date Divider -->
		<Divider v-if="message.sender === 'User'" align="center" type="solid" class="date-separator">
			<TimeAgo :date="new Date(message.timeStamp)" />
		</Divider>

		<!-- Analysis Modal -->
		<AnalysisModal
			:visible="isAnalysisModalVisible"
			:analysis-results="message.analysisResults ?? []"
			@update:visible="isAnalysisModalVisible = $event"
		/>

		<!-- Content Artifact Modal -->
		<Dialog
			v-model:visible="selectedContentArtifact"
			:header="selectedContentArtifact?.title"
			modal
			style="max-width: 85%"
		>
			<div tabindex="0" style="overflow-x: auto">
				<div v-if="selectedContentArtifact">
					<h3 style="width: 100%; margin-top: 0; margin-bottom: 12px; font-size: 1.1rem; font-weight: 600; color: #333;">Properties</h3>
					<table style="width: 100%; border-collapse: collapse; margin-bottom: 24px;">
						<thead>
							<tr>
								<th style="border: 1px solid #ddd; padding: 12px; text-align: left; background-color: #f5f5f5; font-weight: 600;">Property</th>
								<th style="border: 1px solid #ddd; padding: 12px; text-align: left; background-color: #f5f5f5; font-weight: 600;">Value</th>
							</tr>
						</thead>
						<tbody>
							<tr v-for="(value, key) in getMainProperties(selectedContentArtifact)" :key="`main-${key}`" class="artifact-table-row">
								<td style="border: 1px solid #ddd; padding: 12px; font-weight: 500; vertical-align: top;">{{ key }}</td>
								<td class="artifact-value-cell" style="border: 1px solid #ddd; padding: 0; position: relative;">
									<div class="artifact-value-content">
										<span>{{ formatPropertyValue(value) }}</span>
									</div>
									<Button
										v-if="value !== null && value !== undefined && value !== ''"
										class="copy-cell-button"
										size="small"
										text
										icon="pi pi-copy"
										aria-label="Copy value"
										@click.stop="handleCopyCellValue(formatPropertyValue(value))"
									/>
								</td>
							</tr>
						</tbody>
					</table>
				</div>

				<!-- Metadata Table -->
				<div v-if="selectedContentArtifact?.metadata && Object.keys(selectedContentArtifact.metadata).length > 0">
					<h3 style="margin-top: 0; margin-bottom: 12px; font-size: 1.1rem; font-weight: 600; color: #333;">Metadata</h3>
					<table style="width: 100%; border-collapse: collapse;">
						<thead>
							<tr>
								<th style="border: 1px solid #ddd; padding: 12px; text-align: left; background-color: #f5f5f5; font-weight: 600;">Property</th>
								<th style="border: 1px solid #ddd; padding: 12px; text-align: left; background-color: #f5f5f5; font-weight: 600;">Value</th>
							</tr>
						</thead>
						<tbody>
							<tr v-for="(value, key) in selectedContentArtifact.metadata" :key="`meta-${key}`" class="artifact-table-row">
								<td style="border: 1px solid #ddd; padding: 12px; font-weight: 500; vertical-align: top;">{{ key }}</td>
								<td class="artifact-value-cell" style="border: 1px solid #ddd; padding: 0; position: relative;">
									<div class="artifact-value-content">
										<span>{{ formatPropertyValue(value) }}</span>
									</div>
									<Button
										v-if="value !== null && value !== undefined && value !== ''"
										class="copy-cell-button"
										size="small"
										text
										icon="pi pi-copy"
										aria-label="Copy value"
										@click.stop="handleCopyCellValue(formatPropertyValue(value))"
									/>
								</td>
							</tr>
						</tbody>
					</table>
				</div>
			</div>

			<template #footer>
				<Button
					:style="{
						backgroundColor: $appConfigStore.primaryButtonBg,
						borderColor: $appConfigStore.primaryButtonBg,
						color: $appConfigStore.primaryButtonText,
					}"
					class="prompt-dialog__button"
					label="Close"
					@click="selectedContentArtifact = null"
				/>
			</template>
		</Dialog>

		<!-- Message Rating Modal -->
		<Dialog v-model:visible="isRatingModalVisible" header="Rate Message" modal>
			<label for="rating-textarea">Comments</label>
			<Textarea
				id="rating-textarea"
				v-model="message.ratingComments"
				style="width: 100%"
				rows="5"
				type="text"
				placeholder="Add comments here..."
				aria-label="Add comments here..."
				auto-resize
				autofocus
			></Textarea>

			<!-- Like -->
			<span>
				<Button
					class="message__button"
					:disabled="message.type === 'LoadingMessage'"
					size="small"
					text
					:icon="message.rating ? 'pi pi-thumbs-up-fill' : 'pi pi-thumbs-up'"
					:label="message.rating ? 'Message Liked' : 'Like'"
					@click="message.rating === true ? (message.rating = null) : (message.rating = true)"
				/>
			</span>

			<!-- Dislike -->
			<span>
				<Button
					class="message__button"
					:disabled="message.type === 'LoadingMessage'"
					size="small"
					text
					:icon="message.rating === false ? 'pi pi-thumbs-down-fill' : 'pi pi-thumbs-down'"
					:label="message.rating === false ? 'Message Disliked' : 'Dislike'"
					@click="message.rating === false ? (message.rating = null) : (message.rating = false)"
				/>
			</span>

			<template #footer>
				<Button class="message__button" label="Cancel" text @click="closeRatingModal" />
				<Button
					:style="{
						backgroundColor: $appConfigStore.primaryButtonBg,
						borderColor: $appConfigStore.primaryButtonBg,
						color: $appConfigStore.primaryButtonText,
					}"
					class="prompt-dialog__button"
					label="Submit"
					@click="handleRatingSubmit(message)"
				/>
			</template>
		</Dialog>

		<!-- Image Preview Modal -->
		<Dialog
			v-model:visible="isImagePreviewVisible"
			modal
			:style="{ width: '90vw', maxWidth: '1200px' }"
			:closable="true"
			:showHeader="false"
			:dismissableMask="true"
		>
			<div class="image-preview-container">
				<img :src="previewImageUrl" alt="Preview" class="preview-image" />
				<Button
					class="p-button-rounded p-button-text p-button-plain close-button"
					icon="pi pi-times"
					@click="isImagePreviewVisible = false"
					aria-label="Close"
				/>
			</div>
		</Dialog>
	</div>
</template>

<script lang="ts">
import hljs from 'highlight.js';
import 'highlight.js/styles/github-dark-dimmed.css';
import { marked } from 'marked';
import katex from 'katex';
import 'katex/dist/katex.min.css';
// import truncate from 'truncate-html';
import DOMPurify from 'dompurify';
import type { PropType } from 'vue';
import { hideAllPoppers } from 'floating-vue';
import Image from 'primevue/image';
import Dialog from 'primevue/dialog';

import type { Message, MessageContent, CompletionPrompt } from '@/js/types';
import api from '@/js/api';
import { fetchBlobUrl } from '@/js/fileService';

function processLatex(content) {
	const blockLatexPattern = /\\\[\s*([\s\S]+?)\s*\\\]/g;
	const inlineLatexPattern = /\\\(\s*([\s\S]+?)\s*\\\)/g;

	// Match triple & inline backticks
	const codeBlockPattern = /```[\s\S]+?```|`[^`]+`/g;

	const codeBlocks = [];

	// Extract and replace code blocks with placeholders temporarily
	// to ensure LaTeX within is not altered
	content = content.replace(codeBlockPattern, (match) => {
		codeBlocks.push(match);
		return `{{CODE_BLOCK_${codeBlocks.length - 1}}}`;
	});

	try {
		// Process block LaTeX: \[ ... \]
		content = content.replace(blockLatexPattern, (_, math) => {
			return `<div class="katex-block">${katex.renderToString(math, { displayMode: true, throwOnError: false, output: 'mathml' })}</div>`;
		});

		// Process inline LaTeX: \( ... \)
		content = content.replace(inlineLatexPattern, (_, math) => {
			return `<span class="katex-inline">${katex.renderToString(math, { throwOnError: false, output: 'mathml' })}</span>`;
		});
	} catch (error) {
		console.error('LaTeX rendering error:', error);
	}

	// Restore code blocks
	content = content.replace(/\{\{CODE_BLOCK_(\d+)\}\}/g, (_, index) => codeBlocks[Number(index)]);

	return content;
}

function trimToWordCount(str, count) {
	let wordCount = 0;
	let index = 0;

	while (wordCount < count && index < str.length) {
		if (str[index] === ' ' && str[index - 1] !== ' ' && index > 0) {
			wordCount++;
		}
		index++;
	}

	return str.substring(0, index).trim();
}

function getWordCount(str) {
	let wordCount = 0;
	let index = 0;

	str = str.trim();

	while (index < str.length) {
		if (str[index] === ' ' && str[index - 1] !== ' ' && index > 0) {
			wordCount++;
		}
		index++;
	}

	if (str.length > 0) {
		wordCount++;
	}

	return wordCount;
}

const MAX_WORD_SPEED_MS = 15;

export default {
	name: 'ChatMessage',
	components: {
		Image,
		Dialog,
	},

	props: {
		message: {
			type: Object as PropType<Message>,
			required: true,
		},
		showWordAnimation: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['rate', 'scroll-to-bottom'],

	data() {
		return {
			prompt: {} as CompletionPrompt,
			viewPrompt: false,
			currentWordIndex: 0,
			isAnalysisModalVisible: false,
			isRatingModalVisible: false,
			selectedContentArtifact: null,
			isMobile: window.screen.width < 950,
			markedRenderer: null,
			pollingInterval: null,
			pollingIteration: 0,
			totalTimeElapsed: 0,
			totalWordsGenerated: 0,
			averageTimePerWordMS: 50,
			processedContent: [],
			completed: false,
			isRenderingMessage: false,
			loading: true,
			error: false,
			isImagePreviewVisible: false,
			previewImageUrl: '',
		};
	},

	computed: {
		senderDisplayName() {
			let displayName = this.message.senderDisplayName;
			if (this.message.sender && this.message.sender !== 'User') {
				displayName = this.$appStore.mapAgentDisplayName(this.message.senderDisplayName);
			}
			return displayName;
		},

		messageContent() {
			if (this.message.status === 'Failed') {
				const failedMessage = this.message.text ?? 'Failed to generate a response.';
				return [
					{
						type: 'text',
						content: failedMessage,
						value: failedMessage,
						origValue: failedMessage,
					},
				];
			}

			return this.message.content ?? [];
		},

		messageDisplayStatus() {
			if (this.message.status === 'Failed' || this.message.status === 'Completed') return null;

			if (this.isRenderingMessage && this.messageContent.length > 0) return 'Responding';

			// Account for old messages that are complete (status of "Pending" with null operation_id)
			const isPending = this.message.status === 'Pending' && this.message.operation_id;
			if (
				this.showWordAnimation &&
				(isPending || this.message.status === 'InProgress' || this.message.status === 'Loading')
			)
				return 'Thinking';

			return null;
		},
	},

	watch: {
		message: {
			immediate: true,
			deep: true,
			handler(newMessage, oldMessage) {
				// There is an issue here if a message that is not the latest has an incomplete status
				if (newMessage.status === 'Failed' || newMessage.status === 'Completed') {
					this.computedAverageTimePerWord({ ...newMessage }, oldMessage ?? {});
					this.handleMessageCompleted(newMessage);
					return;
				}

				this.computedAverageTimePerWord({ ...newMessage }, oldMessage ?? {});
				if (
					!this.isRenderingMessage &&
					this.showWordAnimation &&
					newMessage.type !== 'LoadingMessage' &&
					newMessage.operation_id
				)
					this.startRenderingMessage();
			},
		},

		processedContent: {
			deep: true,
			handler() {
				this.markSkippableContent();

				// Bind click handlers to any new images
				this.$nextTick(() => {
					const messageImages = this.$el.querySelectorAll('.message-image');
					messageImages.forEach((img) => {
						// Remove any existing click handlers to prevent duplicates
						img.removeEventListener('click', this.handleImageClick);
						// Add the click handler
						img.addEventListener('click', this.handleImageClick);
					});
				});
			},
		},

		isRenderingMessage: {
			handler(newVal, oldVal) {
				if (newVal === oldVal) return;
				this.$appStore.sessionMessagePending = newVal;
				if (newVal) {
					this.keepScrollingUntilCompleted();
				} else if (this.message.status === 'Completed' || this.message.status === 'Failed') {
					// Ensure sessionMessagePending is set to false when message is completed
					this.$appStore.sessionMessagePending = false;
				}
			},
		},
	},

	created() {
		this.createMarkedRenderer();

		if (this.message.text && this.message.sender === 'User') {
			this.processedContent = [
				{
					type: 'text',
					value: this.message.text,
					origValue: this.message.text,
				},
			];
		} else if (this.message.content?.length > 0) {
			this.processedContent = this.message.content.map((content) => {
				this.currentWordIndex = getWordCount(content.value);
				return {
					type: content.type,
					content,
					value: this.processContentBlock(content.value),
					origValue: content.value,
					fileName: content?.fileName,
				};
			});
		} else if (this.message.text) {
			this.processedContent = this.messageContent;
		}
	},

	methods: {
		hideAllPoppers,

		getMainProperties(artifact) {
			if (!artifact) return {};
			
			const mainProperties = {};
			
			Object.keys(artifact).forEach(key => {
				if (key !== 'metadata') {
					mainProperties[key] = artifact[key];
				}
			});
			
			return mainProperties;
		},

		formatPropertyValue(value) {
			if (value === null || value === undefined || value === '') {
				return '-';
			}
			
			if (typeof value === 'string') {
				let formattedValue = value.replace(/\\n/g, '\n').replace(/\\t/g, '\t');
				formattedValue = formattedValue.replace(/\n*""\n*$/, '').trim();
				return formattedValue;
			}
			
			return value;
		},

		async handleCopyCellValue(value) {
			try {
				await navigator.clipboard.writeText(value);
				this.$appStore.addToast({
					severity: 'success',
					life: 3000,
					detail: 'Value copied to clipboard!',
				});
			} catch (error) {
				this.$appStore.addToast({
					severity: 'error',
					life: 3000,
					detail: 'Failed to copy value',
				});
			}
		},

		handleImageClick(event) {
			const img = event.currentTarget;
			this.previewImageUrl = img.getAttribute('data-src') || img.src;
			this.isImagePreviewVisible = true;
		},

		processContentBlock(contentToProcess) {
			let htmlContent = processLatex(contentToProcess ?? '');
			htmlContent = marked(htmlContent, { renderer: this.markedRenderer });
			htmlContent = DOMPurify.sanitize(htmlContent);

			return htmlContent;
		},

		computedAverageTimePerWord(newMessage, oldMessage) {
			const newContent = newMessage.content ?? [];
			const oldContent = oldMessage.content ?? [];

			this.pollingIteration += 1;

			// Calculate the number of words in the previous message content
			let amountOfOldWords = 0;
			if (oldContent) {
				amountOfOldWords = oldContent.reduce((acc, content) => {
					return acc + (content.value?.match(/[\w'-]+/g) || []).length;
				}, 0);
			}

			// Calculate the number of words in the new message content
			const amountOfNewWords = newContent.reduce((acc, content) => {
				return acc + (content.value?.match(/[\w'-]+/g) || []).length;
			}, 0);

			// Calculate the number of new words generated since the last request
			const newWordsGenerated = amountOfNewWords - amountOfOldWords;

			if (newWordsGenerated > 0) {
				this.totalWordsGenerated += newWordsGenerated;
				this.totalTimeElapsed += this.$appStore.getPollingRateMS();
			}

			// Calculate the average time per word
			if (this.pollingIteration === 1) {
				this.averageTimePerWordMS = MAX_WORD_SPEED_MS;
			} else {
				this.averageTimePerWordMS =
					this.totalWordsGenerated > 0
						? Number((this.totalTimeElapsed / this.totalWordsGenerated).toFixed(2))
						: 0;
			}
		},

		handleMessageCompleted() {
			this.averageTimePerWordMS = MAX_WORD_SPEED_MS;
			this.completed = true;
		},

		startRenderingMessage() {
			if (this.isRenderingMessage) return;

			this.displayWordByWord();
		},

		displayWordByWord() {
			this.isRenderingMessage = true;

			let currentContentIndex = Math.max(this.processedContent.length - 1, 0);
			let content = this.messageContent[currentContentIndex]?.value;
			let processedContent = this.processedContent[currentContentIndex]?.content;

			// If the processed content block is the same as the current content block,
			// and there is a new one, then we know to move to the next block
			// check content !== for files blocks that are still null, but the next block is already generated
			if (
				this.messageContent[currentContentIndex + 1] &&
				content === processedContent &&
				!!content
			) {
				currentContentIndex += 1;
				this.currentWordIndex = 0;

				content = this.messageContent[currentContentIndex]?.value;
				processedContent = this.processedContent[currentContentIndex]?.content;
			}

			if (content !== processedContent) {
				this.currentWordIndex += 1;

				if (this.messageContent[currentContentIndex]?.type === 'text') {
					const truncatedContent = trimToWordCount(content, this.currentWordIndex);

					this.processedContent[currentContentIndex] = {
						type: this.messageContent[currentContentIndex]?.type,
						content: truncatedContent,
						value: this.processContentBlock(truncatedContent),
						origValue: this.messageContent[currentContentIndex]?.value,
					};
				} else {
					this.processedContent[currentContentIndex] = {
						type: this.messageContent[currentContentIndex]?.type,
						content,
						value: content,
						origValue: this.messageContent[currentContentIndex]?.value,
						fileName: this.messageContent[currentContentIndex]?.fileName,
					};
				}
			}

			// If the current block is still rendering, or there is another block, or the message is still processing
			if (
				(content !== processedContent && !!content) ||
				this.messageContent[currentContentIndex + 1] ||
				!this.completed
			) {
				return setTimeout(() => this.displayWordByWord(), this.averageTimePerWordMS);
			}

			// Trigger after polling rate from message completion to ensure message is always rendered on completion:
			// Just to make sure the message renders fully in the case the animation fails
			// this.processedContent = this.messageContent.map((content) => {
			// 	return {
			// 		type: content.type,
			// 		content,
			// 		value: content.type === 'text' ? this.processContentBlock(content.value) : content.value,
			// 		origValue: content.value,
			// 	};
			// });

			this.isRenderingMessage = false;
		},

		createMarkedRenderer() {
			this.markedRenderer = new marked.Renderer();

			// Code blocks
			this.markedRenderer.code = (code, infostring = '', escaped = true) => {
				let language = '';
				let sourceCode = '';

				// Handle both string and object input for code
				if (typeof code === 'object' && code !== null) {
					language = code.lang || '';
					sourceCode = code.text || '';
				} else {
					sourceCode = code;
					language = infostring || '';
				}

				const validLanguage = !!(language && hljs.getLanguage(language));
				const highlighted = validLanguage
					? hljs.highlight(sourceCode, { language })
					: hljs.highlightAuto(sourceCode);
				const languageClass = validLanguage ? `hljs language-${language}` : 'hljs';
				const encodedCode = encodeURIComponent(sourceCode);

				return `<pre><code class="${languageClass}" data-code="${encodedCode}" data-language="${highlighted.language}">${highlighted.value}</code></pre>`;
			};

			// Images
			this.markedRenderer.image = function ({ href, title, text }) {
				// Find a matching image_file content block in the message's content
				const matchingImageBlock = this.message.content?.find(
					(block) => block.type === 'image_file' && block.origValue === href,
				);

				// If we found a matching block and it has a blobUrl, use that
				if (matchingImageBlock?.blobUrl) {
					return `<img src="${matchingImageBlock.blobUrl}" alt="${text || ''}" title="${title || ''}" class="message-image" data-src="${matchingImageBlock.blobUrl}" />`;
				}

				// Create a unique ID for this image
				const imageId = `image-${Math.random().toString(36).substr(2, 9)}`;

				// For non-API URLs, use the href directly in the placeholder
				if (!href.startsWith(api.getApiUrl())) {
					return `<img id="${imageId}" src="${href}" alt="${text || ''}" title="${title || ''}" class="message-image" data-src="${href}" />`;
				}

				// For API URLs, use the placeholder and fetch the image
				const placeholder = `<img id="${imageId}" src="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7" alt="${text || ''}" title="${title || ''}" class="message-image" />`;

				// Only fetch the image if we haven't already started fetching it
				if (
					!this.message.content?.some(
						(block) => block.type === 'image_file' && block.origValue === href && block.isLoading,
					)
				) {
					// Mark the block as loading
					if (matchingImageBlock) {
						matchingImageBlock.isLoading = true;
					}

					// Load the image and create a blob URL
					api
						.fetchDirect(href)
						.then((response) => {
							const blobUrl = URL.createObjectURL(response);
							// Update the matching block with the blob URL
							if (matchingImageBlock) {
								matchingImageBlock.blobUrl = blobUrl;
								matchingImageBlock.isLoading = false;
							}
							// Update the image in the DOM
							const container = document.getElementById(imageId);
							if (container) {
								container.setAttribute('src', blobUrl);
								container.setAttribute('data-src', blobUrl);
							}
						})
						.catch((error) => {
							console.error(`Failed to fetch image from ${href}`, error);
							if (matchingImageBlock) {
								matchingImageBlock.isLoading = false;
							}
						});
				}

				return placeholder;
			}.bind(this);

			// Links
			this.markedRenderer.link = ({ href, title, text }) => {
				// Check if the link is a file download type.
				const isFileDownload = href.includes('/files/FoundationaLLM');

				if (isFileDownload) {
					const matchingFileBlock = this.messageContent.find(
						(block) => block.type === 'file_path' && block.value === href,
					);

					// Append file icon if there's a matching file_path
					const fileName = matchingFileBlock?.fileName.split('/').pop() ?? '';
					const fileIcon = matchingFileBlock
						? `<i class="${this.$getFileIconClass(fileName, true)} attachment-icon"></i>`
						: `<i class="pi pi-file" class="attachment-icon"></i>`;
					return `${fileIcon}<a href="#" data-href="${href}" data-filename="${fileName}" title="${title || ''}" class="file-download-link">${text}</a>`;
				} else {
					// Render as inline anchor; if the label itself may contain markdown, parse only inline
					const label = (marked.parseInline && marked.parseInline(text)) || text;
					return `<a href="${href}" title="${title || ''}" target="_blank">${label}</a>`;
				}
			};
		},

		markSkippableContent() {
			if (!this.processedContent) return;

			// Normalize URLs to improve matching
			const normalize = (u = '') =>
				u.replace(/^\/+/, '').replace(/[.,);]+$/, '');

			this.processedContent.forEach((contentBlock) => {
				if (contentBlock.type === 'file_path') {
					const target = normalize(contentBlock.origValue);
					const matchingTextContent = this.processedContent.find(
						(block) =>
							block.type === 'text' &&
							normalize(block.origValue).includes(target),
					);

					if (matchingTextContent) {
						matchingTextContent.fileName = contentBlock.fileName;
						contentBlock.skip = true;
					}
				}
			});
		},

		formatTimeStamp(timeStamp: string) {
			const date = new Date(timeStamp);
			const options = {
				year: 'numeric',
				month: 'long',
				day: 'numeric',
				hour: 'numeric',
				minute: 'numeric',
				second: 'numeric',
				timeZoneName: 'short',
			};
			return date.toLocaleString(undefined, options);
		},

		buildTimeStampTooltip(timeStamp: string, processingTime: number) {
			const date = this.formatTimeStamp(timeStamp);
			if (!processingTime) return date;
			const processingTimeSeconds = processingTime / 1000;
			return `${date}\n(${processingTimeSeconds.toFixed(2)} seconds)`;
		},

		async handleCopyMessageContent() {
			let markdownContent = '';
			let htmlContent = '';
			
			if (this.messageContent && this.messageContent?.length > 0) {
				this.messageContent.forEach((contentBlock) => {
					switch (contentBlock.type) {
						case 'text':
							markdownContent += contentBlock.value;
							break;
						// default:
						// 	markdownContent += `![${contentBlock.fileName || 'image'}](${contentBlock.value})`;
						// 	break;
					}
				});
			} else {
				markdownContent = this.message.text;
			}

			// Process markdown to HTML using the same method as display
			htmlContent = this.processContentBlock(markdownContent);

			try {
				// Use modern clipboard API to copy both formats
				await navigator.clipboard.write([
					new ClipboardItem({
						"text/html": new Blob([htmlContent], { type: "text/html" }),
						"text/plain": new Blob([markdownContent], { type: "text/plain" })
					})
				]);

				this.$appStore.addToast({
					severity: 'success',
					life: 5000,
					detail: 'Message copied to clipboard with formatting!',
				});
			} catch (error) {
				// Fallback to old method if modern clipboard API fails
				console.warn('Modern clipboard API failed, falling back to legacy method:', error);
				
				const textarea = document.createElement('textarea');
				textarea.value = markdownContent;
				document.body.appendChild(textarea);
				textarea.select();
				document.execCommand('copy');
				document.body.removeChild(textarea);

				this.$appStore.addToast({
					severity: 'success',
					life: 5000,
					detail: 'Message copied to clipboard!',
				});
			}
		},

		handleRate(message: Message, isLiked: boolean) {
			this.$emit('rate', { message, isLiked: message.rating === isLiked ? null : isLiked });
		},

		handleRatingSubmit(message: Message) {
			this.$emit('rate', { message });
			this.isRatingModalVisible = false;
			this.$appStore.addToast({
				severity: 'success',
				life: 5000,
				detail: 'Rating submitted!',
			});
		},

		closeRatingModal() {
			this.isRatingModalVisible = false;
		},

		async handleViewPrompt() {
			const prompt = await api.getPrompt(this.message.sessionId, this.message.completionPromptId);
			this.prompt = prompt;
			this.viewPrompt = true;
		},

		handleFileLinkInText(event: MouseEvent) {
			const link = (event.target as HTMLElement).closest('a.file-download-link');
			if (link && link.dataset.href) {
				event.preventDefault();

				const content: MessageContent = {
					type: 'file_path',
					value: link.dataset.href,
					origValue: link.dataset.href,
					fileName: link.dataset.filename || link.textContent,
				};

				fetchBlobUrl(content);
			}
		},

		keepScrollingUntilCompleted() {
			if (!this.isRenderingMessage) return;

			this.$nextTick(() => {
				const previousScrollHeight = this.$parent.$refs.messageContainer?.scrollHeight || 0;

				setTimeout(() => {
					const newScrollHeight = this.$parent.$refs.messageContainer?.scrollHeight || 0;
					const contentGrowth = newScrollHeight - previousScrollHeight;

					if (contentGrowth > 0) {
						this.$emit('scroll-to-bottom', contentGrowth);
					}
					this.keepScrollingUntilCompleted();
				}, 100);
			});
		},
	},
	mounted() {
		// Initial binding of click handlers
		this.$nextTick(() => {
			const messageImages = this.$el.querySelectorAll('.message-image');
			messageImages.forEach((img) => {
				img.addEventListener('click', this.handleImageClick);
			});
		});
	},
};
</script>
<style lang="scss">
.attachment-icon {
	width: 24px;
	margin-right: 6px;
	vertical-align: middle;
	line-height: 1;
}
</style>

<style lang="scss" scoped>
@keyframes loading-shimmer {
	0% {
		background-position: -100% top;
	}

	to {
		background-position: 250% top;
	}
}

$shimmerColor: white;
// $textColor: var(--accent-text);
$textColor: #131833;
.loading-shimmer {
	text-fill-color: transparent;
	-webkit-text-fill-color: transparent;
	animation-delay: 0.3s;
	animation-duration: 3s;
	animation-iteration-count: infinite;
	animation-name: loading-shimmer;
	background: $textColor
		gradient(linear, 100% 0, 0 0, from($textColor), color-stop(0.5, $shimmerColor), to($textColor));
	background: $textColor -webkit-gradient(
			linear,
			100% 0,
			0 0,
			from($textColor),
			color-stop(0.5, $shimmerColor),
			to($textColor)
		);
	background-clip: text;
	-webkit-background-clip: text;
	background-repeat: no-repeat;
	background-size: 50% 200%;
	display: flex;
	align-items: center;
}

[dir='ltr'] .loading-shimmer {
	background-position: -100% top;
}

[dir='rtl'] .loading-shimmer {
	background-position: 200% top;
}

.message-row {
	display: flex;
	align-items: flex-end;
	margin-top: 8px;
	margin-bottom: 8px;
}

.message {
	padding: 12px;
	width: 80%;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
}

.date-separator {
	display: none;
}

.message--in {
	.message {
		background-color: rgba(250, 250, 250, 1);
	}
}

.message--out {
	flex-direction: row-reverse;
	.message {
		background-color: var(--primary-color);
		color: var(--primary-text);
	}
}

.message__header {
	margin-bottom: 12px;
	display: flex;
	justify-content: space-between;
	padding-left: 12px;
	padding-right: 12px;
	padding-top: 8px;
}

.message__header--right {
	display: flex;
	align-items: center;
	flex-shrink: 0;
}

.message__body {
	// white-space: pre-wrap;
	overflow-wrap: break-word;
	overflow-x: auto;
	padding-left: 12px;
	padding-right: 12px;
	padding-top: 8px;
	padding-bottom: 8px;
}

.message__footer {
	margin-top: 8px;
	display: flex;
	justify-content: space-between;
	flex-wrap: wrap;
}

.message__copy {
	color: var(--primary-text) !important;
	margin-left: 4px;
}

.message__copy:focus {
	box-shadow: 0 0 0 0.1rem #fff;
}

.header__sender {
	display: flex;
	align-items: center;
}

.avatar {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	margin-right: 12px;
}

.token-chip {
	border-radius: 24px;
	margin-right: 12px;
}

.token-chip--out {
	background-color: var(--accent-color);
}

.token-chip--in {
	background-color: var(--primary-color);
}

.content-artifacts {
	flex-basis: 100%;
	padding: 8px 12px;
	display: flex;
	flex-wrap: wrap;
	align-items: center;
}

.content-artifact {
	background-color: var(--primary-color);
	color: var(--primary-text);
	margin: 4px;
	padding: 4px 8px;
	cursor: pointer;
	white-space: nowrap;
	max-width: 25rem;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}

.additional-content {
	width: 100%;
	padding: 8px 12px;
}

.ratings {
	display: flex;
	// gap: 16px;
}

.icon {
	margin-right: 4px;
	cursor: pointer;
}

.prompt-text {
	white-space: pre-wrap;
	overflow-wrap: break-word;
}

@media only screen and (max-width: 950px) {
	.message {
		width: 95%;
	}
}

.message__button {
	color: var(--primary-button-bg);
}

.message__button:focus {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
}

.prompt-dialog__button:focus {
	box-shadow: 0 0 0 0.1rem #000;
}

.message__body img {
	max-width: 55% !important;
	height: auto !important;
	display: block !important;
	cursor: pointer;
	transition: opacity 0.2s ease-in-out;

	&:hover {
		opacity: 0.9;
	}
}

.image-preview-container {
	display: flex;
	justify-content: center;
	align-items: center;
	padding: 1rem;
	background-color: var(--surface-ground);
	border-radius: 0.5rem;
	position: relative;

	.close-button {
		position: fixed;
		top: 1rem;
		right: 1rem;
		z-index: 1000;
		background-color: rgba(0, 0, 0, 0.5);
		color: white !important;
		width: 2.5rem;
		height: 2.5rem;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 50%;
		transition: background-color 0.2s ease-in-out;

		&:hover {
			background-color: rgba(0, 0, 0, 0.7);
		}

		&:focus {
			box-shadow: 0 0 0 0.2rem rgba(255, 255, 255, 0.5);
		}
	}
}

.preview-image {
	max-width: 100%;
	height: auto;
	border-radius: 0.5rem;
}
</style>

<style lang="scss">
.p-chip .p-chip-text {
	line-height: 1.1;
	font-size: 0.75rem;
}
.prompt-dialog {
	width: 50vw;
}

.message__body img {
	max-width: 55% !important;
	height: auto !important;
	display: block !important;
}

.message-image {
	cursor: pointer;
	transition: opacity 0.2s ease-in-out;

	&:hover {
		opacity: 0.9;
	}
}

@media only screen and (max-width: 950px) {
	.prompt-dialog {
		width: 90vw;
	}
}

@media only screen and (max-width: 545px) {
	.date-separator {
		display: flex !important;
	}
	.time-stamp {
		display: none;
	}
	.token-chip {
		margin-right: 0px !important;
	}
	.message__button .p-button-label {
		display: none;
	}
	.message__button .p-button-icon {
		margin-right: 0px;
	}
}

/* Artifact table cell copy button styles */
.artifact-value-cell {
	position: relative;
}

.artifact-value-content {
	max-height: 250px;
	max-width: 100%;
	overflow-y: auto;
	overflow-x: auto;
	padding: 12px;
	white-space: pre-wrap;
	word-wrap: break-word;
	box-sizing: border-box;
}

.artifact-value-content::-webkit-scrollbar {
	width: 7px;
	height: 7px;
}
.artifact-value-content::-webkit-scrollbar-track {
	background: #f1f1f1;
	border-radius: 4px;
}
.artifact-value-content::-webkit-scrollbar-thumb {
	background: #888;
	border-radius: 4px;
}
.artifact-value-content::-webkit-scrollbar-thumb:hover {
	background: #555;
}

.artifact-value-cell .copy-cell-button {
	position: absolute;
	top: 8px;
	right: 8px;
	opacity: 0;
	transition: opacity 0.2s ease-in-out;
	background-color: rgba(255, 255, 255, 0.9) !important;
	border: 1px solid #ddd !important;
	padding: 4px 8px !important;
	min-width: auto !important;
	color: #666 !important;
	z-index: 10;
}

.artifact-table-row:hover .copy-cell-button {
	opacity: 1;
}

.artifact-value-cell .copy-cell-button:hover {
	background-color: #f0f0f0 !important;
	color: #333 !important;
}

.artifact-value-cell .copy-cell-button:focus {
	box-shadow: 0 0 0 0.1rem var(--primary-button-bg);
	opacity: 1;
}
</style>
