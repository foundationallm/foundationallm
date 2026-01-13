<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">Edit Knowledge Unit</h2>
				<div class="page-subheader">
					Edit your knowledge unit below.
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="knowledgeUnit.name"
				:scopes="[
					{
						label: 'Knowledge Unit',
						value: `providers/FoundationaLLM.Context/knowledgeUnits/${knowledgeUnit.name}`,
					},
				]"
			/>
		</div>

		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite" aria-label="Loading knowledge unit form">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="col-span-2">
				<div id="aria-knowledge-unit-name" class="step-header !mb-2">Knowledge Unit name:</div>
				<div id="aria-knowledge-unit-name-desc" class="mb-2">
					The name cannot be changed after creation.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="knowledgeUnit.name"
						disabled
						type="text"
						class="w-full"
						placeholder="Knowledge unit name"
						aria-labelledby="aria-knowledge-unit-name aria-knowledge-unit-name-desc"
					/>
				</div>
			</div>
			<div class="col-span-2">
				<div class="step-header !mb-2">Description:</div>
				<div id="aria-description" class="mb-2">
					Provide a description to help others understand the knowledge unit's purpose.
				</div>
				<InputText
					v-model="knowledgeUnit.description"
					type="text"
					class="w-full"
					placeholder="Enter description"
					aria-labelledby="aria-description"
				/>
			</div>
		</div>

		<!-- Actions -->
		<div class="actions">
			<Button @click="showDeleteConfirmation" severity="danger" outlined v-if="canDelete">
				<i class="pi pi-trash"></i>
				Delete
			</Button>
			<div style="flex: 1"></div>
			<Button @click="handleCancel" severity="secondary" outlined>
				<i class="pi pi-times"></i>
				Cancel
			</Button>
			<Button @click="handleSave">
				<i class="pi pi-check"></i>
				Update
			</Button>
		</div>

		<!-- Delete confirmation dialog -->
		<ConfirmationDialog
			:visible="deleteConfirmationVisible"
			@confirm="handleDelete"
			@cancel="deleteConfirmationVisible = false"
			@update:visible="deleteConfirmationVisible = false"
		>
			Do you want to delete the knowledge unit "{{ knowledgeUnit.name }}"?
		</ConfirmationDialog>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
	name: 'EditKnowledgeUnit',

	data() {
		return {
			knowledgeUnit: {
				name: '',
				description: '',
				type: 'knowledge-unit',
				object_id: ''
			},
			loading: false,
			loadingStatusText: 'Loading...',
			canDelete: false,
			deleteConfirmationVisible: false,
		};
	},

	async created() {
		const knowledgeUnitName = this.$route.params.knowledgeUnitName;
		if (knowledgeUnitName) {
			await this.loadKnowledgeUnit(knowledgeUnitName);
		}
	},

	methods: {
		async loadKnowledgeUnit(knowledgeUnitName: string) {
			this.loading = true;
			this.loadingStatusText = 'Loading knowledge unit...';
			try {
				const result = await api.getKnowledgeUnit(knowledgeUnitName);
				if (result && result.length > 0) {
					this.knowledgeUnit = result[0].resource;
					// Check if user has delete permissions
					this.canDelete = result[0].actions?.includes('FoundationaLLM.Context/knowledgeUnits/delete') || false;
				}
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleSave() {
			// Validate required fields
			if (!this.knowledgeUnit.name) {
				this.$toast.add({
					severity: 'warn',
					detail: 'Knowledge unit name is required.',
					life: 3000,
				});
				return;
			}

			this.loading = true;
			this.loadingStatusText = 'Updating knowledge unit...';
			
			try {
				await api.createOrUpdateKnowledgeUnit(this.knowledgeUnit.name, this.knowledgeUnit);
				this.$toast.add({
					severity: 'success',
					detail: 'Knowledge unit updated successfully.',
					life: 3000,
				});
				this.$router.push('/knowledge-units');
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
			this.loading = true;
			this.loadingStatusText = 'Deleting knowledge unit...';
			this.deleteConfirmationVisible = false;
			
			try {
				await api.deleteKnowledgeUnit(this.knowledgeUnit.name);
				this.$toast.add({
					severity: 'success',
					detail: 'Knowledge unit deleted successfully.',
					life: 3000,
				});
				this.$router.push('/knowledge-units');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		showDeleteConfirmation() {
			this.deleteConfirmationVisible = true;
		},

		handleCancel() {
			this.$router.push('/knowledge-units');
		},
	},
};
</script>

<style lang="scss" scoped>
.steps {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 1rem;
	margin-top: 2rem;
	position: relative;
}

.steps__loading-overlay {
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

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

.actions {
	display: flex;
	gap: 1rem;
	margin-top: 2rem;
}

.col-span-2 {
	grid-column: span 2;
}
</style>
