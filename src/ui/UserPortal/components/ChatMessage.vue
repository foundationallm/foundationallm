<template>
	<div>
		<div class="message-row" :class="message.sender === 'User' ? 'message--out' : 'message--in'">
			<div class="message">
				<div class="message__header">
					<!-- Sender -->
					<span class="header__sender">
						<AgentIcon
							v-if="message.sender !== 'User'"
							:src="$appConfigStore.agentIconUrl || '~/assets/FLLM-Agent-Light.svg'"
							alt="Agent avatar"
						/>
						<span>{{ getDisplayName() }}</span>
					</span>

					<!-- Tokens & Timestamp -->
					<span class="message__header--right">
						<Chip
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
						<span v-tooltip="formatTimeStamp(message.timeStamp)" class="time-stamp">{{
							$filters.timeAgo(new Date(message.timeStamp))
						}}</span>
					</span>
				</div>

				<!-- Message text -->
				<div class="message__body">
					<AttachmentList
						v-if="message.sender === 'User'"
						:attachments="message.attachmentDetails ?? []"
						:attachmentIds="message.attachments"
					/>

					<template v-if="message.sender === 'Assistant' && message.type === 'LoadingMessage'">
						<i class="pi pi-spin pi-spinner"></i>
					</template>

					<template v-if="!messageContent || messageContent.length === 0">
						<div v-html="compiledVueTemplate"></div>
					</template>

					<template v-else>
						<!-- Render the html content and any vue components within -->
						<div v-for="content in messageContent" :key="content.fileName" class="message-content">
							<div v-if="content.type === 'text'">
								<component :is="renderMarkdownComponent(content.value)"></component>
							</div>

							<div v-else-if="content.type === 'image_file'">
								<template v-if="content.loading || (!content.error && !content.blobUrl)">
									<div class="loading-content-container">
										<i class="pi pi-image loading-content-icon" style="font-size: 2rem"></i>
										<i
											class="pi pi-spin pi-spinner loading-content-icon"
											style="font-size: 1rem"
										></i>
										<span class="loading-content-text">Loading image...</span>
									</div>
								</template>
								<Image
									v-if="content.blobUrl"
									:src="content.blobUrl"
									:alt="content.fileName"
									@load="content.loading = false"
									@error="
										content.loading = false;
										content.error = true;
									"
									width="45%"
									preview
								/>
								<div v-if="content.error" class="loading-content-error">
									<i
										class="pi pi-times-circle loading-content-error-icon"
										style="font-size: 2rem"
									></i>
									<span class="loading-content-error-text">Could not load image</span>
								</div>
							</div>

							<div v-else-if="content.type === 'html'">
								<template v-if="content.loading || (!content.error && !content.blobUrl)">
									<div class="loading-content-container">
										<i class="pi pi-chart-line loading-content-icon" style="font-size: 2rem"></i>
										<i
											class="pi pi-spin pi-spinner loading-content-icon"
											style="font-size: 1rem"
										></i>
										<span class="loading-content-text">Loading visualization...</span>
									</div>
								</template>
								<iframe v-if="content.blobUrl" :src="content.blobUrl" frameborder="0"> </iframe>
							</div>

							<div v-else-if="content.type === 'file_path'">
								Download <i :class="$getFileIconClass(content.fileName, true)" class="attachment-icon"></i> 
								<a
									:href="content.blobUrl"
									:download="content.fileName ?? content.blobUrl ?? content.value"
									target="_blank"
								>
									{{ content.fileName ?? content.blobUrl }}
								</a>
							</div>
						</div>

						<Button
							v-if="message.analysisResults && message.analysisResults.length > 0"
							class="message__button"
							:disabled="message.type === 'LoadingMessage'"
							size="small"
							text
							icon="pi pi-window-maximize"
							label="Analysis"
							@click.stop="showAnalysisModal"
						/>
					</template>
				</div>

				<div v-if="message.sender !== 'User'" class="message__footer">
					<div v-if="message.citations?.length" class="citations">
						<span><b>Citations: </b></span>
						<span
							v-for="citation in message.citations"
							:key="citation.id"
							v-tooltip.top="{ value: citation.filepath, showDelay: 500, hideDelay: 300 }"
							class="citation"
						>
							<i class="pi pi-file"></i>
							{{ citation.title.split('/').pop() }}
						</span>
					</div>
					<span class="ratings">
						<!-- Like -->
						<span>
							<Button
								class="message__button"
								:disabled="message.type === 'LoadingMessage'"
								size="small"
								text
								:icon="message.rating ? 'pi pi-thumbs-up-fill' : 'pi pi-thumbs-up'"
								:label="message.rating ? 'Message Liked!' : 'Like'"
								@click.stop="handleRate(message, true)"
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
								:label="message.rating === false ? 'Message Disliked.' : 'Dislike'"
								@click.stop="handleRate(message, false)"
							/>
						</span>
					</span>

					<!-- View prompt -->
					<span class="view-prompt">
						<Button
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
							class="prompt-dialog"
							:visible="viewPrompt"
							modal
							header="Completion Prompt"
							:closable="false"
						>
							<p class="prompt-text">{{ prompt.prompt }}</p>
							<template #footer>
								<Button
									:style="{
										backgroundColor: primaryButtonBg,
										borderColor: primaryButtonBg,
										color: primaryButtonText,
									}"
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
		<Divider v-if="message.sender == 'User'" align="center" type="solid" class="date-separator">
			{{ $filters.timeAgo(new Date(message.timeStamp)) }}
		</Divider>

		<AnalysisModal
			:visible="isAnalysisModalVisible"
			:analysisResults="message.analysisResults ?? []"
			@update:visible="isAnalysisModalVisible = $event"
		/>
	</div>
</template>

<script lang="ts">
import hljs from 'highlight.js';
import 'highlight.js/styles/github-dark-dimmed.css';
import { marked } from 'marked';
import katex from 'katex';
import truncate from 'truncate-html';
import DOMPurify from 'dompurify';
import type { PropType, ref } from 'vue';

import type { Message, CompletionPrompt } from '@/js/types';
import api from '@/js/api';
import CodeBlockHeader from '@/components/CodeBlockHeader.vue';
import AttachmentList from '@/components/AttachmentList.vue';
import AnalysisModal from '@/components/AnalysisModal.vue';
import AgentIcon from '@/components/AgentIcon.vue';

const renderer = new marked.Renderer();

renderer.code = (code, language) => {
	const sourceCode = code.raw || code;
	const validLanguage = !!(language && hljs.getLanguage(language));
	const highlighted = validLanguage
		? hljs.highlight(sourceCode, { language })
		: hljs.highlightAuto(sourceCode);
	const languageClass = validLanguage ? `hljs language-${language}` : 'hljs';
	const encodedCode = encodeURIComponent(sourceCode);
	return `<pre><code class="${languageClass}" data-code="${encodedCode}" data-language="${highlighted.language}">${highlighted.value}</code></pre>`;
};
function processLatex(html) {
	const blockLatexPattern = /\\\[([^\]]+)\\\]/g;
	const inlineLatexPattern = /\\\(([^\)]+)\\\)/g;

	// Check if LaTeX syntax is detected in the content
	const hasBlockLatex = blockLatexPattern.test(html);
	const hasInlineLatex = inlineLatexPattern.test(html);

	// Process block LaTeX: \[ ... \]
	html = html.replace(blockLatexPattern, (_, math) => {
		return katex.renderToString(math, { displayMode: true, throwOnError: false });
	});

	// Process inline LaTeX: \( ... \)
	html = html.replace(inlineLatexPattern, (_, math) => {
		return katex.renderToString(math, { throwOnError: false });
	});

	// If LaTeX was found, render the content again with marked
	if (hasBlockLatex || hasInlineLatex) {
		html = marked(html);
	}

	return html;
}

marked.use({ renderer });

function addCodeHeaderComponents(htmlString) {
	const parser = new DOMParser();
	const doc = parser.parseFromString(htmlString, 'text/html');

	doc.querySelectorAll('pre code').forEach((element) => {
		const languageClass = element.getAttribute('class');
		const encodedCode = element.getAttribute('data-code');
		// const autoDetectLanguage = element.getAttribute('data-language');
		const languageMatch = languageClass.match(/language-(\w+)/);
		const language = languageMatch ? languageMatch[1] : 'plaintext';

		const header = document.createElement('div');
		header.innerHTML = `<code-block-header language="${language}" codecontent="${encodedCode}"></code-block-header>`;

		element.parentNode.insertBefore(header.firstChild, element);
	});

	const html = doc.body.innerHTML;
	const withVueCurlyBracesSanitized = html
		.replace(/{{/g, '&#123;&#123;')
		.replace(/}}/g, '&#125;&#125;');

	return withVueCurlyBracesSanitized;
}

export default {
	name: 'ChatMessage',

	components: {
		AttachmentList,
		AnalysisModal,
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

	setup() {
		const isAnalysisModalVisible = ref(false);

		function showAnalysisModal() {
			isAnalysisModalVisible.value = true;
		}

		return {
			isAnalysisModalVisible,
			showAnalysisModal,
		};
	},

	emits: ['rate', 'refresh'],

	data() {
		return {
			prompt: {} as CompletionPrompt,
			viewPrompt: false,
			compiledVueTemplate: '',
			currentWordIndex: 0,
			primaryButtonBg: this.$appConfigStore.primaryButtonBg,
			primaryButtonText: this.$appConfigStore.primaryButtonText,
			messageContent: this.message.content
				? JSON.parse(JSON.stringify(this.message.content))
				: null,
		};
	},

	computed: {
		compiledMarkdown() {
			let htmlContent = processLatex(this.message.text ?? '');
			htmlContent = marked(htmlContent);
			return DOMPurify.sanitize(htmlContent);
		},

		compiledMarkdownComponent() {
			return {
				template: `<div>${this.compiledVueTemplate}</div>`,
				components: {
					CodeBlockHeader,
				},
			};
		},
	},

	created() {
		if (this.showWordAnimation) {
			this.displayWordByWord();
		} else {
			this.compiledVueTemplate = addCodeHeaderComponents(this.compiledMarkdown);
		}
	},

	mounted() {
		this.fetchContentFiles();
	},

	methods: {
		renderMarkdownComponent(contentValue: string) {
			let htmlContent = processLatex(contentValue ?? '');
			htmlContent = DOMPurify.sanitize(marked(htmlContent));
			return {
				template: `<div>${htmlContent}</div>`,
				components: {
					CodeBlockHeader,
				},
			};
		},

		displayWordByWord() {
			const words = this.compiledMarkdown.split(/\s+/);
			if (this.currentWordIndex >= words.length) {
				this.compiledVueTemplate = addCodeHeaderComponents(this.compiledMarkdown);
				return;
			}

			this.currentWordIndex += 1;

			const htmlString = truncate(this.compiledMarkdown, this.currentWordIndex, {
				byWords: true,
				stripTags: false,
				ellipsis: '',
				decodeEntities: false,
				excludes: '',
				reserveLastWord: false,
				keepWhitespaces: true,
			});

			this.compiledVueTemplate = addCodeHeaderComponents(htmlString);

			setTimeout(() => this.displayWordByWord(), 10);
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

		getDisplayName() {
			return this.message.sender === 'User'
				? this.message.senderDisplayName
				: `${this.message.sender} - ${this.message.senderDisplayName}`;
		},

		handleRate(message: Message, isLiked: boolean) {
			this.$emit('rate', { message, isLiked: message.rating === isLiked ? null : isLiked });
		},

		async handleViewPrompt() {
			const prompt = await api.getPrompt(this.message.sessionId, this.message.completionPromptId);
			this.prompt = prompt;
			this.viewPrompt = true;
		},

		// Add this method to fetch content files securely
		async fetchContentFiles() {
			if (!this.messageContent || this.messageContent.length === 0) return;

			const fetchPromises = this.messageContent.map(async (content) => {
				if (['image_file', 'html', 'file_path'].includes(content.type)) {
					content.loading = true;
					content.error = false;
					try {
						const response = await api.fetchDirect(content.value);
						if (content.type === 'html') {
							const blob = new Blob([response], { type: 'text/html' });
							content.blobUrl = URL.createObjectURL(blob);
						} else {
							content.blobUrl = URL.createObjectURL(response);
						}
						content.fileName = content.fileName?.split('/').pop();
					} catch (error) {
						console.error(`Failed to fetch content from ${content.value}`, error);
						content.error = true;
					}
					content.loading = false;
				}
			});

			await Promise.all(fetchPromises);
		},
	},
};
</script>

<style lang="scss" scoped>
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

.citations {
	flex-basis: 100%;
	padding: 8px 12px;
	display: flex;
	flex-wrap: wrap;
	align-items: center;
}

.citation {
	background-color: var(--primary-color);
	color: var(--primary-text);
	margin: 4px;
	padding: 4px 8px;
	cursor: pointer;
	white-space: nowrap;
}

.ratings {
	display: flex;
	gap: 16px;
}

.icon {
	margin-right: 4px;
	cursor: pointer;
}

.view-prompt {
	cursor: pointer;
}

.dislike {
	margin-left: 12px;
	cursor: pointer;
}

.like {
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

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}

.message-content {
	margin-top: 5px;
	margin-bottom: 5px;
}

img {
	max-width: 100%;
	height: auto;
	border-radius: 8px;
}

iframe {
	width: 100%;
	height: 600px;
	border-radius: 8px;
}

.loading-content-container {
	display: flex;
	align-items: center;

	.loading-content-icon {
		margin-right: 8px;
		vertical-align: middle;
		line-height: 1;
	}

	.loading-content-text {
		font-size: 0.75rem;
		font-style: italic;
		line-height: 1.5;
	}
}

.loading-content-error {
	display: flex;
	align-items: center;
	width: 200px;
	padding: 8px 12px;
	border-radius: 0.75rem;
	border-color: rgb(182, 2, 2);
	color: rgb(182, 2, 2);
	box-shadow: 0 1px 3px rgba(182, 2, 2, 0.664);

	.loading-content-error-icon {
		margin-right: 8px;
		vertical-align: middle;
		line-height: 1;
	}

	.loading-content-error-text {
		font-size: 0.85rem;
		font-style: italic;
		line-height: 1.5;
	}
}

.attachment-icon {
        width: 24px;
        margin-right: 4px;
        vertical-align: middle;
        line-height: 1;
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
</style>
