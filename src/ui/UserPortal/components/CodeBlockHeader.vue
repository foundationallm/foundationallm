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
		copyToClipboard() {
			const textarea = document.createElement('textarea');
			textarea.value = decodeURIComponent(this.codecontent);
			document.body.appendChild(textarea);
			textarea.select();
			document.execCommand('copy');
			document.body.removeChild(textarea);

			this.$toast.add({
				severity: 'success',
				detail: 'Copied to clipboard!',
				life: this.$appStore.autoHideToasts ? 5000 : null,
			});
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
