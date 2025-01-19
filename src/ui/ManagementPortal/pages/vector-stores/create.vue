<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editId ? 'Edit Vector Store' : 'Create Vector Store' }}</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your vector store settings below.'
							: 'Complete the settings below to configure the vector store.'
					}}
				</div>
			</div>
		</div>

		<!-- Steps -->
		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header span-2">Vector Store Name:</div>
			<div class="span-2">
				<div id="aria-source-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="vectorStore.name"
						:disabled="editId"
						type="text"
						class="w-100"
						placeholder="Enter vector store name"
						aria-labelledby="aria-source-name aria-source-name-desc"
						@input="handleNameInput"
						:invalid="errors.name"
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
				<div v-if="errors.name" class="error-message">{{ errors.name }}</div>
			</div>

			<!-- Description -->
			<div class="step-header span-2">Vector Store Description:</div>
			<div class="input-wrapper span-2">
				<InputText
					v-model="vectorStore.description"
					type="text"
					class="w-100"
					placeholder="Enter a description for this vector store"
					aria-labelledby="aria-data-desc"
				/>
			</div>

			<!-- Type -->
			<div id="aria-source-type" class="step-header span-2">
				What is the indexer of the vector store?
			</div>
			<div class="span-2">
				<Dropdown
					v-model="vectorStore.indexer"
					:options="profileIndexerOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-source-type"
					:invalid="errors.indexer"
				/>
				<div v-if="errors.indexer" class="error-message">{{ errors.indexer }}</div>
			</div>

			<!-- Settings -->

			<!-- Index Name -->
			<div id="aria-index-name" class="step-header span-2">Index Name:</div>
			<div class="span-2">
				<InputText
					v-model="vectorStore.settings.IndexName"
					type="text"
					class="w-100"
					placeholder="Enter index name"
					aria-labelledby="aria-index-name"
					@input="handleIndexNameInput"
					:invalid="errors.IndexName"
				/>
				<div v-if="errors.IndexName" class="error-message">{{ errors.IndexName }}</div>
			</div>

			<!-- Embedding Field Name -->
			<div id="aria-embedding-field-name" class="step-header span-2">Embedding Field Name:</div>
			<div class="span-2">
				<InputText
					v-model="vectorStore.settings.EmbeddingFieldName"
					type="text"
					class="w-100"
					placeholder="Enter embedding field name"
					aria-labelledby="aria-embedding-field-name"
					:invalid="errors.EmbeddingFieldName"
				/>
				<div v-if="errors.EmbeddingFieldName" class="error-message">
					{{ errors.EmbeddingFieldName }}
				</div>
			</div>

			<!-- Text Field Name -->
			<div id="aria-text-field-name" class="step-header span-2">Text Field Name:</div>
			<div class="span-2">
				<InputText
					v-model="vectorStore.settings.TextFieldName"
					type="text"
					class="w-100"
					placeholder="Enter text field name"
					aria-labelledby="aria-text-field-name"
					:invalid="errors.TextFieldName"
				/>
				<div v-if="errors.TextFieldName" class="error-message">{{ errors.TextFieldName }}</div>
			</div>

			<!-- Indexing Service -->
			<div id="aria-api-endpoint-configuration-object-id" class="step-header span-2">
				Indexing Service:
			</div>
			<div class="span-2">
				<Dropdown
					v-model="vectorStore.settings.api_endpoint_configuration_object_id"
					:options="profileIndexerAPIEndpointOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-api-endpoint-configuration-object-id"
					:invalid="errors.api_endpoint_configuration_object_id"
				/>
				<div v-if="errors.api_endpoint_configuration_object_id" class="error-message">
					{{ errors.api_endpoint_configuration_object_id }}
				</div>
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create Vector Store -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Vector Store'"
					severity="primary"
					@click="handleCreateVectorStore"
				/>

				<!-- Cancel -->
				<Button
					v-if="editId"
					style="margin-left: 16px"
					label="Cancel"
					severity="secondary"
					@click="handleCancel"
				/>
			</div>
		</div>
	</main>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';

export default {
	name: 'CreateVectorStore',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			accessControlModalOpen: false,

			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: null as string | null,

			vectorStore: {
				type: 'indexing-profile',
				name: '',
				display_name: null,
				description: null,
				cost_center: null,
				indexer: '',
				settings: {
					IndexName: '',
					TopN: '',
					Filters: '',
					EmbeddingFieldName: '',
					TextFieldName: '',
					api_endpoint_configuration_object_id: '',
				},
			} as null | any,

			profileIndexerOptions: [
				{
					label: 'Azure AI Search Indexer',
					value: 'AzureAISearchIndexer',
				},
			],

			profileIndexerAPIEndpointOptions: [] as any[],

			errors: {
				name: null,
				indexer: null,
				IndexName: null,
				EmbeddingFieldName: null,
				TextFieldName: null,
				api_endpoint_configuration_object_id: null,
			},
		};
	},

	async created() {
		this.loading = true;

		if (this.editId) {
			this.loadingStatusText = `Retrieving vector store "${this.editId}"...`;
			const vectorStoreResult = await api.getIndexingProfile(this.editId);
			const vectorStore = vectorStoreResult.resource;
			this.vectorStore = vectorStore;
		} else {
			// Create a new vectorStore object.
			const newVectorStore = {
				name: '',
				display_name: '',
				description: '',
				indexer: '',
				settings: {
					IndexName: '',
					TopN: '',
					Filters: '',
					EmbeddingFieldName: '',
					TextFieldName: '',
					api_endpoint_configuration_object_id: '',
				},
			};
			this.vectorStore = newVectorStore;
		}

		this.getAPIEndpointConfigurationObjectIDs();

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkIndexingProfileName(this.vectorStore.name);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
				}
			} catch (error) {
				console.error('Error checking agent name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the agent name. Please try again.';
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/vector-stores');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.vectorStore.name = sanitizedValue;

			// Check if the name is available if we are creating a new vector store.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		handleIndexNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.vectorStore.settings.IndexName = sanitizedValue;
		},

		async handleCreateVectorStore() {
			if (!this.validateForm()) {
				this.$toast.add({
					severity: 'error',
					detail: 'Please correct the errors before submitting.',
					life: 5000,
				});
				return;
			}

			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving vector store...';
				await api.createIndexingProfile(this.vectorStore);
				successMessage = `Vector Store "${this.vectorStore.name}" was successfully saved.`;
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
				this.$router.push('/vector-stores');
			}
		},

		async getAPIEndpointConfigurationObjectIDs() {
			this.loading = true;
			try {
				const response = await api.getOrchestrationServices();

				const filteredData = response.filter(
					(item) =>
						item.resource &&
						item.resource.category &&
						item.resource.category === 'General' &&
						item.resource.subcategory &&
						item.resource.subcategory === 'Indexing',
				);

				filteredData.forEach((item) => {
					this.profileIndexerAPIEndpointOptions.push({
						label: item.resource.name,
						value: item.resource.object_id,
					});
				});
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		validateForm() {
			this.errors = {
				name: null,
				indexer: null,
				IndexName: null,
				EmbeddingFieldName: null,
				TextFieldName: null,
				api_endpoint_configuration_object_id: null,
			};

			let isValid = true;

			// Name validation
			if (!this.vectorStore.name) {
				this.errors.name = 'Name is required.';
				isValid = false;
			}

			// Indexer validation
			if (!this.vectorStore.indexer) {
				this.errors.indexer = 'Indexer is required.';
				isValid = false;
			}

			// Conditional fields if Azure AI Search Indexer is selected
			if (this.vectorStore.indexer === 'AzureAISearchIndexer') {
				if (!this.vectorStore.settings.IndexName) {
					this.errors.IndexName = 'Index Name is required.';
					isValid = false;
				}
				if (!this.vectorStore.settings.EmbeddingFieldName) {
					this.errors.EmbeddingFieldName = 'Embedding Field Name is required.';
					isValid = false;
				}
				if (!this.vectorStore.settings.TextFieldName) {
					this.errors.TextFieldName = 'Text Field Name is required.';
					isValid = false;
				}
			}

			// Indexing Service validation
			if (!this.vectorStore.settings.api_endpoint_configuration_object_id) {
				this.errors.api_endpoint_configuration_object_id = 'Indexing Service is required.';
				isValid = false;
			}

			return isValid;
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

.steps--loading {
	pointer-events: none;
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

.step-section-header {
	background-color: rgba(150, 150, 150, 1);
	color: white;
	font-size: 1rem;
	font-weight: 600;
	padding: 16px;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.step {
	// display: flex;
	// flex-direction: column;
}

.step--disabled {
	pointer-events: none;
	opacity: 0.5;
}

.step-container {
	// padding: 16px;
	border: 2px solid #e1e1e1;
	flex-grow: 1;
	position: relative;

	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}

	&__header {
		font-weight: bold;
		margin-bottom: 8px;
	}
}

.step-container__view {
	// padding: 16px;
	height: 100%;
	display: flex;
	flex-direction: row;
}

.step-container__view__inner {
	padding: 16px;
	flex-grow: 1;
	word-break: break-word;
}

.step-container__view__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	width: 40px;
	min-width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;

	&:hover {
		background-color: #cacaca;
	}
}

$editStepPadding: 16px;
.step-container__edit {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	min-height: calc(100% + 4px);
	// padding: $editStepPadding;
	display: flex;
	flex-direction: row;
}

.step-container__edit__inner {
	padding: $editStepPadding;
	flex-grow: 1;
}

.step-container__edit__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	min-width: 40px;
	width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;
	transform: rotate(180deg);

	&:hover {
		background-color: #cacaca;
	}
}

.step-container__edit-dropdown {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	display: flex;
	flex-direction: column;
	min-height: calc(100% + 4px);
}

.step-container__edit__header {
	padding: $editStepPadding;
}

.step-container__edit__group-header {
	font-weight: bold;
	padding: $editStepPadding;
	padding-bottom: 0px;
}

.step-container__edit__option {
	padding: $editStepPadding;
	word-break: break-word;
	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}
}

// .step-container__edit__option + .step-container__edit__option{
// 	border-top: 2px solid #e1e1e1;
// }

.step-container__edit__option--selected {
	// outline: 2px solid #e1e1e1;
	// background-color: rgba(217, 217, 217, 0.4);
}

.step__radio {
	display: flex;
	gap: 10px;
}

.step-option__header {
	text-decoration: underline;
	margin-right: 8px;
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

.flex-container {
	display: flex;
	align-items: center; /* Align items vertically in the center */
}

.flex-item {
	flex-grow: 1; /* Allow the textarea to grow and fill the space */
}

.flex-item-button {
	margin-left: 8px; /* Add some space between the textarea and the button */
}

.p-chips {
	ul {
		width: 100%;
		li {
			input {
				width: 100% !important;
			}
		}
	}
}

.error-message {
	color: red;
	font-size: 1rem;
	margin-top: 4px;
}
</style>
