<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Prompts</h2>
				<div class="page-subheader">The following agent and tool prompts are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/prompts/create" tabindex="-1">
					<Button aria-label="Create prompt">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Prompt
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading prompts">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:value="prompts"
				:globalFilterFields="['resource.name', 'resource.description']"
				v-model:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				striped-rows
				scrollable
				:multi-sort-meta="sortingFields"
				:sort-order="1"
				sort-mode="multiple"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search prompts" />
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="getPrompts"
						/>
					</div>
				</template>

				<template #empty>
					<div role="alert" aria-live="polite">No prompts found.</div>
				</template>
				<template #loading>Loading agent prompts. Please wait.</template>

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

				<!-- Category -->
				<Column
					field="resource.category"
					header="Category"
					header-style="width:6rem"
					sortable
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
						<template v-if="data.actions.includes('FoundationaLLM.Prompt/prompts/write')">
							<NuxtLink :to="'/prompts/edit/' + data.resource.name" class="table__button">
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
				<template #groupheader="slotProps">
					<div class="flex items-center gap-2">
						<span class="pi pi-book" aria-hidden="true"></span>
						<span>&nbsp;&nbsp;Category: {{ slotProps.data.resource.category ?? 'None' }}</span>
					</div>
				</template>
			</DataTable>
		</div>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Prompt, ResourceProviderGetResult } from '@/js/types';
import { useListFilterStore } from '@/stores/listFilterStore';

const FILTER_KEY = 'prompts';

export default {
	name: 'Prompts',

	data() {
		return {
			prompts: [] as ResourceProviderGetResult<Prompt>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
			sortingFields: [
				// { field: 'resource.category', order: 2 },
				{ field: 'resource.name', order: 1 },
			],
			accessControlModalOpen: false,
		};
	},

	async created() {
		// Restore filter from store
		const listFilterStore = useListFilterStore();
		const savedFilter = listFilterStore.getFilter(FILTER_KEY);
		if (savedFilter) {
			this.filters.global.value = savedFilter;
		}

		await this.getPrompts();
	},

	beforeUnmount() {
		const listFilterStore = useListFilterStore();
		listFilterStore.setFilter(FILTER_KEY, this.filters.global.value);
	},

	methods: {
		async getPrompts() {
			this.loading = true;
			try {
				this.prompts = (await api.getPrompts()) || [];
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
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
