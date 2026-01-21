<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Vector Databases</h2>
				<div class="page-subheader">The following vector databases are available.</div>
			</div>

			<div v-if="hasContributorRole" style="display: flex; align-items: center">
				<NuxtLink to="/vector-databases/create" tabindex="-1">
					<Button aria-label="Create vector database">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Vector Database
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div
					class="grid__loading-overlay"
					role="status"
					aria-live="polite"
					aria-label="Loading vector databases"
				>
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:globalFilterFields="['resource.name']"
				v-model:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:value="vectorDatabases"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search vector databases" />
						<Button type="button" icon="pi pi-refresh" @click="getVectorDatabases" />
					</div>
				</template>

				<template #empty>
					<div role="alert" aria-live="polite">No vector databases found.</div>
				</template>

				<template #loading>Loading vector databases. Please wait.</template>

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

				<!-- Type -->
				<Column
					field="resource.database_type"
					header="Database Type"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

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
						<template v-if="data.actions.includes('FoundationaLLM.Vector/vectorDatabases/write')">
							<NuxtLink :to="'/vector-databases/edit/' + data.resource.name" class="table__button">
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
							<span
								aria-disabled="true"
								class="table__button"
								style="opacity: 0.6; cursor: default"
							>
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
						<template v-if="data.actions.includes('FoundationaLLM.Vector/vectorDatabases/delete')">
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button
									link
									:aria-label="`Delete ${data.resource.name}`"
									@click="vectorDatabaseToDelete = data.resource"
								>
									<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
								</Button>
								<template #popper
									><div role="tooltip">Delete {{ data.resource.name }}</div></template
								>
							</VTooltip>
						</template>
						<template v-else>
							<span aria-disabled="true" style="opacity: 0.6; cursor: default">
								<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
							</span>
						</template>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Delete dialog -->
		<ConfirmationDialog
			v-if="vectorDatabaseToDelete !== null"
			:visible="vectorDatabaseToDelete !== null"
			header="Delete Vector Database"
			confirm-text="Yes"
			cancel-text="Cancel"
			confirm-button-severity="danger"
			@confirm="handleDeleteVectorDatabase"
			@cancel="vectorDatabaseToDelete = null"
			@update:visible="vectorDatabaseToDelete = null"
		>
			Do you want to delete the vector database "{{ vectorDatabaseToDelete.name }}"?
		</ConfirmationDialog>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { VectorDatabase, ResourceProviderGetResult } from '@/js/types';
import { useListFilterStore } from '@/stores/listFilterStore';

const FILTER_KEY = 'vectorDatabases';

export default {
	name: 'VectorDatabases',

	data() {
		return {
			vectorDatabases: [] as ResourceProviderGetResult<VectorDatabase>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' },
			},
			vectorDatabaseToDelete: null as VectorDatabase | null,
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
			this.getVectorDatabases(),
			this.checkContributorRole(),
		]);
	},

	beforeUnmount() {
		// Save filter to store when leaving
		const listFilterStore = useListFilterStore();
		listFilterStore.setFilter(FILTER_KEY, this.filters.global.value);
	},

	methods: {
		async checkContributorRole() {
			this.hasContributorRole = await api.hasContributorRole('Vector Databases Contributor');
		},

		async getVectorDatabases() {
			this.loading = true;
			try {
				this.vectorDatabases = await api.getVectorDatabases();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteVectorDatabase() {
			try {
				await api.deleteVectorDatabase(this.vectorDatabaseToDelete!.name);
				this.vectorDatabaseToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getVectorDatabases();
		},
	},
};
</script>

<style lang="scss" scoped>
.table__button {
	color: var(--primary-button-bg);
}

.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
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
