<template>
	<div class="header">
		<!-- Language name -->
		<span>{{ language }}</span>

		<!-- Copy button -->
		<Button text size="small" label="Copy" class="copy-button" @click="copyToClipboard"></Button>
	</div>

	<!-- Highlighted code templating -->
	<slot />
</template>

<script>
export default {
	props: {
		language: {
			type: String,
			required: false,
			default: 'plaintext',
		},

		codecontent: {
			type: String,
			required: true,
		},
	},

	methods: {
		async copyToClipboard() {
			try {
				const plainText = decodeURIComponent(this.codecontent);
				
				// Create HTML formatted code block with syntax highlighting
				const htmlContent = `<pre><code class="language-${this.language}">${this.escapeHtml(plainText)}</code></pre>`;
				
				// Use modern clipboard API - copy both plain text and HTML formats
				if (navigator.clipboard && navigator.clipboard.write) {
					await navigator.clipboard.write([
						new ClipboardItem({
							"text/html": new Blob([htmlContent], { type: "text/html" }),
							"text/plain": new Blob([plainText], { type: "text/plain" })
						})
					]);
				} else {
					// Fallback: legacy method (plain text only)
					const textarea = document.createElement('textarea');
					textarea.value = plainText;
					document.body.appendChild(textarea);
					textarea.select();
					document.execCommand('copy');
					document.body.removeChild(textarea);
				}

				this.$appStore.addToast({
					severity: 'success',
					detail: 'Copied to clipboard!',
				});
			} catch (error) {
				console.error('Clipboard copy failed:', error);
				this.$appStore.addToast({
					severity: 'error',
					detail: 'Failed to copy to clipboard',
				});
			}
		},

		escapeHtml(text) {
			const div = document.createElement('div');
			div.textContent = text;
			return div.innerHTML;
		},
	},
};
</script>

<style scoped>
.header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--secondary-button-text);
	background: var(--secondary-color);
	padding-left: 8px;
}

.copy-button {
	color: var(--secondary-button-text) !important;
}
</style>
