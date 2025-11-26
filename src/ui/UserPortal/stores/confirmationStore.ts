import { defineStore } from 'pinia';

interface ConfirmationOptions {
	title?: string;
	message: string;
	confirmText?: string;
	cancelText?: string;
	confirmButtonSeverity?: 'primary' | 'danger' | 'success' | 'warning';
	onConfirm?: () => void | Promise<void>;
	onCancel?: () => void;
}

export const useConfirmationStore = defineStore('confirmation', {
	state: () => ({
		isVisible: false,
		title: 'Confirm?',
		message: '',
		confirmText: 'Confirm',
		cancelText: 'Cancel',
		confirmButtonSeverity: 'primary' as 'primary' | 'danger' | 'success' | 'warning',
		onConfirm: null as (() => void | Promise<void>) | null,
		onCancel: null as (() => void) | null,
	}),

	actions: {
		show(options: ConfirmationOptions) {
			this.title = options.title || 'Confirm?';
			this.message = options.message;
			this.confirmText = options.confirmText || 'Confirm';
			this.cancelText = options.cancelText || 'Cancel';
			this.confirmButtonSeverity = options.confirmButtonSeverity || 'primary';
			this.onConfirm = options.onConfirm || null;
			this.onCancel = options.onCancel || null;
			this.isVisible = true;
		},

		hide() {
			this.isVisible = false;
		},

		async confirm() {
			if (this.onConfirm) {
				await this.onConfirm();
			}
			this.hide();
		},

		cancel() {
			if (this.onCancel) {
				this.onCancel();
			}
			this.hide();
		},
		confirmAsync(options: Omit<ConfirmationOptions, 'onConfirm' | 'onCancel'>): Promise<boolean> {
			return new Promise((resolve) => {
				this.show({
					...options,
					onConfirm: () => resolve(true),
					onCancel: () => resolve(false),
				});
			});
		},
	},
});


