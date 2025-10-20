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
				
				// Create HTML formatted code block with proper styling for rich text applications
				const htmlContent = this.createFormattedHtml(plainText);
				
				// Use modern clipboard API - copy both plain text and HTML formats
				await navigator.clipboard.write([
					new ClipboardItem({
						"text/html": new Blob([htmlContent], { type: "text/html" }),
						"text/plain": new Blob([plainText], { type: "text/plain" })
					})
				]);

				this.$appStore.addToast({
					severity: 'success',
					life: 5000,
					detail: 'Code copied to clipboard with formatting!',
				});
			} catch (error) {
				console.warn('Modern clipboard API failed, falling back to legacy method:', error);
				
				// Fallback: legacy method (plain text only)
				try {
					const plainText = decodeURIComponent(this.codecontent);
					const textarea = document.createElement('textarea');
					textarea.value = plainText;
					document.body.appendChild(textarea);
					textarea.select();
					document.execCommand('copy');
					document.body.removeChild(textarea);

					this.$appStore.addToast({
						severity: 'success',
						life: 5000,
						detail: 'Code copied to clipboard!',
					});
				} catch (fallbackError) {
					console.error('Clipboard copy failed:', fallbackError);
					this.$appStore.addToast({
						severity: 'error',
						detail: 'Failed to copy to clipboard',
						life: 5000
					});
				}
			}
		},

		createFormattedHtml(code) {
			// Create a more comprehensive HTML structure that works well in rich text editors
			const escapedCode = this.escapeHtml(code);
			const languageLabel = this.language !== 'plaintext' ? this.language : '';
			
			return `
				<div style="font-family: 'Consolas', 'Monaco', 'Courier New', monospace; background-color: #f6f8fa; border: 1px solid #d0d7de; border-radius: 6px; padding: 16px; margin: 8px 0; overflow-x: auto;">
					${languageLabel ? `<div style="font-size: 12px; color: #656d76; margin-bottom: 8px; font-weight: 600;">${languageLabel.toUpperCase()}</div>` : ''}
					<pre style="margin: 0; font-family: inherit; font-size: 14px; line-height: 1.45; color: #24292f; white-space: pre; overflow-x: auto;"><code>${escapedCode}</code></pre>
				</div>
			`.trim();
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
