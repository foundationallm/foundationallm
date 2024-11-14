<!-- ColorInputContainer.vue -->
<template>
	<div class="color-input-container">
		<InputText
			:value="value"
			@input="$emit('updateValue', $event.target.value)"
			class="branding-input branding-color-input"
			:aria-labelledby="ariaLabel"
		/>
		<ColorPicker
			:modelValue="color"
			class="color-picker"
			:format="format"
			defaultColor="ffffff"
			@change="onColorChange"
			:pt="{
				input: {
					style: {
						backgroundColor: color,
					},
				},
			}"
		/>
		<Button
			class="color-undo-button"
			icon="pi pi-undo"
			@click="$emit('reset', originalValue)"
			:disabled="value === originalValue"
			aria-label="Reset to default color"
		/>
	</div>
</template>

<script lang="ts">
export default {
	props: {
		value: String,
		color: String,
		format: String,
		originalValue: String,
		ariaLabel: String,
	},
	emits: ['updateValue', 'reset', 'colorChange'],
	methods: {
		onColorChange(event: any) {
			this.$emit('updateValue', event.value);
		},
	},
};
</script>

<style scoped>
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
	border: 2px solid #e1e1e1;
	border-width: 2px 2px 2px 0;
	width: 50px;
}
</style>
