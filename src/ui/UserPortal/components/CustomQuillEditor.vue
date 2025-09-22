<template>
	<div ref="quillContainer" class="quill-container">
		<QuillEditor
			ref="quillEditor"
			class="custom-quill-editor"
			:content="content"
			:toolbar="`#${toolbarId}`"
			content-type="html"
			:options="quillOptions"
			@keydown.enter="handleEnterFromQuill"
			@update:content="handleContentUpdate"
		>
			<template #toolbar>
				<div :id="toolbarId">
					<select class="ql-header" aria-label="Heading" title="Heading">
						<option value="" selected>Normal</option>
						<option value="2">Heading 2</option>
						<option value="3">Heading 3</option>
						<option value="4">Heading 4</option>
						<option value="5">Heading 5</option>
						<option value="6">Heading 6</option>
					</select>
					<button class="ql-bold" aria-label="Bold" title="Bold"></button>
					<button class="ql-italic" aria-label="Italic" title="Italic"></button>
					<button class="ql-underline" aria-label="Underline" title="Underline"></button>
					<button class="ql-strike" aria-label="Strike" title="Strike"></button>
					<button class="ql-link" aria-label="Link" title="Link"></button>
					<button class="ql-image" aria-label="Image" title="Image"></button>
					<button
						class="ql-list"
						value="ordered"
						aria-label="Ordered List"
						title="Ordered List"
					></button>
					<button
						class="ql-list"
						value="bullet"
						aria-label="Unordered List"
						title="Unordered List"
					></button>
					<button class="ql-blockquote" aria-label="Quote" title="Quote"></button>
					<button class="ql-clean" aria-label="Remove Styles" title="Remove Styles"></button>
					<button
						:class="`quill-view-html view-html-${randomNumber}`"
						aria-label="Edit HTML"
						style="width: 100px"
						@click="toggleHtmlDialog"
						@keydown.enter="toggleHtmlDialog"
					>
						Edit HTML
					</button>
				</div>
			</template>
		</QuillEditor>

		<!-- Raw HTML Dialog -->
		<Dialog
			v-model:visible="showHtmlDialog"
			:modal="true"
			:closable="false"
			:style="{ width: '90vw' }"
			header="Edit Footer HTML"
		>
			<CodeEditor
				ref="codeEditor"
				v-model="rawHtml"
				:languages="[['html', 'HTML']]"
				:wrap="true"
				autofocus
				theme="github-dark"
				style="width: 100%; height: 100%"
				@update:model-value="handleCodeEditorChange"
			/>

			<template #footer>
				<Button label="Save" @click="handleSaveRawHTML" />
				<Button label="Cancel" @click="toggleHtmlDialog" />
			</template>
		</Dialog>

		<!-- HTML Correction Dialog -->
		<Dialog
			v-model:visible="showHtmlCorrectionDialog"
			:modal="true"
			:closable="false"
			:style="{ width: '90vw' }"
			header="HTML Content Optimization"
		>
			<div style="margin-bottom: 16px;">
				<h4 style="margin-top: 0px; margin-bottom: 8px; color: #374151;">
					Your HTML content has been optimized for better formatting:
				</h4>
				<p style="margin: 8px 0; color: #6b7280; font-size: 0.9rem;">
					The editor has cleaned up unnecessary whitespace and formatting. Please review the changes below and save if they look correct.
				</p>
			</div>

			<Diff
				mode="split"
				theme="light"
				language="html"
				:prev="rawHtml"
				:current="processedContent"
			/>

			<template #footer>
				<Button label="Save Optimized Content" @click="handleSaveCorrectedHTML" class="p-button-success" />
				<Button label="Back to Edit" @click="handleHtmlCorrectionBack" class="p-button-secondary" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';
import { Diff } from 'vue-diff';
import 'vue-diff/dist/index.css';
import 'highlight.js';
// @ts-ignore
import CodeEditor from 'simple-code-editor';
// @ts-ignore
import { v4 as uuidv4 } from 'uuid';

function getFocusableElements(container: HTMLElement): HTMLElement[] {
	return Array.from(
		container.querySelectorAll(
			'a, button, input, textarea, select, details, [tabindex]:not([tabindex="-1"])',
		),
	).filter((el: Element) => {
		const htmlEl = el as HTMLElement;
		return !htmlEl.hasAttribute('disabled') && htmlEl.offsetParent !== null;
	}) as HTMLElement[];
}

export default {
	components: {
		QuillEditor,
		Diff,
		CodeEditor,
	},

	props: {
		initialContent: {
			type: String,
			required: true,
			default: '',
		},
	},

	emits: ['content-update'],

	data() {
		return {
			content: this.initialContent,
			oldContent: this.initialContent,
			rawHtml: '',
			processedContent: '',
			showHtmlDialog: false,
			showHtmlCorrectionDialog: false,
			toolbarId: `toolbar-${uuidv4()}`,
			randomNumber: Math.random(),
			quillOptions: {
				modules: {
					toolbar: false, // Custom toolbar kullanıyoruz
					keyboard: {
						bindings: {
							// Shift+Enter should insert <br> instead of <p><br></p>
							shift_enter: {
								key: 13, // Enter key
								shiftKey: true,
								handler: function(range: any, context: any) {
									const quill = (this as any).quill;
									// Insert a line break and move cursor
									quill.insertText(range.index, '\n', 'user');
									quill.setSelection(range.index + 1, 'user');
									return false; // Prevent default behavior
								}
							}
						}
					}
				},
				theme: 'snow'
			}
		};
	},

	watch: {
		initialContent(newContent) {
			if (newContent !== this.content) {
				this.content = newContent;
			}
		},
	},

	// mounted() {
	// 	this.initializeQuill();
	// },

	methods: {
		// Clean and optimize HTML content (simplified for performance)
		cleanHtmlContent(html: string): string {
			if (!html) return '';
			
			// Simple and fast HTML cleaning
			let cleaned = html
				// Remove empty paragraphs with br
				.replace(/<p><br\s*\/?><\/p>/gi, '')
				.replace(/<p>\s*<br\s*\/?>\s*<\/p>/gi, '')
				// Remove empty paragraphs
				.replace(/<p>\s*<\/p>/gi, '')
				// Remove empty headings
				.replace(/<h[1-6]>\s*<\/h[1-6]>/gi, '')
				.replace(/<h[1-6]><br\s*\/?><\/h[1-6]>/gi, '')
				// Remove empty blockquotes
				.replace(/<blockquote>\s*<\/blockquote>/gi, '')
				.replace(/<blockquote><br\s*\/?><\/blockquote>/gi, '')
				// Remove empty list items
				.replace(/<li>\s*<\/li>/gi, '')
				.replace(/<li><br\s*\/?><\/li>/gi, '')
				// Normalize br tags
				.replace(/\s*<br\s*\/?>\s*/g, '<br>')
				// Convert line breaks to br tags
				.replace(/\n/g, '<br>')
				// Clean up extra spaces
				.replace(/\s+/g, ' ')
				.trim();
			
			// If content is only whitespace or empty, return empty string
			if (!cleaned || cleaned.replace(/<[^>]*>/g, '').trim() === '') {
				return '';
			}
			
			return cleaned;
		},

		// Preserve empty lines by adding br tags between block elements
		preserveEmptyLines(html: string): string {
			if (!html) return '';
			
			// Add br tags between consecutive block elements to preserve empty lines
			let processed = html
				// Add br between paragraphs
				.replace(/(<\/p>)(<p>)/gi, '$1<br>$2')
				// Add br between heading and paragraph
				.replace(/(<\/h[1-6]>)(<p>)/gi, '$1<br>$2')
				// Add br between paragraph and heading
				.replace(/(<\/p>)(<h[1-6]>)/gi, '$1<br>$2')
				// Add br between consecutive headings
				.replace(/(<\/h[1-6]>)(<h[1-6]>)/gi, '$1<br>$2')
				// Add br between blockquote and other elements
				.replace(/(<\/blockquote>)(<p>)/gi, '$1<br>$2')
				.replace(/(<\/p>)(<blockquote>)/gi, '$1<br>$2')
				.replace(/(<\/blockquote>)(<h[1-6]>)/gi, '$1<br>$2')
				.replace(/(<\/h[1-6]>)(<blockquote>)/gi, '$1<br>$2');
			
			return processed;
		},

		// Check if HTML content is valid
		isValidHtml(html: string): boolean {
			if (!html) return true;
			
			// Allowed HTML tags
			const allowedTags = ['p', 'br', 'strong', 'b', 'em', 'i', 'u', 's', 'strike', 'a', 'img', 'ul', 'ol', 'li', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'code'];
			
			// Find HTML tags
			const tagRegex = /<\/?([a-zA-Z][a-zA-Z0-9]*)\b[^<>]*>/g;
			const tags = html.match(tagRegex);
			
			if (!tags) return true;
			
			// Check if each tag is in the allowed list
			for (const tag of tags) {
				const match = tag.match(/<\/?([a-zA-Z][a-zA-Z0-9]*)/);
				if (match && match[1]) {
					const tagName = match[1].toLowerCase();
					if (!allowedTags.includes(tagName)) {
						return false;
					}
				}
			}
			
			return true;
		},

		toggleHtmlDialog() {
			this.rawHtml = this.content;
			this.showHtmlDialog = !this.showHtmlDialog;
		},

		handleContentUpdate(newContent: string) {
			this.oldContent = this.content;
			if (newContent !== this.content) {
				// Simple content update without heavy processing to prevent performance issues
				this.$emit('content-update', newContent);
			}
		},

		handleCodeEditorChange() {
			// Reset the highlighting state
			const codeEditor = this.$refs.codeEditor as any;
			if (codeEditor && codeEditor.$refs && codeEditor.$refs.code) {
				codeEditor.$refs.code.removeAttribute('data-highlighted');
			}
		},

		handleSaveRawHTML() {
			// Simple HTML processing to prevent performance issues
			const cleanedHtml = this.cleanHtmlContent(this.rawHtml);
			
			// Validate HTML
			if (!this.isValidHtml(cleanedHtml)) {
				// Suggest correction for invalid HTML
				this.processedContent = cleanedHtml;
				this.showHtmlCorrectionDialog = true;
				return;
			}
			
			// Load cleaned HTML into Quill editor
			const quillEditor = this.$refs.quillEditor as any;
			if (quillEditor) {
				quillEditor.pasteHTML(cleanedHtml);
				const quillProcessedContent = quillEditor.getHTML();
				
				// Simple check for significant differences
				if (cleanedHtml !== quillProcessedContent) {
					this.processedContent = quillProcessedContent;
					this.showHtmlCorrectionDialog = true;
					return;
				}
				
				this.$emit('content-update', quillProcessedContent);
			} else {
				this.$emit('content-update', cleanedHtml);
			}
			
			this.showHtmlDialog = false;
		},

		// Check if there's a significant difference between two HTML contents
		hasSignificantDifference(original: string, processed: string): boolean {
			// Only whitespace differences are not significant
			const originalText = original.replace(/\s+/g, ' ').trim();
			const processedText = processed.replace(/\s+/g, ' ').trim();
			
			// Tag structure differences are significant
			const originalTags = originalText.match(/<[^>]*>/g) || [];
			const processedTags = processedText.match(/<[^>]*>/g) || [];
			
			if (originalTags.length !== processedTags.length) {
				return true;
			}
			
			// Content differences are significant
			return originalText !== processedText;
		},

		handleSaveCorrectedHTML() {
			this.$emit('content-update', this.processedContent);
			this.showHtmlCorrectionDialog = false;
			this.showHtmlDialog = false;
		},

		handleHtmlCorrectionBack() {
			this.handleCodeEditorChange();
			this.showHtmlCorrectionDialog = false;
		},

		handleEnterFromQuill(event: KeyboardEvent) {
			// console.log(event);
			if (event.key === 'Enter' && !event.shiftKey) {
				// event.preventDefault(); // Prevent Quill's default Tab handling
				const focusableElements = getFocusableElements(document.body);
				// console.log(focusableElements);
				const currentIndex = focusableElements.indexOf(
					focusableElements.find((el: HTMLElement) => el.classList.contains(`view-html-${this.randomNumber}`)) as HTMLElement,
				);
				// console.log(currentIndex);
				const nextIndex = event.shiftKey ? currentIndex - 1 : currentIndex + 1;
				// console.log(nextIndex);
				this.$emit('content-update', this.oldContent);
				// focusableElements[nextIndex]?.focus();

				// this.$nextTick(() => {
				// 	this.content = this.oldContent;
				setTimeout(() => {
					// this.$emit('content-update', this.oldContent);
					const nextElement = focusableElements[nextIndex];
					if (nextElement && typeof nextElement.focus === 'function') {
						nextElement.focus();
					}
				}, 50);
				// });

				// focusableElements[nextIndex]?.focus();
				// console.log(focusableElements[nextIndex]);
				// console.log(focusableElements[nextIndex].focus());
				// const nextElement = this.$refs.quillContainer;
				// console.log(nextElement);
				// nextElement?.focus(); // Focus the next element in the DOM
			}
		},

		// initializeQuill() {
		// 	console.log(this.$refs.quillEditor.getQuill());
		// 	const quill = this.$refs.quillEditor.getQuill();

		// 	quill.keyboard.addBinding(
		// 		{
		// 			key: 'Enter',
		// 		},
		// 		() => {
		// 			console.log('Enter key pressed');
		// 			const focusableElements = getFocusableElements(document);

		// 			const currentFocusedElement = document.activeElement;

		// 			const currentIndex = focusableElements.indexOf(currentFocusedElement);

		// 			const nextIndex = context.shiftKey ? currentIndex - 1 : currentIndex + 1;

		// 			if (focusableElements[nextIndex]) {
		//                 focusableElements[nextIndex].focus();
		//             }
		// 		}
		// 	);
		// },
	},
};
</script>

<style lang="scss" scoped>
.quill-container {
	max-width: 80ch;
}

.quill-view-html {
	font-size: 1rem;
	color: #4b5563;
	font-weight: 550;
	border-radius: 4px;
	padding: 0.5rem;
	cursor: pointer;
}
</style>

<style lang="scss">
.custom-quill-editor {
	height: auto;

	.ql-container {
		height: auto;
	}

	.ql-editor {
		height: auto;
		min-height: 150px;
		max-height: 800px;
		resize: vertical;
		font-family: 'Poppins', sans-serif;
	}
}
</style>
