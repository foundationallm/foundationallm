<template>
	<div>
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Quota Management</h2>
				<div class="page-subheader">Manage API rate limits and quotas for your instance.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/quotas/create">
					<Button>
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Quota
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite" aria-label="Loading quotas">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:globalFilterFields="['resource.name', 'resource.context', 'resource.quota_type']"
				:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:value="quotas"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search quotas" />
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="getQuotas"
						/>
					</div>
				</template>

				<template #empty>
					No quotas found. Click "Create Quota" to add a new rate limit.
				</template>

				<template #loading>Loading quotas. Please wait.</template>

				<!-- Name -->
				<Column
					field="resource.name"
					header="Name"
					sortable
					style="min-width: 180px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Context -->
				<Column
					field="resource.context"
					header="Context"
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
					field="resource.quota_type"
					header="Type"
					sortable
					style="min-width: 160px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						<Tag
							:severity="data.resource.quota_type === 'RawRequestRateLimit' ? 'info' : 'warning'"
							:pt="{
								root: {
									style: {
										backgroundColor: 'var(--primary-color)',
										color: 'var(--primary-text)',
										border: 'none',
									},
								},
							}"
						>
							{{ data.resource.quota_type }}
						</Tag>
					</template>
				</Column>

				<!-- Partition -->
				<Column
					field="resource.metric_partition"
					header="Partition"
					sortable
					style="min-width: 140px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Limit -->
				<Column
					field="resource.metric_limit"
					header="Limit"
					sortable
					style="min-width: 80px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						{{ data.resource.metric_limit }} / {{ data.resource.metric_window_seconds }}s
					</template>
				</Column>

				<!-- Lockout -->
				<Column
					field="resource.lockout_duration_seconds"
					header="Lockout"
					sortable
					style="min-width: 80px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						{{ data.resource.lockout_duration_seconds }}s
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
						<NuxtLink :to="'/quotas/edit/' + data.resource.name" class="table__button">
							<Button link :aria-label="`Edit ${data.resource.name}`">
								<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
						</NuxtLink>
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
						<Button link :aria-label="`Delete ${data.resource.name}`" @click="itemToDelete = data.resource">
							<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
						</Button>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Delete quota dialog -->
		<ConfirmationDialog
			v-if="itemToDelete !== null"
			:visible="itemToDelete !== null"
			header="Delete Quota"
			confirm-text="Yes"
			cancel-text="Cancel"
			confirm-button-severity="danger"
			@confirm="handleDelete"
			@cancel="itemToDelete = null"
			@update:visible="itemToDelete = null"
		>
			Do you want to delete the quota "{{ itemToDelete.name }}"?
		</ConfirmationDialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { QuotaDefinition } from '@/js/types';

export default {
	name: 'Quotas',

	data() {
		return {
			quotas: [] as QuotaDefinition[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
			itemToDelete: null as QuotaDefinition | null,
		};
	},

	async created() {
		await this.getQuotas();
	},

	beforeUnmount() {
		this.filters.global.value = null;
	},

	methods: {
		async getQuotas() {
			this.loading = true;
			try {
				this.quotas = await api.getQuotas();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDelete() {
			try {
				await api.deleteQuota(this.itemToDelete!.name);
				this.$toast.add({
					severity: 'success',
					detail: `Successfully deleted quota "${this.itemToDelete!.name}"!`,
					life: 5000,
				});
				this.itemToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getQuotas();
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
