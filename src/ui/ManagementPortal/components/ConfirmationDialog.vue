<template>
	<Dialog modal :visible="isVisible" :header="dialogTitle" :style="{ minWidth: '50%', maxWidth: '600px' }" :closable="false" class="confirmation-dialog">
		<!-- Body slot -->
		<div class="confirmation-message">
			<slot>{{ dialogMessage }}</slot>
		</div>

		<template #footer>
		<div class="confirmation-dialog-footer">
			<!-- Confirm -->
			<Button :severity="dialogConfirmButtonSeverity === 'danger' ? 'danger' : 'primary'" :label="dialogConfirmText" @click="handleConfirm" autofocus/>

			<!-- Cancel -->
			<Button class="ml-2" :label="dialogCancelText" text @click="handleCancel" />
		</div>
		</template>
	</Dialog>
</template>

<script lang="ts">
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
			default: 'primary',
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

		dialogTitle() {
			return this.header || this.confirmationStore.title;
		},

		dialogMessage() {
			return this.$props.message || this.confirmationStore.message;
		},

		dialogConfirmText() {
			return this.$props.confirmText || this.confirmationStore.confirmText;
		},

		dialogCancelText() {
			return this.$props.cancelText || this.confirmationStore.cancelText;
		},

		dialogConfirmButtonSeverity() {
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
