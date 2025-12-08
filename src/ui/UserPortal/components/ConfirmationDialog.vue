<template>
	<Dialog
		modal
		:visible="isVisible"
		:header="title"
		:style="{ minWidth: '50%', maxWidth: '600px' }"
		:closable="false"
		class="confirmation-dialog"
	>
		<div class="confirmation-message">
			<slot :message="resolvedMessage" :message-html="messageHtml">
				<div v-if="messageHtml" class="confirmation-message-content" v-html="messageHtml" />
				<p v-else>{{ resolvedMessage }}</p>
			</slot>
		</div>

		<template #footer>
			<div class="confirmation-dialog-footer">
				<Button
					:severity="confirmButtonSeverity"
					:label="confirmText"
					@click="handleConfirm"
					autofocus
				/>

				<Button v-if="hasCancelButton" class="ml-2" :label="cancelText" text @click="handleCancel" />
			</div>
		</template>
	</Dialog>
</template>

<script lang="ts">
import { marked } from 'marked';
import DOMPurify from 'dompurify';
import { useConfirmationStore } from '@/stores/confirmationStore';

export default {
	props: {
		header: {
			type: String,
			required: false,
			default: undefined,
		},

		confirmText: {
			type: String,
			required: false,
			default: undefined,
		},

		hasCancelButton: {
			type: Boolean,
			required: false,
			default: undefined,
		},

		cancelText: {
			type: String,
			required: false,
			default: undefined,
		},

		visible: {
			type: Boolean,
			required: false,
			default: undefined,
		},

		message: {
			type: String,
			required: false,
			default: '',
		},

		confirmButtonSeverity: {
			type: String as () => 'primary' | 'danger' | 'success' | 'warning',
			required: false,
			default: undefined,
		},
	},

	emits: ['cancel', 'confirm', 'update:visible'],

	computed: {
		confirmationStore() {
			return useConfirmationStore();
		},

		// Use store values if not used as a local component
		isVisible() {
			return this.visible !== undefined ? this.visible : this.confirmationStore.isVisible;
		},

		title() {
			return this.header || this.confirmationStore.title;
		},

		resolvedMessage() {
			return this.$props.message || this.confirmationStore.message;
		},

		messageHtml() {
			const rawMessage = this.resolvedMessage || '';
			if (!rawMessage) {
				return '';
			}

			const html = marked.parse(rawMessage, { async: false });
			return DOMPurify.sanitize(html);
		},

		confirmText() {
			return this.$props.confirmText || this.confirmationStore.confirmText;
		},

		hasCancelButton() {
			return this.$props.hasCancelButton || this.confirmationStore.hasCancelButton;
		},

		cancelText() {
			return this.$props.cancelText || this.confirmationStore.cancelText;
		},

		confirmButtonSeverity() {
			return this.$props.confirmButtonSeverity || this.confirmationStore.confirmButtonSeverity;
		},
	},

	methods: {
		async handleConfirm() {
			if (this.visible !== undefined) {
				this.$emit('confirm', true);
				this.$emit('update:visible', false);
			} else {
				await this.confirmationStore.confirm();
			}
		},

		handleCancel() {
			if (this.visible !== undefined) {
				this.$emit('cancel', false);
				this.$emit('update:visible', false);
			} else {
				this.confirmationStore.cancel();
			}
		},
	},
};
</script>

<style lang="scss" scoped>
.confirmation-dialog {
	:deep(.p-dialog-header) {
		background-color: var(--primary-color, #6366f1);
		color: var(--primary-text, white);
		font-weight: 600;
		padding: 1.25rem;
	}

	:deep(.p-dialog-content) {
		padding: 1.5rem;
	}

	:deep(.p-dialog-footer) {
		padding: 1rem 1.5rem;
		border-top: 1px solid #e5e7eb;
	}
}

.confirmation-message {
	font-size: 1rem;
	line-height: 1.5;
	color: #4b5563;
}

.confirmation-dialog-footer {
	display: flex;
	justify-content: flex-end;
	gap: 0.5rem;
}
</style>


