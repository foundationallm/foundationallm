<template>
	<div :class="{ 'grid--loading': loading }" style="overflow: auto">
		<!-- Loading overlay -->
		<template v-if="loading">
			<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading agents">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>

		<!-- Table -->
		<DataTable
			:globalFilterFields="['resource.name', 'resource.description']"
			v-model:filters="filters"
			filterDisplay="menu"
			paginator
			:rows="10"
			:rowsPerPageOptions="[10, 25, 50, 100]"
			:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
			:value="agents"
			striped-rows
			scrollable
			sort-field="resource.name"
			:sort-order="1"
			table-style="max-width: 100%"
			size="small"
		>
			<template #header>
				<div class="w-full flex justify-between">
					<TableSearch v-model="filters" placeholder="Search agents" />
					<Button
						type="button"
						icon="pi pi-refresh"
						@click="$emit('refresh-agents')"
					/>
				</div>
			</template>
	
			<template #empty>
				<div role="alert" aria-live="polite">
					No agents found. Please use the menu on the left to create a new agent.
				</div>
			</template>
			<template #loading>Loading agent data. Please wait.</template>

			<!-- Name -->
			<Column
				field="resource.name"
				header="Name"
				sortable
				:style="columnStyle"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<span
						>{{ data.resource.name }}
						{{ data.resource.display_name ? `(${data.resource.display_name})` : '' }}</span
					>
					<template v-if="data.resource.properties?.default_resource === 'true'">
						<Chip label="Default" icon="pi pi-star" style="margin-left: 8px" />
					</template>
				</template>
			</Column>

			<!-- Description -->
			<Column
				field="resource.description"
				header="Description"
				sortable
				:style="columnStyle"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			></Column>

			<!-- Expiration -->
			<Column
				field="resource.expiration_date"
				header="Expiration Date"
				sortable
				:style="columnStyle"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<span>{{ $filters.formatDate(data.resource.expiration_date) }}</span>
				</template>
			</Column>

			<!-- Edit -->
			<Column
				header="Edit"
				header-style="width:6rem"
				style="text-align: center"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					headerContent: { style: { justifyContent: 'center' } },
				}"
			>
				<template #body="{ data }">
					<template v-if="data.actions.includes('FoundationaLLM.Agent/agents/write')">
						<NuxtLink :to="'/agents/edit/' + data.resource.name" class="table__button">
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button link :aria-label="`Edit ${data.resource.name}`">
									<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
								</Button>
								<template #popper
									><div role="tooltip">
										Edit {{ data.resource.display_name || data.resource.name }}
									</div></template
								>
							</VTooltip>
						</NuxtLink>
					</template>
					<template v-else>
						<span aria-disabled="true" class="table__button" style="opacity:.6; cursor: default;">
							<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
						</span>
					</template>
				</template>
			</Column>

			<!-- Delete -->
			<Column
				header="Delete"
				header-style="width:6rem"
				style="text-align: center"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					headerContent: { style: { justifyContent: 'center' } },
				}"
			>
				<template #body="{ data }">
					<template v-if="data.actions.includes('FoundationaLLM.Agent/agents/delete')">
						<VTooltip :auto-hide="false" :popper-triggers="['hover']">
							<Button link :aria-label="`Delete ${data.resource.name}`" @click="agentToDelete = data.resource">
								<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
							<template #popper
								><div role="tooltip">
									Delete {{ data.resource.display_name || data.resource.name }}
								</div></template
							>
						</VTooltip>
					</template>
					<template v-else>
						<span aria-disabled="true" style="opacity:.6; cursor: default;">
							<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
						</span>
					</template>
				</template>
			</Column>

			<!-- Set Default -->
			<Column
				header="Set Default"
				header-style="width:6rem"
				style="text-align: center"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					headerContent: { style: { justifyContent: 'center' } },
				}"
			>
				<template #body="{ data }">
					<template v-if="data.roles.includes('User Access Administrator')">
						<VTooltip :auto-hide="false" :popper-triggers="['hover']">
							<Button link :aria-label="`Set ${data.resource.name} as default`" @click="agentToSetAsDefault = data.resource">
								<i class="pi pi-star" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
							<template #popper
								><div role="tooltip">
									Set {{ data.resource.display_name || data.resource.name }} as default agent
								</div></template
							>
						</VTooltip>
					</template>
					<template v-else>
						<span aria-disabled="true" style="opacity:.6; cursor: default;">
							<i class="pi pi-star" style="font-size: 1.2rem" aria-hidden="true"></i>
						</span>
					</template>
				</template>
			</Column>
		</DataTable>

	<!-- Delete agent dialog -->
	<ConfirmationDialog
		v-if="agentToDelete !== null"
		:visible="agentToDelete !== null"
		header="Delete Agent"
		confirmText="Yes"
		cancelText="Cancel"
		confirm-button-severity="danger"
		@cancel="agentToDelete = null"
		@confirm="handleDeleteAgent"
	>
		<div>
			Are you sure you want to delete the agent "{{ agentToDelete!.name }}"?
		</div>
	</ConfirmationDialog>

	<!-- Set default agent dialog -->
	<ConfirmationDialog
		v-if="agentToSetAsDefault !== null"
		:visible="agentToSetAsDefault !== null"
		header="Set Default Agent"
		confirmText="Yes"
		cancelText="Cancel"
		@cancel="agentToSetAsDefault = null"
		@confirm="handleSetDefaultAgent"
	>
		<div>
			Are you sure you want to set the "{{ agentToSetAsDefault!.name }}" agent as default?
			<br />
			Default agents are automatically selected in the User Portal for new conversations.
		</div>
	</ConfirmationDialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Agent, ResourceProviderGetResult, ResourceProviderActionResult } from '@/js/types';

export default {
	name: 'AgentsList',

	props: {
		agents: {
			type: Array as () => ResourceProviderGetResult<Agent>[],
			required: true,
		},
		loading: {
			type: Boolean,
			required: true,
		},
		loadingStatusText: {
			type: String,
			required: true,
		},
	},

	emits: ['refresh-agents'],

	data() {
		return {
			agentToDelete: null as Agent | null,
			agentToSetAsDefault: null as Agent | null,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
		};
	},

	beforeUnmount() {
		// Clear filters when leaving the component
		this.filters.global.value = null;
	},

	computed: {
		columnStyle() {
			return window.innerWidth <= 768 ? {} : { minWidth: '200px' };
		},
	},

	methods: {
		async handleDeleteAgent() {
			try {
				await api.deleteAgent(this.agentToDelete!.name);
				this.agentToDelete = null;
				this.$emit('refresh-agents');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},

		async handleSetDefaultAgent() {
			try {
				const result: ResourceProviderActionResult = await api.setDefaultAgent(
					this.agentToSetAsDefault!.name,
				);
				if (result.isSuccessResult) {
					this.$toast.add({
						severity: 'success',
						detail: `Agent "${this.agentToSetAsDefault!.name}" set as default.`,
						life: 5000,
					});
				} else {
					this.$toast.add({
						severity: 'error',
						detail: 'Could not set the default agent. Please try again.',
						life: 5000,
					});
				}
				this.agentToSetAsDefault = null;
				this.$emit('refresh-agents');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},
	},
};
</script>

<style lang="scss" scoped>
.table__button {
	color: var(--primary-button-bg);
}

.grid--loading {
	pointer-events: none;
}

.grid__loading-overlay {
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
	pointer-events: none;
}

@media (max-width: 768px) {
	.p-column {
		min-width: auto !important;
		white-space: normal;
	}

	.p-datatable {
		font-size: 0.9rem;
	}

	.p-column-header-content {
		text-align: left;
	}
}
</style>
