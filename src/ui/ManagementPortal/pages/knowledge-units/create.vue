<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">
					{{ editKnowledgeUnit ? 'Edit Knowledge Unit' : 'Create New Knowledge Unit' }}
				</h2>
				<div class="page-subheader">
					{{
						editKnowledgeUnit
							? 'Edit your knowledge unit below.'
							: 'Complete the settings below to create a new knowledge unit.'
					}}
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="editKnowledgeUnit"
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
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="knowledgeUnit.name"
						:disabled="editKnowledgeUnit"
						type="text"
						class="w-full"
						placeholder="Enter knowledge unit name"
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
			<Button @click="handleCancel" severity="secondary" outlined>
				<i class="pi pi-times"></i>
				Cancel
			</Button>
			<Button @click="handleSave">
				<i class="pi pi-check"></i>
				{{ editKnowledgeUnit ? 'Update' : 'Create' }}
			</Button>
		</div>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
	name: 'CreateKnowledgeUnit',

	data() {
		return {
			knowledgeUnit: {
				name: '',
				description: '',
				type: 'knowledge-unit',
				object_id: ''
			},
			editKnowledgeUnit: false,
			loading: false,
			loadingStatusText: 'Loading...',
		};
	},

	async created() {
		// Check if we're editing an existing knowledge unit
		const knowledgeUnitName = this.$route.params.knowledgeUnitName;
		if (knowledgeUnitName) {
			this.editKnowledgeUnit = true;
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
			this.loadingStatusText = this.editKnowledgeUnit ? 'Updating knowledge unit...' : 'Creating knowledge unit...';
			
			try {
				await api.createOrUpdateKnowledgeUnit(this.knowledgeUnit.name, this.knowledgeUnit);
				this.$toast.add({
					severity: 'success',
					detail: `Knowledge unit ${this.editKnowledgeUnit ? 'updated' : 'created'} successfully.`,
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
	justify-content: flex-end;
}

.col-span-2 {
	grid-column: span 2;
}
</style>
