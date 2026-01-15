<template>
	<Dialog
		:visible="visible"
		modal
		header="Configure Tool"
		:style="{ minWidth: '80%' }"
		:closable="false"
	>
		<!-- 	<div id="aria-tool-type" class="mb-2 font-bold">Tool type:</div>
		<Dropdown
			v-model="toolType"
			:options="toolTypeOptions"
			option-label="label"
			option-value="value"
			placeholder="--Select--"
			aria-labelledby="aria-tool-type"
		/> -->

		<!-- 		<div id="aria-tool-name" class="mb-2 font-bold">Tool:</div>
		<Dropdown
			v-model="toolObject.name"
			:options="toolOptions"
			option-label="label"
			option-value="label"
			placeholder="--Select--"
			aria-labelledby="aria-tool-type"
			@change="handleToolSelection"
		/> -->

		<div id="aria-tool-name" class="mb-2 font-bold">Tool name:</div>
		<InputText
			v-model="toolObject.name"
			type="text"
			class="w-full"
			placeholder="Enter tool name"
			aria-labelledby="aria-tool-name"
		/>

		<div id="aria-tool-description" class="mt-6 mb-2 font-bold">Tool description:</div>
		<Textarea
			v-model="toolObject.description"
			auto-resize
			rows="5"
			type="text"
			class="w-full"
			placeholder="Enter tool description"
			aria-labelledby="aria-tool-description"
		/>

		<div id="aria-tool-package-name" class="mt-6 mb-2 font-bold">Tool package name:</div>
		<InputText
			v-model="toolObject.package_name"
			type="text"
			class="w-full"
			placeholder="Enter tool package name"
			aria-labelledby="aria-tool-package-name"
		/>

		<div id="aria-tool-class-name" class="mt-6 mb-2 font-bold">Tool class name:</div>
		<InputText
			v-model="toolObject.class_name"
			type="text"
			class="w-full"
			placeholder="Enter tool class name"
			aria-labelledby="aria-tool-class-name"
		/>

		<div class="mt-6 mb-2 font-bold">Tool resources:</div>
		<ResourceTable :resources="toolObject.resource_object_ids" @delete="handleDeleteToolResource" />

		<CreateResourceObjectDialog
			v-if="showCreateResourceObjectDialog"
			:visible="showCreateResourceObjectDialog"
			resource-context="tool"
			@update:visible="showCreateResourceObjectDialog = false"
			@update:model-value="handleAddToolResource"
		/>

		<div class="flex justify-end mt-4">
			<Button
				severity="primary"
				label="Add Tool Resource"
				@click="showCreateResourceObjectDialog = true"
			/>
		</div>

		<div class="mt-6 mb-2 font-bold">Tool properties:</div>
		<PropertyBuilder v-model="toolObject.properties" />

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
		</template>
	</Dialog>
</template>

<script lang="ts">
// import api from '@/js/api';

export default {
	props: {
		modelValue: {
			type: [Object, String],
			required: false,
			default: () => ({
				name: '' as string,
				description: '' as string,
				package_name: 'FoundationaLLM' as string,
				class_name: '' as string,
				resource_object_ids: {},
			}),
		},

		existingTools: {
			type: Array,
			required: false,
			default: () => [],
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	emits: ['update:modelValue', 'update:visible'],

	data() {
		return {
			toolObject: {
				type: 'tool' as string,
				name: '' as string,
				description: '' as string,
				package_name: 'FoundationaLLM' as string,
				class_name: '' as string,
				resource_object_ids: {},
			},
			showCreateResourceObjectDialog: false,

			// toolType: null,
			// toolTypeOptions: [
			// 	{
			// 		label: 'Internal',
			// 		value: 'internal',
			// 	},
			// 	{
			// 		label: 'Custom',
			// 		value: 'custom',
			// 	},
			// ] as Object[],

			toolOptions: [] as Object[],
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.toolObject)) return;
				this.toolObject = JSON.parse(JSON.stringify(this.modelValue));
			},
		},
	},

	async created() {
		// const tools = await api.getAgentTools();
		// this.toolOptions = tools.map((tool) => ({
		// 	label: tool.resource.name,
		// 	value: tool.resource,
		// }));
	},

	methods: {
		handleToolSelection(event) {
			const tool = this.toolOptions.find((tool) => tool.label === event.value)?.value;
			this.toolObject.type = tool.type;
			this.toolObject.name = tool.name;
			this.toolObject.description = tool.description;
		},

		handleAddToolResource(resourceToAdd) {
			this.toolObject.resource_object_ids[resourceToAdd.object_id] = resourceToAdd;
			this.showCreateResourceObjectDialog = false;
		},

		// handleEditToolResource(resourceToEdit) {
		// 	this.toolObject.resource_object_ids[resourceToEdit.object_id] = resourceToEdit;
		// },

		handleDeleteToolResource(resourceToDelete) {
			delete this.toolObject.resource_object_ids[resourceToDelete.object_id];
		},

		handleSave() {
			const errors = [];

			if (
				this.modelValue.name !== this.toolObject.name &&
				this.existingTools.findIndex((tool) => tool.name === this.toolObject.name) !== -1
			) {
				errors.push('This tool name aleady exists on this agent.');
			}

			if (!this.toolObject.name) {
				errors.push('Please provide a tool name.');
			}

			if (!this.toolObject.description) {
				errors.push('Please provide a tool description.');
			}

			if (!this.toolObject.package_name) {
				errors.push('Please provide a tool package name.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.$emit('update:modelValue', this.toolObject);
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
