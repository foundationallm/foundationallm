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
						@input="handleNameInput"
					/>
					<span
						v-if="!editId && nameValidationStatus"
						class="ml-2"
						:title="validationMessage"
						style="font-size: 1.2rem"
						:aria-label="validationMessage"
					>
						<i v-if="nameValidationStatus === 'valid'" class="pi pi-check" style="color: green"></i>
						<i v-else-if="nameValidationStatus === 'invalid'" class="pi pi-times" style="color: red"></i>
					</span>
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
					@change="handleVectorDatabaseChange"
				/>
			</div>

			<!-- Vector Store ID -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Vector Store ID:</div>
				<div id="aria-vector-store-id" class="mb-2">
					The identifier of the vector store within the selected vector database. If not specified, queries must provide this explicitly.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="knowledgeUnit.vector_store_id"
						type="text"
						class="w-full"
						placeholder="Enter vector store ID (optional)"
						aria-labelledby="aria-vector-store-id"
						@input="handleVectorStoreIdInput"
					/>
					<span
						v-if="vectorStoreIdValidationStatus"
						class="ml-2"
						:title="vectorStoreIdValidationMessage"
						style="font-size: 1.2rem"
						:aria-label="vectorStoreIdValidationMessage"
					>
						<i v-if="vectorStoreIdValidationStatus === 'valid'" class="pi pi-check" style="color: green"></i>
						<i v-else-if="vectorStoreIdValidationStatus === 'invalid'" class="pi pi-times" style="color: red"></i>
					</span>
				</div>
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
import { debounce } from 'lodash';

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
			nameValidationStatus: null as 'valid' | 'invalid' | null,
			validationMessage: null as string | null,
			debouncedCheckName: null as any,
			vectorStoreIdValidationStatus: null as 'valid' | 'invalid' | null,
			vectorStoreIdValidationMessage: null as string | null,
			debouncedCheckVectorStoreId: null as any,
		};
	},

	async created() {
		if (this.editId) {
			await this.loadKnowledgeUnit(this.editId);
		} else {
			await this.loadVectorDatabases();
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
		this.debouncedCheckVectorStoreId = debounce(this.checkVectorStoreId, 500);
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkKnowledgeUnitName(this.knowledgeUnit.name);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.error_message;
				}
			} catch (error) {
				console.error('Error checking knowledge unit name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the knowledge unit name. Please try again.';
			}
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.knowledgeUnit.name = sanitizedValue;

			// Check if the name is available if we are creating a new knowledge unit.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async checkVectorStoreId() {
			// Only validate if both vector database and vector store ID are provided
			if (!this.knowledgeUnit.vector_database_object_id || !this.knowledgeUnit.vector_store_id) {
				this.vectorStoreIdValidationStatus = null;
				this.vectorStoreIdValidationMessage = null;
				return;
			}

			try {
				const response = await api.checkVectorStoreId(
					this.knowledgeUnit.vector_database_object_id,
					this.knowledgeUnit.vector_store_id
				);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Vector store ID is valid
					this.vectorStoreIdValidationStatus = 'valid';
					this.vectorStoreIdValidationMessage = null;
				} else if (response.status === 'Denied') {
					// Vector store ID is invalid
					this.vectorStoreIdValidationStatus = 'invalid';
					this.vectorStoreIdValidationMessage = response.error_message;
				}
			} catch (error) {
				console.error('Error checking vector store ID: ', error);
				this.vectorStoreIdValidationStatus = 'invalid';
				this.vectorStoreIdValidationMessage = 'Error checking the vector store ID. Please try again.';
			}
		},

		handleVectorStoreIdInput() {
			// Reset validation status if vector store ID is cleared
			if (!this.knowledgeUnit.vector_store_id) {
				this.vectorStoreIdValidationStatus = null;
				this.vectorStoreIdValidationMessage = null;
				return;
			}

			// Check if the vector store ID is valid
			this.debouncedCheckVectorStoreId();
		},

		handleVectorDatabaseChange() {
			// Re-validate vector store ID when vector database changes
			if (this.knowledgeUnit.vector_store_id) {
				this.debouncedCheckVectorStoreId();
			}
		},

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

			if (!this.editId && this.nameValidationStatus === 'invalid') {
				this.$toast.add({
					severity: 'warn',
					detail: this.validationMessage || 'The knowledge unit name is not available.',
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

			if (this.vectorStoreIdValidationStatus === 'invalid') {
				this.$toast.add({
					severity: 'warn',
					detail: this.vectorStoreIdValidationMessage || 'The vector store ID is not valid.',
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
