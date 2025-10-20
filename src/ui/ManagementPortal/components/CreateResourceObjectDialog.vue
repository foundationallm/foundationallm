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
	<div class="relative">
	<InputText
		v-model="resourceSearch"
		:disabled="!resourceType"
		type="text"
		class="w-full"
		placeholder="Start typing resource name..."
		@input="onResourceSearchInput"
	/>
		<div v-if="resourceSearchLoading" class="absolute right-2 top-1/2 -translate-y-1/2">
			<i class="pi pi-spin pi-spinner text-blue-500"></i>
		</div>
	</div>
	<div
		v-if="resourceOptions.length > 0 && resourceSearch && !selectedResource"
		class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto"
	>
		<div
			v-for="resource in resourceOptions"
			:key="resource.object_id"
			@click="selectResource(resource)"
			class="px-3 py-2 hover:bg-gray-100 cursor-pointer border-b border-gray-100 last:border-b-0"
		>
			<div class="font-medium">{{ resource.name }}</div>
		</div>
	</div>

	<div v-if="selectedResource" class="mt-2 mb-4 p-3 bg-gray-50 border border-gray-200 rounded-md">
		<div class="flex items-center justify-between">
			<div>
				<span class="text-sm font-medium">Selected Resource:</span>
				<span class="ml-2 text-sm">{{ selectedResource.name }}</span>
			</div>
			<Button icon="pi pi-times" severity="secondary" text @click="clearSelectedResource" class="p-1" />
		</div>
	</div>

		<!-- Search filter for prompts -->
		<div v-if="resourceType === 'prompt'" class="mb-1 mt-4">
			<div class="mb-2">Search prompts:</div>
			<InputText
				v-model="searchFilter"
				type="text"
				class="w-full"
				placeholder="Filter prompts by name or description..."
				@input="filterPrompts"
			/>
		</div>

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
			resourceSearch: '' as string,
			resourceSearchLoading: false as boolean,
			selectedResource: null as any,
			allResourcesCache: [] as any[],
		};
	},

	watch: {
		resourceType(newType, oldType) {
			if (newType === oldType) return;

			this.resourceSearch = '';
			this.selectedResource = null;
			this.resourceOptions = [];
			this.allResourcesCache = [];
			this.resourceSearchLoading = false;
			this.updateResourceOptions(newType);
		},
	},

	methods: {
		getResourceNameFromId(resourceId: string): string {
			const parts = resourceId.split('/').filter(Boolean);
			return parts.slice(-1)[0];
		},

		getResourceTypeFromId(resourceId: string): string {
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

		async updateResourceOptions(resourceType: string): Promise<void> {
			// Check if resources are already cached for this type
			if (this.resourceCache.has(resourceType)) {
				this.allResourceOptions = this.resourceCache.get(resourceType)!;
				this.applyResourceFiltering(resourceType);
				return;
			}

			// Check if already loading for this type
			if (this.resourceLoadingStates.get(resourceType)) {
				return;
			}

			let apiMethod: (() => Promise<any>) | null = null;
			if (resourceType === 'model') {
				apiMethod = api.getAIModels;
			} else if (resourceType === 'textEmbeddingProfile') {
				apiMethod = api.getTextEmbeddingProfiles;
			} else if (resourceType === 'indexingProfile') {
				apiMethod = api.getAgentIndexes;
			} else if (resourceType === 'prompt') {
				apiMethod = api.getPrompts;
			} else if (resourceType === 'apiEndpoint') {
				apiMethod = api.getOrchestrationServices;
			} else if (resourceType === 'datapipeline') {
				apiMethod = api.getPipelines;
			} else if (resourceType === 'vectordatabase') {
				apiMethod = api.getVectorDatabases;
			} else if (resourceType === 'project') {
				apiMethod = api.getProjects;
			}
			this.resourceSearchLoading = true;
			
			try {
				this.allResourcesCache = (await apiMethod.call(api))
					.map((resource) => resource.resource)
					.sort((a, b) => a.name.localeCompare(b.name));
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			} finally {
				this.loading = false;
				this.resourceLoadingStates.set(resourceType, false);
			}
		},

		/**
		 * Apply filtering based on resource type and search criteria
		 */
		applyResourceFiltering(resourceType: string): void {
			if (resourceType === 'prompt') {
				this.resourceOptions = this.filterPromptsByCriteria(
					this.allResourceOptions, 
					'workflowResource', 
					this.searchFilter
				);
			} else {
				// For other resource types, apply search filter if provided
				if (this.searchFilter) {
					const criteria = this.searchFilter.toLowerCase();
					this.resourceOptions = this.allResourceOptions.filter(resource => 
						resource.name.toLowerCase().includes(criteria) ||
						resource.description?.toLowerCase().includes(criteria)
					);
				} else {
					this.resourceOptions = this.allResourceOptions;
				}
			}
			this.resourceSearchLoading = false;
		},

		onResourceSearchInput() {
			if (!this.resourceSearch || !this.resourceSearch.trim()) {
				this.resourceOptions = [];
				return;
			}
			
			const searchTerm = this.resourceSearch.toLowerCase().trim();
			this.resourceOptions = this.allResourcesCache.filter((resource) => {
				const name = (resource.name || '').toLowerCase();
				const description = (resource.description || '').toLowerCase();
				return name.includes(searchTerm) || description.includes(searchTerm);
			});
		},

		selectResource(resource) {
			this.selectedResource = resource;
			this.resourceId = resource.object_id;
			this.resourceSearch = '';
			this.resourceOptions = [];
		},

		clearSelectedResource() {
			this.selectedResource = null;
			this.resourceId = '';
			this.resourceSearch = '';
			this.resourceOptions = [];
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
