<template>
	<div class="quill-container">
		<QuillEditor
			ref="quillEditor"
			:content="content"
			:toolbar="`#${toolbarId}`"
			content-type="html"
			@update:content="handleContentUpdate"
		>
			<template #toolbar>
				<div :id="toolbarId">
					<select class="ql-size" aria-label="Text Size" title="Text Size">
						<option value="small"></option>
						<option selected></option>
						<option value="large"></option>
						<option value="huge"></option>
					</select>
					<button class="ql-bold" aria-label="Bold"></button>
					<button class="ql-italic" aria-label="Italic"></button>
					<button class="ql-underline" aria-label="Underline"></button>
					<button class="ql-strike" aria-label="Strike"></button>
					<button class="ql-link" aria-label="Link"></button>
					<button class="ql-image" aria-label="Image"></button>
					<button class="ql-list" value="ordered" aria-label="Ordered List"></button>
					<button class="ql-list" value="bullet" aria-label="Unordered List"></button>
					<button class="ql-clean" aria-label="Remove Styles"></button>
					<button
						class="quill-view-html"
						aria-label="Edit HTML"
						style="width: 100px"
						@click="toggleHtmlDialog"
					>
						Edit HTML
					</button>
				</div>
			</template>
		</QuillEditor>
		<Dialog
			v-model:visible="showHtmlDialog"
			:modal="true"
			:closable="false"
			:style="{ width: '50vw' }"
		>
			<Textarea v-model="rawHtml" style="width: 100%; height: 100%" auto-resize />
			<template #footer>
				<Button label="Save" @click="saveRawHtml" />
				<Button label="Cancel" @click="toggleHtmlDialog" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';
import { v4 as uuidv4 } from 'uuid';

function filterQuillHTML(html: string) {
	return html
		.replace(/(<p><br><\/p>)+/g, (match) => '<br>'.repeat(match.split('<p><br></p>').length))
		.replace(/<\/p><p>/g, '<br>')
		.replace(/<\/?p[^>]*>/g, '');
}

export default {
	components: { QuillEditor },

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
			rawHtml: '',
			showHtmlDialog: false,
			toolbarId: `toolbar-${uuidv4()}`, // Generate unique toolbar ID
		};
	},

	watch: {
		initialContent(newContent) {
			if (newContent !== this.content) {
				this.content = newContent;
			}
		},
	},

	methods: {
		toggleHtmlDialog() {
			this.rawHtml = filterQuillHTML(JSON.parse(JSON.stringify(this.content)));
			this.showHtmlDialog = !this.showHtmlDialog;
		},

		saveRawHtml() {
			this.rawHtml = this.rawHtml.replace(/<br>/g, '<br>');
			this.content = this.rawHtml
				.split('<br>')
				.map((line) => `<p>${line}</p>`)
				.join('');
			this.$emit('content-update', this.content);
			this.showHtmlDialog = false;
		},

		handleContentUpdate(newContent) {
			if (newContent !== this.content) {
				this.$emit('content-update', newContent);
			}
		},
	},
};
</script>

<style scoped>
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
