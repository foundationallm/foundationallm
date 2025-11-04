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
			allResourceOptions: [], // Store all resources for filtering
			searchFilter: '' as string,
			resourceCache: new Map<string, any[]>(), // Cache for different resource types
			resourceLoadingStates: new Map<string, boolean>(), // Track loading states
		};
	},

	watch: {
		async resourceType(newType, oldType) {
			if (newType === oldType) return;

			await this.updateResourceOptions(newType);
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
				this.loadingStatusText = 'Loading models...';
				apiMethod = api.getAIModels;
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

			// Mark as loading
			this.resourceLoadingStates.set(resourceType, true);
			this.loading = true;

			try {
				if (!apiMethod) {
					throw new Error(`No API method found for resource type: ${resourceType}`);
				}
				const allResources = (await apiMethod.call(api))
					.map((resource: any) => resource.resource)
					.sort((a: any, b: any) => a.name.localeCompare(b.name));

				// Cache the resources
				this.resourceCache.set(resourceType, allResources);
				this.allResourceOptions = allResources;
				
				// Apply initial filtering
				this.applyResourceFiltering(resourceType);
			} catch (error: any) {
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
		},

		/**
		 * Filter prompts based on context and search criteria
		 * @param prompts - Array of prompt objects
		 * @param context - The context for filtering ('workflowResource', 'editMode', etc.)
		 * @param searchCriteria - Optional search criteria
		 */
		filterPromptsByCriteria(prompts: any[], context = 'workflowResource', searchCriteria = ''): any[] {
			let filteredPrompts = [...prompts];

			// Apply context-specific filtering
			if (context === 'workflowResource') {
				// For workflow resources, filter to relevant prompt categories
				filteredPrompts = filteredPrompts.filter(prompt => 
					prompt.category === 'Workflow' || 
					prompt.category === 'System' ||
					prompt.name.toLowerCase().includes('workflow')
				);
			}

			// Apply search criteria if provided
			if (searchCriteria) {
				const criteria = searchCriteria.toLowerCase();
				filteredPrompts = filteredPrompts.filter(prompt => 
					prompt.name.toLowerCase().includes(criteria) ||
					prompt.description?.toLowerCase().includes(criteria)
				);
			}

			return filteredPrompts;
		},

		/**
		 * Filter prompts based on search input
		 */
		filterPrompts(): void {
			this.applyResourceFiltering(this.resourceType);
		},

		/**
		 * Clear resource cache when needed
		 */
		clearResourceCache(): void {
			this.resourceCache.clear();
			this.resourceLoadingStates.clear();
			this.allResourceOptions = [];
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
