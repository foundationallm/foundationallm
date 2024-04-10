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
			<DataTable :value="agents" striped-rows scrollable table-style="max-width: 100%" size="small">
				<template #empty>
					No agents found. Please use the menu on the left to create a new agent.</template
				>
				<template #loading>Loading agent data. Please wait.</template>

				<!-- Name -->
				<Column
					field="name"
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
					field="type"
					header="Type"
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
						<NuxtLink :to="'/agents/edit/' + data.name" class="table__button">
							<Button link>
								<i class="pi pi-cog" style="font-size: 1.2rem"></i>
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
						<Button link @click="agentToDelete = data">
							<i class="pi pi-trash" style="font-size: 1.2rem; color: var(--red-400)"></i>
						</Button>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Delete agent dialog -->
		<Dialog :visible="agentToDelete !== null" modal header="Delete Agent" :closable="false">
			<p>Do you want to delete the agent "{{ agentToDelete.name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="agentToDelete = null" />
				<Button label="Delete" severity="danger" @click="handleDeleteAgent" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Agent } from '@/js/types';

export default {
	name: 'PublicAgents',

	data() {
		return {
			agents: [] as Agent,
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			agentToDelete: null as Agent | null,
		};
	},

	async created() {
		await this.getAgents();
	},

	methods: {
		async getAgents() {
			this.loading = true;
			try {
				this.agents = await api.getAgents();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteAgent() {
			try {
				await api.deleteAgent(this.agentToDelete!.name);
				this.agentToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getAgents();
		},
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
