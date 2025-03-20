<template>
	<Dialog modal :visible="true" :header="header" :style="{ minWidth: '50%' }" :closable="false">
		<!-- Body slot -->
		<slot />

		<template #footer>
			<!-- Confirm -->
			<Button severity="primary" :label="confirmText" @click="handleConfirm" />

			<!-- Cancel -->
			<Button class="ml-2" :label="cancelText" text @click="handleCancel" />
		</template>
	</Dialog>
</template>

<script lang="ts">
export default {
	props: {
		header: {
			type: String,
			required: false,
			default: 'Confirm?',
		},

		confirmText: {
			type: String,
			required: false,
			default: 'Confirm',
		},

		cancelText: {
			type: String,
			required: false,
			default: 'Cancel',
		},
	},

	emits: ['cancel', 'confirm'],

	data() {
		return {};
	},

	methods: {
		handleConfirm() {
			this.$emit('confirm', true);
		},

		handleCancel() {
			this.$emit('cancel', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
