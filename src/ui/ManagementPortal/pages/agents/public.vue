<template>
	<div>
		<h2 class="page-header">Public Agents</h2>
		<div class="page-subheader">View your publicly accessible agents.</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable :value="agents" stripedRows scrollable tableStyle="max-width: 100%" size="small">
				<Column field="name" header="Name" sortable style="min-width: 200px" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, sortIcon: { style: { color: 'var(--primary-text)' } } }"></Column>
				<Column field="type" header="Type" sortable style="min-width: 200px" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, sortIcon: { style: { color: 'var(--primary-text)' } } }"></Column>
				<Column header="Edit" headerStyle="width:6rem" style="text-align: center" :pt="{ headerCell: { style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' } }, headerContent: { style: { justifyContent: 'center' } } }">
					<template #body="{ data }">
						<NuxtLink :to="'/agents/edit/' + data.name" class="table__button">
							<i class="pi pi-cog" style="font-size: 1.5rem"></i>
						</NuxtLink>
					</template>
				</Column>
			</DataTable>
		</div>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type Agent from '@/js/types';

export default {
	name: 'PublicAgents',

	data() {
		return {
			agents: [] as Agent,
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
		};
	},

	async created() {
		this.loading = true;
		try {
			this.agents = await api.getAgents();
		} catch(error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
			});
		}
		this.loading = false;
	},
};
</script>

<style lang="scss">
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
	position: absolute;
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
