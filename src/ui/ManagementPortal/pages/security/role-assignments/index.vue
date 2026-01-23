<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Role Assignments</h2>
				<div class="page-subheader">Manage instance-level and resource-specific role assignments.</div>
			</div>

			<div style="display: flex; align-items: center">
				<Button aria-label="Create Role Assignment" @click="openCreateDialog">
					<i
						class="pi pi-plus"
						style="color: var(--text-primary); margin-right: 8px"
						aria-hidden="true"
					></i>
					Create Role Assignment
				</Button>
			</div>
		</div>

		<!-- Create Role Assignment Dialog -->
		<Dialog
			v-model:visible="createDialogOpen"
			modal
			header="Create Role Assignment"
			:style="{ minWidth: '70%' }"
			@hide="handleDialogClose"
		>
			<CreateRoleAssignment
				ref="createForm"
				headless
				:scope="currentScope"
				:allowed-role-definition-names="allowedRoleDefinitions"
			/>

			<template #footer>
				<div class="mt-4">
					<Button label="Cancel" text @click="createDialogOpen = false" />
					<Button @click="handleCreateRoleAssignment">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Role Assignment
					</Button>
				</div>
			</template>
		</Dialog>

		<!-- Scope Selection Section -->
		<section class="scope-selection-section">
			<div class="flex flex-wrap gap-4 items-end mb-4">
				<!-- Scope Type Dropdown -->
				<div class="flex flex-col gap-1">
					<label for="scopeType" class="text-sm font-medium">Scope</label>
					<Dropdown
						id="scopeType"
						v-model="scopeType"
						:options="scopeTypeOptions"
						optionLabel="label"
						optionValue="value"
						placeholder="Select a scope"
						class="w-48"
						@change="onScopeTypeChange"
					/>
				</div>

				<!-- Portal Access Dropdown (only for portalAccess scope) -->
				<div v-if="scopeType === 'portalAccess'" class="flex flex-col gap-1">
					<label for="portalType" class="text-sm font-medium">Portal</label>
					<Dropdown
						id="portalType"
						v-model="selectedPortal"
						:options="portalOptions"
						optionLabel="label"
						optionValue="value"
						placeholder="Select a portal"
						class="w-64"
						@change="onPortalChange"
					/>
				</div>

				<!-- Resource Provider Dropdown (only for resource scope) -->
				<div v-if="scopeType === 'resource'" class="flex flex-col gap-1">
					<label for="resourceProvider" class="text-sm font-medium">Resource Provider</label>
					<Dropdown
						id="resourceProvider"
						v-model="selectedResourceProvider"
						:options="resourceProviders"
						optionLabel="label"
						optionValue="value"
						placeholder="Select a resource provider"
						class="w-64"
						@change="onResourceProviderChange"
					/>
				</div>

				<!-- Resource Type Dropdown (only for resource scope) -->
				<div v-if="scopeType === 'resource'" class="flex flex-col gap-1">
					<label for="resourceType" class="text-sm font-medium">Resource Type</label>
					<Dropdown
						id="resourceType"
						v-model="selectedResourceType"
						:options="filteredResourceTypes"
						optionLabel="label"
						optionValue="value"
						placeholder="Select a resource type"
						class="w-64"
						:disabled="!selectedResourceProvider"
						@change="onResourceTypeChange"
					/>
				</div>

				<!-- Resource Name Text Box (only for resource scope) -->
				<div v-if="scopeType === 'resource'" class="flex flex-col gap-1">
					<label for="resourceName" class="text-sm font-medium">Resource Name</label>
					<InputText
						id="resourceName"
						v-model="resourceName"
						placeholder="Enter resource name"
						class="w-64"
						:disabled="!selectedResourceType"
						@keyup.enter="viewResourcePermissions"
					/>
				</div>

				<!-- View Permissions Button (only for resource scope) -->
				<div v-if="scopeType === 'resource'" class="flex flex-col gap-1">
					<label class="text-sm font-medium invisible">Action</label>
					<Button
						label="View Permissions"
						icon="pi pi-search"
						:disabled="!canViewResourcePermissions"
						@click="viewResourcePermissions"
					/>
				</div>
			</div>

			<!-- Current Scope Display -->
			<div v-if="currentScopeDisplay" class="current-scope-display mb-4">
				<span class="text-sm text-gray-600">Viewing permissions for: </span>
				<span class="text-sm font-medium">{{ currentScopeDisplay }}</span>
			</div>
		</section>

		<!-- Role Assignments Table -->
		<template v-if="shouldShowTable">
			<RoleAssignmentsTable
				:key="tableKey"
				:scope="currentScope"
				:default-hide-inherited="scopeType === 'portalAccess'"
			/>
		</template>
		<template v-else>
			<div class="resource-select-prompt">
				<i class="pi pi-info-circle" style="font-size: 1.5rem; color: var(--primary-color)"></i>
				<p v-if="scopeType === 'portalAccess'">Select a portal to view its role assignments.</p>
				<p v-else>Select a resource provider, type, and name, then click "View Permissions" to see role assignments.</p>
			</div>
		</template>
	</main>
</template>

<script lang="ts">
import CreateRoleAssignment from '@/pages/security/role-assignments/create.vue';

// Resource providers and their associated resource types
const RESOURCE_PROVIDERS = [
	{ label: 'FoundationaLLM.Agent', value: 'FoundationaLLM.Agent' },
	{ label: 'FoundationaLLM.AIModel', value: 'FoundationaLLM.AIModel' },
	{ label: 'FoundationaLLM.Attachment', value: 'FoundationaLLM.Attachment' },
	{ label: 'FoundationaLLM.Authorization', value: 'FoundationaLLM.Authorization' },
	{ label: 'FoundationaLLM.AzureAI', value: 'FoundationaLLM.AzureAI' },
	{ label: 'FoundationaLLM.AzureOpenAI', value: 'FoundationaLLM.AzureOpenAI' },
	{ label: 'FoundationaLLM.Configuration', value: 'FoundationaLLM.Configuration' },
	{ label: 'FoundationaLLM.Context', value: 'FoundationaLLM.Context' },
	{ label: 'FoundationaLLM.Conversation', value: 'FoundationaLLM.Conversation' },
	{ label: 'FoundationaLLM.DataPipeline', value: 'FoundationaLLM.DataPipeline' },
	{ label: 'FoundationaLLM.DataSource', value: 'FoundationaLLM.DataSource' },
	{ label: 'FoundationaLLM.Plugin', value: 'FoundationaLLM.Plugin' },
	{ label: 'FoundationaLLM.Prompt', value: 'FoundationaLLM.Prompt' },
	{ label: 'FoundationaLLM.Vector', value: 'FoundationaLLM.Vector' },
];

const RESOURCE_TYPES_BY_PROVIDER: Record<string, { label: string; value: string }[]> = {
	'FoundationaLLM.Agent': [
		{ label: 'agents', value: 'agents' },
		{ label: 'agentFiles', value: 'agentFiles' },
		{ label: 'agentAccessTokens', value: 'agentAccessTokens' },
		{ label: 'agentTemplates', value: 'agentTemplates' },
		{ label: 'tools', value: 'tools' },
		{ label: 'workflows', value: 'workflows' },
	],
	'FoundationaLLM.AIModel': [
		{ label: 'aiModels', value: 'aiModels' },
	],
	'FoundationaLLM.Attachment': [
		{ label: 'attachments', value: 'attachments' },
	],
	'FoundationaLLM.Authorization': [
		{ label: 'roleAssignments', value: 'roleAssignments' },
		{ label: 'roleDefinitions', value: 'roleDefinitions' },
		{ label: 'securityPrincipals', value: 'securityPrincipals' },
	],
	'FoundationaLLM.AzureAI': [
		{ label: 'projects', value: 'projects' },
	],
	'FoundationaLLM.AzureOpenAI': [
		{ label: 'assistantUserContexts', value: 'assistantUserContexts' },
		{ label: 'fileUserContexts', value: 'fileUserContexts' },
	],
	'FoundationaLLM.Configuration': [
		{ label: 'appConfigurations', value: 'appConfigurations' },
		{ label: 'appConfigurationSets', value: 'appConfigurationSets' },
		{ label: 'apiEndpointConfigurations', value: 'apiEndpointConfigurations' },
	],
	'FoundationaLLM.Context': [
		{ label: 'fileContexts', value: 'fileContexts' },
	],
	'FoundationaLLM.Conversation': [
		{ label: 'conversations', value: 'conversations' },
	],
	'FoundationaLLM.DataPipeline': [
		{ label: 'dataPipelines', value: 'dataPipelines' },
	],
	'FoundationaLLM.DataSource': [
		{ label: 'dataSources', value: 'dataSources' },
	],
	'FoundationaLLM.Plugin': [
		{ label: 'plugins', value: 'plugins' },
	],
	'FoundationaLLM.Prompt': [
		{ label: 'prompts', value: 'prompts' },
	],
	'FoundationaLLM.Vector': [
		{ label: 'indexingProfiles', value: 'indexingProfiles' },
		{ label: 'textPartitioningProfiles', value: 'textPartitioningProfiles' },
		{ label: 'textEmbeddingProfiles', value: 'textEmbeddingProfiles' },
	],
};

// Portal access options
const PORTAL_OPTIONS = [
	{ label: 'User Portal', value: 'providers/FoundationaLLM.Configuration/appConfigurationSets/UserPortal' },
	{ label: 'Management Portal', value: 'providers/FoundationaLLM.Configuration/appConfigurationSets/ManagementPortal' },
];

// Scope type options
const SCOPE_TYPE_OPTIONS = [
	{ label: 'Instance', value: 'instance' },
	{ label: 'Portal Access', value: 'portalAccess' },
	{ label: 'Resource', value: 'resource' },
];

export default {
	name: 'RoleAssignments',

	components: {
		CreateRoleAssignment,
	},

	data() {
		return {
			scopeType: 'instance' as 'instance' | 'portalAccess' | 'resource',
			scopeTypeOptions: SCOPE_TYPE_OPTIONS,
			resourceProviders: RESOURCE_PROVIDERS,
			portalOptions: PORTAL_OPTIONS,
			selectedResourceProvider: null as string | null,
			selectedResourceType: null as string | null,
			resourceName: '' as string,
			selectedPortal: null as string | null,
			tableKey: 0,
			activeResourceScope: null as string | null,
			createDialogOpen: false,
		};
	},

	computed: {
		filteredResourceTypes(): { label: string; value: string }[] {
			if (!this.selectedResourceProvider) {
				return [];
			}
			return RESOURCE_TYPES_BY_PROVIDER[this.selectedResourceProvider] || [];
		},

		canViewResourcePermissions(): boolean {
			return !!(this.selectedResourceProvider && this.selectedResourceType && this.resourceName?.trim());
		},

		currentScope(): string | null {
			if (this.scopeType === 'instance') {
				return null;
			}
			if (this.scopeType === 'portalAccess') {
				return this.selectedPortal;
			}
			return this.activeResourceScope;
		},

		shouldShowTable(): boolean {
			// Show table for instance scope, or for portal/resource scope when a selection has been made
			if (this.scopeType === 'instance') return true;
			if (this.scopeType === 'portalAccess') return this.selectedPortal !== null;
			return this.activeResourceScope !== null;
		},

		currentScopeDisplay(): string {
			if (this.scopeType === 'instance') {
				return 'Instance (all instance-level permissions)';
			}
			if (this.scopeType === 'portalAccess' && this.selectedPortal) {
				const portal = this.portalOptions.find((p: { label: string; value: string }) => p.value === this.selectedPortal);
				return portal ? portal.label : this.selectedPortal;
			}
			if (this.activeResourceScope) {
				return this.activeResourceScope;
			}
			return '';
		},

		allowedRoleDefinitions(): string[] | null {
			// For portal access, only Reader role is allowed
			if (this.scopeType === 'portalAccess') {
				return ['Reader'];
			}
			return null;
		},
	},

	methods: {
		onScopeTypeChange() {
			if (this.scopeType === 'instance') {
				this.activeResourceScope = null;
				this.selectedPortal = null;
				this.tableKey++;
			} else if (this.scopeType === 'portalAccess') {
				this.activeResourceScope = null;
				this.selectedResourceProvider = null;
				this.selectedResourceType = null;
				this.resourceName = '';
			} else if (this.scopeType === 'resource') {
				this.selectedPortal = null;
			}
		},

		onPortalChange() {
			this.tableKey++;
		},

		onResourceProviderChange() {
			this.selectedResourceType = null;
			this.resourceName = '';
		},

		onResourceTypeChange() {
			this.resourceName = '';
		},

		viewResourcePermissions() {
			if (!this.canViewResourcePermissions) return;

			this.activeResourceScope = `providers/${this.selectedResourceProvider}/${this.selectedResourceType}/${this.resourceName}`;
			this.tableKey++;
		},

		openCreateDialog() {
			this.createDialogOpen = true;
		},

		handleDialogClose() {
			this.createDialogOpen = false;
		},

		async handleCreateRoleAssignment() {
			try {
				await this.$refs.createForm.createRoleAssignment();
				this.createDialogOpen = false;
				this.tableKey++; // Refresh the table
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error,
					life: 5000,
				});
			}
		},
	},
};
</script>

<style lang="scss" scoped>
.scope-selection-section {
	margin-top: 1.5rem;
	margin-bottom: 1rem;
	padding: 1rem;
	background-color: var(--surface-ground);
	border-radius: 8px;
}

.current-scope-display {
	padding: 0.5rem 1rem;
	background-color: var(--surface-card);
	border-radius: 4px;
	border-left: 3px solid var(--primary-color);
}

.resource-select-prompt {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	gap: 1rem;
	padding: 3rem;
	text-align: center;
	background-color: var(--surface-ground);
	border-radius: 8px;
	color: var(--text-color-secondary);
}
</style>
