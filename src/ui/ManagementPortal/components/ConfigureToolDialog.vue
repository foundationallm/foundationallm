<template>
	<Dialog
		:visible="visible"
		modal
		header="Configure Tool"
		:style="{ minWidth: '50%' }"
		:closable="false"
	>
		<div id="aria-description" class="mb-2 font-weight-bold">Tool name:</div>
		<InputText
			v-model="modelValue.name"
			type="text"
			class="w-100"
			placeholder="Enter tool name"
			aria-labelledby="aria-cost-center"
		/>

		<div id="aria-description" class="mt-6 mb-2 font-weight-bold">Tool description:</div>
		<Textarea
			v-model="modelValue.description"
			auto-resize
			rows="5"
			type="text"
			class="w-100"
			placeholder="Enter tool description"
			aria-labelledby="aria-cost-center"
		/>

		<div id="aria-description" class="mt-6 mb-2 font-weight-bold">Tool package name:</div>
		<InputText
			v-model="modelValue.package_name"
			type="text"
			class="w-100"
			placeholder="Enter tool package name"
			aria-labelledby="aria-cost-center"
		/>

		<div id="aria-description" class="mt-6 mb-2 font-weight-bold">Resource objects:</div>
		<div v-for="(resourceObject, resourceObjectId) in modelValue.resource_object_ids">
			<div id="aria-resource-object-id" class="mt-6 mb-2">{{ resourceObjectId }}</div>

			<!-- <InputText
				v-model="modelValue.resource_object_ids[resourceObjectId].object_id"
				type="text"
				class="w-100"
				placeholder="Enter tool package name"
				aria-labelledby="aria-cost-center"
			/> -->

			<PropertyBuilder v-model="modelValue.resource_object_ids[resourceObjectId].properties" />
		</div>

		<!-- <PropertyBuilder v-model="modelValue.properties" /> -->

		<!-- <JsonEditorVue v-model="json" /> -->
		<div id="aria-description" class="mt-6 mb-2 font-weight-bold">Tool properties:</div>
		<PropertyBuilder v-model="modelValue.properties" />

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
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
			required: false,
		},

		models: {
			type: Array,
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
	},

	methods: {
		handleSave() {
			this.$emit('update:modelValue', this.json);
			this.handleClose();
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
