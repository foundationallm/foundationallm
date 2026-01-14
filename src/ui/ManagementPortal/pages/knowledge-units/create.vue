<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">
					{{ editId ? 'Edit Knowledge Unit' : 'Create Knowledge Unit' }}
				</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your knowledge unit below.'
							: 'Complete the settings below to create a new knowledge unit.'
					}}
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="editId"
				:scopes="[
					{
						label: 'Knowledge Unit',
						value: `providers/FoundationaLLM.Context/knowledgeUnits/${editId}`,
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
					{{ editId ? 'The name cannot be changed after creation.' : 'No special characters or spaces, use letters and numbers with dashes and underscores only.' }}
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="knowledgeUnit.name"
						:disabled="!!editId"
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

			<!-- Vector Database -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Vector Database:</div>
				<div id="aria-vector-database" class="mb-2">
					Select the vector database associated with this knowledge unit.
				</div>
				<Dropdown
					v-model="knowledgeUnit.vector_database_object_id"
					:options="vectorDatabaseOptions"
					option-label="name"
					option-value="object_id"
					placeholder="Select a vector database"
					class="w-full"
					aria-labelledby="aria-vector-database"
				/>
			</div>

			<!-- Vector Store ID -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Vector Store ID:</div>
				<div id="aria-vector-store-id" class="mb-2">
					The identifier of the vector store within the selected vector database. If not specified, queries must provide this explicitly.
				</div>
				<InputText
					v-model="knowledgeUnit.vector_store_id"
					type="text"
					class="w-full"
					placeholder="Enter vector store ID (optional)"
					aria-labelledby="aria-vector-store-id"
				/>
			</div>

			<!-- Has Knowledge Graph -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Has Knowledge Graph:</div>
				<div id="aria-has-knowledge-graph" class="mb-2">
					Enable if this knowledge unit also has an associated knowledge graph.
				</div>
				<InputSwitch
					v-model="knowledgeUnit.has_knowledge_graph"
					aria-labelledby="aria-has-knowledge-graph"
				/>
			</div>

			<!-- Knowledge Graph Vector Database (shown when has_knowledge_graph is true) -->
			<div v-if="knowledgeUnit.has_knowledge_graph" class="col-span-2">
				<div class="step-header !mb-2">Knowledge Graph Vector Database:</div>
				<div id="aria-kg-vector-database" class="mb-2">
					Select the vector database used to store knowledge graph embeddings.
				</div>
				<Dropdown
					v-model="knowledgeUnit.knowledge_graph_vector_database_object_id"
					:options="vectorDatabaseOptions"
					option-label="name"
					option-value="object_id"
					placeholder="Select a vector database for knowledge graph"
					class="w-full"
					aria-labelledby="aria-kg-vector-database"
				/>
			</div>
		</div>

		<!-- Buttons -->
		<div class="flex justify-end gap-4 mt-6">
			<!-- Create/Save knowledge unit -->
			<Button
				:label="editId ? 'Save Changes' : 'Create Knowledge Unit'"
				severity="primary"
				@click="handleSave"
			/>

			<!-- Cancel -->
			<Button label="Cancel" severity="secondary" @click="handleCancel" />
		</div>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';

export default {
	name: 'CreateKnowledgeUnit',

	props: {
		editId: {
			type: String,
			default: null,
		},
	},

	data() {
		return {
			knowledgeUnit: {
				name: '',
				description: '',
				type: 'knowledge-unit',
				object_id: '',
				vector_database_object_id: '',
				vector_store_id: '',
				has_knowledge_graph: false,
				knowledge_graph_vector_database_object_id: '',
			},
			vectorDatabaseOptions: [] as { name: string; object_id: string }[],
			loading: false,
			loadingStatusText: 'Loading...',
		};
	},

	async created() {
		if (this.editId) {
			await this.loadKnowledgeUnit(this.editId);
		} else {
			await this.loadVectorDatabases();
		}
	},

	methods: {
		async loadVectorDatabases() {
			this.loading = true;
			this.loadingStatusText = 'Loading vector databases...';
			try {
				const vectorDbResult = await api.getVectorDatabases();
				if (vectorDbResult && Array.isArray(vectorDbResult)) {
					this.vectorDatabaseOptions = vectorDbResult
						.map((item: any) => ({
							name: item.resource?.name || item.name || 'Unknown',
							object_id: item.resource?.object_id || item.object_id || '',
						}))
						.sort((a, b) => a.name.localeCompare(b.name));
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

		async loadKnowledgeUnit(knowledgeUnitName: string) {
			this.loading = true;
			this.loadingStatusText = 'Loading knowledge unit...';
			try {
				// Load vector databases and knowledge unit in parallel
				const [vectorDbResult, knowledgeUnitResult] = await Promise.all([
					api.getVectorDatabases(),
					api.getKnowledgeUnit(knowledgeUnitName),
				]);

				// Process vector database options
				if (vectorDbResult && Array.isArray(vectorDbResult)) {
					this.vectorDatabaseOptions = vectorDbResult
						.map((item: any) => ({
							name: item.resource?.name || item.name || 'Unknown',
							object_id: item.resource?.object_id || item.object_id || '',
						}))
						.sort((a, b) => a.name.localeCompare(b.name));
				}

				// Process knowledge unit
				if (knowledgeUnitResult && knowledgeUnitResult.length > 0) {
					this.knowledgeUnit = {
						...this.knowledgeUnit,
						...knowledgeUnitResult[0].resource,
					};
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

			if (!this.knowledgeUnit.vector_database_object_id) {
				this.$toast.add({
					severity: 'warn',
					detail: 'Vector database is required.',
					life: 3000,
				});
				return;
			}

			// Clear knowledge graph vector database if not using knowledge graph
			const knowledgeUnitToSave = { ...this.knowledgeUnit };
			if (!knowledgeUnitToSave.has_knowledge_graph) {
				knowledgeUnitToSave.knowledge_graph_vector_database_object_id = null;
			}

			this.loading = true;
			this.loadingStatusText = this.editId ? 'Updating knowledge unit...' : 'Creating knowledge unit...';
			
			try {
				await api.createOrUpdateKnowledgeUnit(this.knowledgeUnit.name, knowledgeUnitToSave);
				this.$toast.add({
					severity: 'success',
					detail: this.editId ? 'Knowledge unit updated successfully.' : 'Knowledge unit created successfully.',
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

.col-span-2 {
	grid-column: span 2;
}
</style>
