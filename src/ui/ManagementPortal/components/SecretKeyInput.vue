<template>
	<div class="flex-container">
		<Textarea
			v-if="textarea"
			:value="displayValue"
			:readonly="isHidden"
			class="w-100"
			auto-resize
			rows="5"
			type="text"
			@update:model-value="handleUpdate"
		/>

		<InputText
			v-else
			:value="displayValue"
			:readonly="isHidden"
			class="w-100"
			type="text"
			@update:model-value="handleUpdate"
		/>

		<Button
			:icon="isHidden ? 'pi pi-eye' : 'pi pi-eye-slash'"
			:label="isHidden ? 'Show' : 'Hide'"
			class="p-button-text"
			@click="() => (isHidden = !isHidden)"
		></Button>
	</div>
</template>

<script>
export default {
	props: {
		modelValue: {
			type: String,
			required: true,
		},

		textarea: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['update:modelValue'],

	data() {
		return {
			isHidden: true,
			value: '',
		};
	},

	computed: {
		displayValue() {
			// Replace each character with a dot to obscure it
			// Maybe replace with a default amount of dots instead if we don't
			// want the length of the secret to be disclosed either?
			if (this.isHidden) return this.value.replace(/./g, '•');
			return this.value;
		},
	},

	watch: {
		modelValue: {
			immediate: true,
			handler() {
				this.value = this.modelValue;
			},
		},
	},

	created() {
		// Determine the initial visibility based on whether a resolved value exists.
		// If a resolved value exists and is not an empty string, it defaults to being hidden (false).
		// If no resolved value exists (undefined or empty string), the field should be shown to allow input (true).
		this.isHidden = !!this.value;
	},

	methods: {
		handleUpdate(inputValue) {
			if (this.isHidden) return;
			this.value = inputValue;
			this.$emit('update:modelValue', this.value);
		},
	},
};
</script>

<style lang="scss" scoped>
.flex-container {
	display: flex;
	align-items: center;
}
</style>
