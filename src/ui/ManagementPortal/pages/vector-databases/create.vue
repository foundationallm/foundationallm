<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">
					{{ editId ? 'Edit Vector Database' : 'Create Vector Database' }}
				</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your vector database settings below.'
							: 'Complete the settings below to configure the vector database.'
					}}
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="editId"
				:scopes="[
					{
						label: 'Vector Database',
						value: `providers/FoundationaLLM.Vector/vectorDatabases/${editId}`,
					},
				]"
			/>
		</div>

		<!-- Steps -->
		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div
					class="steps__loading-overlay"
					role="status"
					aria-live="polite"
					aria-label="Loading vector database form"
				>
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header col-span-2">What is the name of the vector database?</div>
			<div class="col-span-2">
				<div id="aria-db-name" class="mb-2">Vector database name:</div>
				<div id="aria-db-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="vectorDatabase.name"
						:disabled="editId"
						type="text"
						class="w-full"
						placeholder="Enter vector database name"
						aria-labelledby="aria-db-name aria-db-name-desc"
						@input="handleNameInput"
					/>
					<span
						v-if="nameValidationStatus === 'valid'"
						class="icon valid"
						title="Name is available"
						aria-label="Name is available"
					>
						✔️
					</span>
					<span
						v-else-if="nameValidationStatus === 'invalid'"
						:title="validationMessage"
						class="icon invalid"
						:aria-label="validationMessage"
					>
						❌
					</span>
				</div>

				<div id="aria-db-desc" class="mb-2 mt-2">Description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="vectorDatabase.description"
						type="text"
						class="w-full"
						placeholder="Enter a description for this vector database"
						aria-labelledby="aria-db-desc"
					/>
				</div>
			</div>

			<!-- Database Type -->
			<div id="aria-db-type" class="step-header col-span-2">
				What is the type of the vector database?
			</div>
			<div class="col-span-2">
				<Dropdown
					v-model="vectorDatabase.database_type"
					:options="databaseTypeOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-db-type"
				/>
			</div>

			<!-- Database Name -->
			<div id="aria-database-name" class="step-header col-span-2">
				What is the database name in the vector database service?
			</div>
			<div class="col-span-2">
				<InputText
					v-model="vectorDatabase.database_name"
					type="text"
					class="w-full"
					placeholder="Enter database name"
					aria-labelledby="aria-database-name"
				/>
			</div>

			<!-- API Endpoint Configuration -->
			<div id="aria-api-endpoint" class="step-header col-span-2">
				What is the API endpoint configuration for this vector database?
			</div>
			<div class="col-span-2">
				<Dropdown
					v-model="vectorDatabase.api_endpoint_configuration_object_id"
					:options="apiEndpointOptions"
					option-label="name"
					option-value="object_id"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-api-endpoint"
				/>
			</div>

			<!-- Embedding Model -->
			<div id="aria-embedding-model" class="step-header col-span-2">
				What is the embedding model?
			</div>
			<div class="col-span-2">
				<InputText
					v-model="vectorDatabase.embedding_model"
					type="text"
					class="w-full"
					placeholder="Enter embedding model"
					aria-labelledby="aria-embedding-model"
				/>
			</div>

			<!-- Embedding Dimensions -->
			<div id="aria-embedding-dimensions" class="step-header col-span-2">
				How many dimensions do the embeddings have?
			</div>
			<div class="col-span-2">
				<InputNumber
					v-model="vectorDatabase.embedding_dimensions"
					class="w-full"
					placeholder="Enter embedding dimensions"
					aria-labelledby="aria-embedding-dimensions"
					:min="1"
				/>
			</div>

			<!-- Property Names -->
			<div class="step-header col-span-2">What are the property names in the vector database?</div>
			<div class="col-span-2">
				<div id="aria-embedding-prop" class="mb-2">Embedding property name:</div>
				<InputText
					v-model="vectorDatabase.embedding_property_name"
					type="text"
					class="w-full mb-2"
					placeholder="Enter embedding property name"
					aria-labelledby="aria-embedding-prop"
				/>

				<div id="aria-content-prop" class="mb-2">Content property name:</div>
				<InputText
					v-model="vectorDatabase.content_property_name"
					type="text"
					class="w-full mb-2"
					placeholder="Enter content property name"
					aria-labelledby="aria-content-prop"
				/>

				<div id="aria-vector-store-id-prop" class="mb-2">Vector store ID property name:</div>
				<InputText
					v-model="vectorDatabase.vector_store_id_property_name"
					type="text"
					class="w-full mb-2"
					placeholder="Enter vector store ID property name"
					aria-labelledby="aria-vector-store-id-prop"
				/>

				<div id="aria-metadata-prop" class="mb-2">Metadata property name:</div>
				<InputText
					v-model="vectorDatabase.metadata_property_name"
					type="text"
					class="w-full mb-2"
					placeholder="Enter metadata property name"
					aria-labelledby="aria-metadata-prop"
				/>

				<div id="aria-metadata-props" class="mb-2">
					Metadata properties (comma-separated name|type pairs):
				</div>
				<div id="aria-metadata-props-desc" class="mb-2">
					Enter metadata properties as comma-separated name|type pairs, e.g.,
					"title|Edm.String,timestamp|Edm.DateTimeOffset"
				</div>
				<Textarea
					v-model="vectorDatabase.metadata_properties"
					class="w-full"
					placeholder="e.g., title|Edm.String,timestamp|Edm.DateTimeOffset"
					aria-labelledby="aria-metadata-props aria-metadata-props-desc"
					rows="3"
				/>
			</div>

			<div id="aria-cost-center" class="step-header col-span-2">
				Would you like to assign this vector database to a cost center?
			</div>
			<div class="col-span-2">
				<InputText
					v-model="vectorDatabase.cost_center"
					type="text"
					class="w-50"
					placeholder="Enter cost center name"
					aria-labelledby="aria-cost-center"
				/>
			</div>

			<!-- Buttons -->
			<div class="flex col-span-2 justify-end gap-4">
				<!-- Create/Save vector database -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Vector Database'"
					severity="primary"
					@click="handleCreateVectorDatabase"
				/>

				<!-- Cancel -->
				<Button label="Cancel" severity="secondary" @click="handleCancel" />
			</div>
		</div>
	</main>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';
import { useConfirmationStore } from '@/stores/confirmationStore';
import type { VectorDatabase, VectorDatabaseType } from '@/js/types';

export default {
	name: 'CreateVectorDatabase',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: null as string | null,
			debouncedCheckName: null as any,

			vectorDatabase: {
				type: 'vector-database',
				name: '',
				display_name: '',
				object_id: '',
				description: '',
				cost_center: '',
				database_type: 'AzureAISearch' as VectorDatabaseType,
				database_name: '',
				embedding_model: 'text-embedding-3-large',
				embedding_dimensions: 2048,
				embedding_property_name: 'Embedding',
				content_property_name: 'Text',
				vector_store_id_property_name: 'VectorStoreId',
				metadata_property_name: 'Metadata',
				metadata_properties: 'FileId|Edm.String,FileName|Edm.String',
				api_endpoint_configuration_object_id: '',
			} as VectorDatabase,

			databaseTypeOptions: [
				{
					label: 'Azure AI Search',
					value: 'AzureAISearch',
				},
				{
					label: 'Azure Cosmos DB NoSQL',
					value: 'AzureCosmosDBNoSQL',
				},
				{
					label: 'Azure PostgreSQL',
					value: 'AzurePostgreSQL',
				},
			],

			apiEndpointOptions: [] as any[],
		};
	},

	async created() {
		this.loading = true;

		// Load API endpoints
		try {
			this.loadingStatusText = 'Retrieving API endpoints...';
			const endpoints = await api.filterAPIEndpointConfigurations({
				category: 'General',
				subcategory: 'Indexing',
			});
			this.apiEndpointOptions = endpoints.sort((a, b) => a.name.localeCompare(b.name));
		} catch (error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
				life: 5000,
			});
		}

		if (this.editId) {
			this.loadingStatusText = `Retrieving vector database "${this.editId}"...`;
			try {
				const vectorDatabaseResult = await api.getVectorDatabase(this.editId);
				const vectorDatabase = vectorDatabaseResult?.resource;
				if (!vectorDatabase) {
					throw new Error('Vector database not found');
				}
				this.vectorDatabase = {
					...this.vectorDatabase,
					...vectorDatabase,
				};
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: `Failed to load vector database "${this.editId}": ${error?.response?._data || error?.message || error}`,
					life: 5000,
				});
				this.$router.push('/vector-databases');
				return;
			}
		}

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkVectorDatabaseName(this.vectorDatabase.name);

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
				console.error('Error checking vector database name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the vector database name. Please try again.';
			}
		},

		async handleCancel() {
			const confirmationStore = useConfirmationStore();
			const confirmed = await confirmationStore.confirmAsync({
				title: this.editId ? 'Cancel Vector Database Edit' : 'Cancel Vector Database Creation',
				message: 'Are you sure you want to cancel?',
				confirmText: 'Yes',
				cancelText: 'Cancel',
				confirmButtonSeverity: 'danger',
			});

			if (confirmed) {
				this.$router.push('/vector-databases');
			}
		},

		handleNameInput(event: Event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.vectorDatabase.name = sanitizedValue;

			// Check if the name is available if we are creating a new vector database.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async handleCreateVectorDatabase() {
			const errors: string[] = [];
			if (!this.vectorDatabase.name) {
				errors.push('Please give the vector database a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage || 'The vector database name is not available.');
			}

			if (!this.vectorDatabase.database_type) {
				errors.push('Please specify a database type.');
			}

			if (!this.vectorDatabase.database_name) {
				errors.push('Please specify a database name.');
			}

			if (!this.vectorDatabase.api_endpoint_configuration_object_id) {
				errors.push('Please select an API endpoint configuration.');
			}

			if (!this.vectorDatabase.embedding_model) {
				errors.push('Please specify an embedding model.');
			}

			if (
				!this.vectorDatabase.embedding_dimensions ||
				this.vectorDatabase.embedding_dimensions < 1
			) {
				errors.push('Please specify valid embedding dimensions.');
			}

			if (!this.vectorDatabase.embedding_property_name) {
				errors.push('Please specify an embedding property name.');
			}

			if (!this.vectorDatabase.content_property_name) {
				errors.push('Please specify a content property name.');
			}

			if (!this.vectorDatabase.vector_store_id_property_name) {
				errors.push('Please specify a vector store ID property name.');
			}

			if (!this.vectorDatabase.metadata_property_name) {
				errors.push('Please specify a metadata property name.');
			}

			if (!this.vectorDatabase.metadata_properties) {
				errors.push('Please specify metadata properties.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving vector database...';
				await api.upsertVectorDatabase(this.vectorDatabase);
				successMessage = `Vector database "${this.vectorDatabase.name}" was successfully saved.`;
			} catch (error) {
				this.loading = false;
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
				life: 5000,
			});

			this.loading = false;

			if (!this.editId) {
				this.$router.push('/vector-databases');
			}
		},
	},
};
</script>

<style lang="scss">
.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.steps__loading-overlay {
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

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

input {
	width: 100%;
	padding-right: 30px;
}

.icon {
	position: absolute;
	right: 10px;
	cursor: default;
}

.valid {
	color: green;
}

.invalid {
	color: red;
}
</style>
