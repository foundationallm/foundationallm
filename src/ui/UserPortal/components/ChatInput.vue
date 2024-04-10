<template>
	<div class="chat-input p-inputgroup">
		<InputText
			v-model="text"
			:disabled="disabled"
			class="input"
			type="text"
			placeholder="What would you like to ask?"
			@keydown.enter="handleSend"
		/>
		<Button
			:disabled="disabled"
			class="primary-button submit"
			icon="pi pi-send"
			label="Send"
			@click="handleSend"
		/>
	</div>
</template>

<script lang="ts">
export default {
	name: 'ChatInput',

	props: {
		disabled: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['send'],

	data() {
		return {
			text: '' as string,
		};
	},

	methods: {
		handleSend() {
			this.$emit('send', this.text);
			this.text = '';
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-input {
	display: flex;
	background-color: white;
	border-radius: 8px;
	width: 100%;
}

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}

.pre-input {
	flex: 0 0 10%;
}

.input {
	width: 100%;
	height: 100%;
	height: 64px;
}

.input:focus {
	// height: 192px;
}

.submit {
	flex: 0 0 10%;
	text-align: left;
	flex-basis: auto;
}
</style>

<style lang="scss">
@media only screen and (max-width: 545px) {
	.submit .p-button-label {
		display: none;
	}

	.submit .p-button-icon {
		margin: 0;
	}
}
</style>
