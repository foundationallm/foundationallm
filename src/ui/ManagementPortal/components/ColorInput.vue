<template>
	<div class="color-input-container">
		<InputText
			:value="value"
			:aria-labelledby="ariaLabel"
			class="branding-input branding-color-input"
			@input="$emit('update-value', $event.target.value)"
		/>
		<ColorPicker
			:model-value="color"
			:format="format"
			:pt="{
				input: {
					style: {
						backgroundColor: color,
					},
				},
			}"
			default-color="ffffff"
			class="color-picker"
			@change="onColorChange"
		/>
		<Button
			:disabled="value === originalValue"
			class="color-undo-button"
			icon="pi pi-undo"
			aria-label="Reset to default color"
			@click="$emit('reset', originalValue)"
		/>
	</div>
</template>

<script lang="ts">
export default {
	props: {
		value: {
			required: true,
			type: String,
		},

		color: {
			required: true,
			type: String,
		},

		format: {
			required: true,
			type: String,
		},

		originalValue: {
			required: true,
			type: String,
		},

		ariaLabel: {
			required: true,
			type: String,
		},
	},

	emits: ['update-value', 'reset'],

	methods: {
		onColorChange(event: any) {
			this.$emit('update-value', event.value);
		},
	},
};
</script>

<style lang="scss" scoped>
.color-input-container {
	display: flex;
	align-items: center;
}

.branding-color-input {
	width: 30ch;
}

.color-picker {
	width: 50px;
}

.color-undo-button {
	border: 2px solid #e1e1e1 !important;
	border-width: 2px 2px 2px 0 !important;
	width: 50px;
}
</style>
