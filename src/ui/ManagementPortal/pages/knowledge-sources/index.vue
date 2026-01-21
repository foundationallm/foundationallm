<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Knowledge Sources</h2>
				<div class="page-subheader">The following knowledge sources are available.</div>
			</div>

			<div v-if="hasContributorRole" style="display: flex; align-items: center">
				<NuxtLink to="/knowledge-sources/create" tabindex="-1">
					<Button aria-label="Create knowledge source">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Knowledge Source
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading knowledge sources">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:value="knowledgeSources"
				:globalFilterFields="['resource.name', 'resource.description']"
				v-model:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search knowledge sources" />
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="getKnowledgeSources"
						/>
					</div>
				</template>

				<template #empty>
					<div role="alert" aria-live="polite">No knowledge sources found.</div>
				</template>
				<template #loading>Loading knowledge sources. Please wait.</template>

				<!-- Name -->
				<Column
					field="resource.name"
					header="Name"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Description -->
				<Column
					field="resource.description"
					header="Description"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Knowledge Units Count -->
				<Column
					header="Knowledge Units"
					sortable
					style="min-width: 150px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						{{ data.resource.knowledge_unit_object_ids?.length || 0 }}
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
						<template v-if="data.actions.includes('FoundationaLLM.Context/knowledgeSources/write')">
							<NuxtLink :to="'/knowledge-sources/edit/' + data.resource.name" class="table__button">
								<VTooltip :auto-hide="false" :popper-triggers="['hover']">
									<Button link :aria-label="`Edit ${data.resource.name}`">
										<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
									</Button>
									<template #popper
										><div role="tooltip">Edit {{ data.resource.name }}</div></template
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
						<template v-if="data.actions.includes('FoundationaLLM.Context/knowledgeSources/delete')">
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button link :aria-label="`Delete ${data.resource.name}`" @click="knowledgeSourceToDelete = data.resource">
									<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
								</Button>
								<template #popper
									><div role="tooltip">Delete {{ data.resource.name }}</div></template
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
			</DataTable>
		</div>

		<!-- Delete knowledge source dialog -->
		<ConfirmationDialog
			v-if="knowledgeSourceToDelete !== null"
			:visible="knowledgeSourceToDelete !== null"
			header="Delete Knowledge Source"
			confirm-text="Yes"
			cancel-text="Cancel"
			confirm-button-severity="danger"
			@confirm="handleDeleteKnowledgeSource"
			@cancel="knowledgeSourceToDelete = null"
			@update:visible="knowledgeSourceToDelete = null"
		>
			Do you want to delete the knowledge source "{{ knowledgeSourceToDelete.name }}"?
		</ConfirmationDialog>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { ResourceProviderGetResult } from '@/js/types';
import { useListFilterStore } from '@/stores/listFilterStore';

const FILTER_KEY = 'knowledgeSources';

export default {
	name: 'KnowledgeSources',

	data() {
		return {
			knowledgeSources: [] as ResourceProviderGetResult<any>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
			knowledgeSourceToDelete: null as any | null,
			hasContributorRole: false as boolean,
		};
	},

	async created() {
		// Restore filter from store
		const listFilterStore = useListFilterStore();
		const savedFilter = listFilterStore.getFilter(FILTER_KEY);
		if (savedFilter) {
			this.filters.global.value = savedFilter;
		}

		await Promise.all([
			this.getKnowledgeSources(),
			this.checkContributorRole(),
		]);
	},

	beforeUnmount() {
		const listFilterStore = useListFilterStore();
		listFilterStore.setFilter(FILTER_KEY, this.filters.global.value);
	},

	methods: {
		async checkContributorRole() {
			this.hasContributorRole = await api.hasContributorRole('Knowledge Sources Contributor');
		},

		async getKnowledgeSources() {
			this.loading = true;
			try {
				this.knowledgeSources = (await api.getKnowledgeSources()) || [];
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteKnowledgeSource() {
			try {
				await api.deleteKnowledgeSource(this.knowledgeSourceToDelete!.name);
				this.knowledgeSourceToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getKnowledgeSources();
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
</style>
