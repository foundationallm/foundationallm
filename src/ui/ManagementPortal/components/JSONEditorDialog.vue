<template>
	<Dialog
		:visible="visible"
		modal
		header="Edit JSON"
		:style="{ minWidth: '50%' }"
		:closable="false"
	>
		<JsonEditorVue v-model="json" />

		<template #footer>
			<!-- Cancel -->
			<Button
				class="sidebar-dialog__button"
				label="Close"
				text
				@click="$emit('update:visible', false)"
			/>
		</template>
	</Dialog>
</template>

<script>
import JsonEditorVue from 'json-editor-vue';

export default {
	components: {
		JsonEditorVue,
	},

	props: {
		modelValue: {
			type: [Object, String],
			required: true,
		},

		visible: {
			type: Boolean,
			required: true,
		},
	},

	data() {
		return {
			json: {},
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.json)) return;
				this.json = this.modelValue;
			},
		},

		json: {
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.json)) return;
				this.modelValue = this.json;
			},
		},
	},
};
</script>

<style lang="scss" scoped>
	
</style>
