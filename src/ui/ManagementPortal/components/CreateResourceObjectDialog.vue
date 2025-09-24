<template>
	<Dialog
		:visible="visible"
		modal
		header="Add Resource"
		:style="{ minWidth: '50%' }"
		:closable="false"
	>
		<template v-if="loading">
			<div class="loading-overlay" role="status" aria-live="polite" aria-label="Loading resource creation">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>

		<!-- Resource type -->
		<div class="mb-1">Resource Type:</div>
		<Dropdown
			v-model="resourceType"
			:options="resourceTypeOptions"
			option-label="label"
			option-value="value"
			placeholder="--Select--"
		/>

		<!-- Resource selection -->
		<div class="mb-1 mt-4">Resource:</div>
		<Dropdown
			v-model="resourceId"
			:options="resourceOptions"
			option-label="name"
			option-value="object_id"
			placeholder="--Select--"
		/>

		<!-- Resource role -->
		<div class="mb-1 mt-4">Resource Role:</div>
		<InputText
			v-model="resourceRole"
			type="text"
			class="w-full"
			placeholder="Enter resource role"
			aria-labelledby="aria-cost-center"
		/>

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
		</template>
	</Dialog>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
	props: {
		modelValue: {
			type: [Object, String],
			required: false,
			default: () => ({
				object_id: null,
				properties: {},
			}),
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	emits: ['update:modelValue', 'update:visible'],

	data() {
		return {
			loading: false,
			loadingStatusText: '',

			resourceId: '' as string,
			resourceRole: '' as string,

			resourceType: null,
			resourceTypeOptions: [
				{
					label: 'API Endpoint',
					value: 'apiEndpoint',
				},
				{
					label: 'Azure AI Project',
					value: 'project',
				},
				{
					label: 'Data Pipeline',
					value: 'datapipeline',
				},
				{
					label: 'Embedding profile',
					value: 'textEmbeddingProfile',
				},
				{
					label: 'Model',
					value: 'model',
				},
				{
					label: 'Prompt',
					value: 'prompt',
				},
				{
					label: 'Vector Database',
					value: 'vectordatabase',
				},
				{
					label: 'Vector store',
					value: 'indexingProfile',
				},
			],

			resourceOption: null,
			resourceOptions: [],
		};
	},

	watch: {
		async resourceType(newType, oldType) {
			if (newType === oldType) return;

			await this.updateResourceOptions(newType);
		},
	},

	methods: {
		getResourceNameFromId(resourceId) {
			const parts = resourceId.split('/').filter(Boolean);
			return parts.slice(-1)[0];
		},

		getResourceTypeFromId(resourceId) {
			const parts = resourceId.split('/').filter(Boolean);
			const type = parts.slice(-2)[0];

			if (type === 'prompts') {
				return 'prompt';
			} else if (type === 'aiModels') {
				return 'model';
			} else if (type === 'textEmbeddingProfiles') {
				return 'textEmbeddingProfile';
			} else if (type === 'indexingProfiles') {
				return 'indexingProfile';
			} else if (type === 'apiEndpointConfigurations') {
				return 'apiEndpoint';
			}

			return type;
		},

		async updateResourceOptions(resourceType) {
			let apiMethod = null;
			if (resourceType === 'model') {
				this.loadingStatusText = 'Loading models...';
				apiMethod = api.getAIModels;
			} else if (resourceType === 'textEmbeddingProfile') {
				this.loadingStatusText = 'Loading text embedding profiles...';
				apiMethod = api.getTextEmbeddingProfiles;
			} else if (resourceType === 'indexingProfile') {
				this.loadingStatusText = 'Loading vector stores...';
				apiMethod = api.getAgentIndexes;
			} else if (resourceType === 'prompt') {
				this.loadingStatusText = 'Loading prompts...';
				apiMethod = api.getPrompts;
			} else if (resourceType === 'apiEndpoint') {
				this.loadingStatusText = 'Loading api endpoints...';
				apiMethod = api.getOrchestrationServices;
			} else if (resourceType === 'datapipeline') {
				this.loadingStatusText = 'Loading data pipelines...';
				apiMethod = api.getPipelines;
			} else if (resourceType === 'vectordatabase') {
				this.loadingStatusText = 'Loading vector databases...';
				apiMethod = api.getVectorDatabases;
			} else if (resourceType === 'project') {
				this.loadingStatusText = 'Loading projects...';
				apiMethod = api.getProjects;
			}

			this.loading = true;
			try {
				this.resourceOptions = (await apiMethod.call(api))
					.map((resource) => resource.resource)
					.sort((a, b) => a.name.localeCompare(b.name));
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		handleSave() {
			const errors = [];

			if (!this.resourceId) {
				errors.push('Please select a resource.');
			}

			if (!this.resourceRole) {
				errors.push('Please input a resource role.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.$emit('update:modelValue', {
				object_id: this.resourceId,
				properties: {
					object_role: this.resourceRole,
				},
			});
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped>
.loading-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 16px;
	z-index: 10;
	background-color: rgba(255, 255, 255, 0.9);
	pointer-events: auto;
}
</style>
