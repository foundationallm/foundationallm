<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editId ? 'Edit Indexing Profile' : 'Create Indexing Profile' }}</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your indexing profile settings below.'
							: 'Complete the settings below to configure the indexing profile.'
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
			<div class="step-header span-2">What is the name of the indexing profile?</div>
			<div class="span-2">
				<div id="aria-source-name" class="mb-2">Indexing profile name:</div>
				<div id="aria-source-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="indexingProfile.name"
						:disabled="editId"
						type="text"
						class="w-100"
						placeholder="Enter indexing profile name"
						aria-labelledby="aria-source-name aria-source-name-desc"
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

				<div id="aria-data-desc" class="mb-2 mt-2">Indexing Profile description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="indexingProfile.description"
						type="text"
						class="w-100"
						placeholder="Enter a description for this indexing profile"
						aria-labelledby="aria-data-desc"
					/>
				</div>
			</div>

			<!-- Type -->
			<div id="aria-source-type" class="step-header span-2">
				What is the indexer of the indexing profile?
			</div>
			<div class="span-2">
				<Dropdown
					v-model="indexingProfile.indexer"
					:options="profileIndexerOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-source-type"
				/>
			</div>

			<!-- Settings -->

			<!-- Index Name -->
			<div id="aria-index-name" class="step-header span-2">Index Name:</div>
			<div class="span-2">
				<InputText
					v-model="indexingProfile.settings.IndexName"
					:disabled="editId"
					type="text"
					class="w-100"
					placeholder="Enter index name"
					aria-labelledby="aria-index-name"
					@input="handleIndexNameInput"
				/>
			</div>

			<!-- TopN -->
			<div id="aria-top-n" class="step-header span-2">Top N:</div>
			<div class="span-2">
				<InputText
					v-model="indexingProfile.settings.TopN"
					type="text"
					class="w-100"
					placeholder="Enter top N"
					aria-labelledby="aria-top-n"
				/>
			</div>

			<!-- Filters -->
			<div id="aria-filters" class="step-header span-2">Filters:</div>
			<div class="span-2">
				<InputText
					v-model="indexingProfile.settings.Filters"
					type="text"
					class="w-100"
					placeholder="Enter filters"
					aria-labelledby="aria-filters"
				/>
			</div>

			<!-- Embedding Field Name -->
			<div id="aria-embedding-field-name" class="step-header span-2">Embedding Field Name:</div>
			<div class="span-2">
				<InputText
					v-model="indexingProfile.settings.EmbeddingFieldName"
					type="text"
					class="w-100"
					placeholder="Enter embedding field name"
					aria-labelledby="aria-embedding-field-name"
				/>
			</div>

			<!-- Text Field Name -->
			<div id="aria-text-field-name" class="step-header span-2">Text Field Name:</div>
			<div class="span-2">
				<InputText
					v-model="indexingProfile.settings.TextFieldName"
					type="text"
					class="w-100"
					placeholder="Enter text field name"
					aria-labelledby="aria-text-field-name"
				/>
			</div>

			<!-- API Endpoint Configuration Object ID -->
			<div id="aria-api-endpoint-configuration-object-id" class="step-header span-2">
				API Endpoint Configuration Object ID:
			</div>
			<div class="span-2">
				<Dropdown
					v-model="indexingProfile.settings.api_endpoint_configuration_object_id"
					:options="profileIndexerAPIEndpointOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-api-endpoint-configuration-object-id"
				/>
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create data source -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Indexing Profile'"
					severity="primary"
					@click="handleCreateIndexingProfile"
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
	name: 'CreateDataSource',

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

            indexingProfile: {
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
                }
            } as null | any,

			profileIndexerOptions: [
				{
					label: 'Azure AI Search Indexer',
					value: 'AzureAISearchIndexer',
				},
			],

			profileIndexerAPIEndpointOptions: [] as any[],
		};
	},

	async created() {
		this.loading = true;

        if (this.editId) {
            this.loadingStatusText = `Retrieving indexing source "${this.editId}"...`;
            const indexingProfileResult = await api.getIndexingProfile(this.editId);
            const indexingProfile = indexingProfileResult.resource;
            this.indexingProfile = indexingProfile;
        } else {
            // Create a new IndexingProfile object.
            const newIndexingProfile = {
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
                }
            };
            this.indexingProfile = newIndexingProfile;
        }

		this.getAPIEndpointConfigurationObjectIDs();

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkIndexingProfileName(this.indexingProfile.name);

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

			this.$router.push('/data-sources');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.indexingProfile.name = sanitizedValue;

			// Check if the name is available if we are creating a new data source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		handleIndexNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.indexingProfile.settings.IndexName = sanitizedValue;
		},

		async handleCreateIndexingProfile() {
			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving data source...';
				await api.createIndexingProfile(this.indexingProfile);
				successMessage = `Indexing Profile "${this.indexingProfile.name}" was successfully saved.`;
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
				this.$router.push('/pipeline/indexing-profiles');
			}
		},

		async getAPIEndpointConfigurationObjectIDs() {
			this.loading = true;
			try {
				const response = await api.getOrchestrationServices();

				const filteredData = response.filter(item => 
					item.resource &&
					item.resource.category &&
					item.resource.category === "General"&&
					item.resource.subcategory &&
					item.resource.subcategory === "Indexing"
				);

				filteredData.forEach(item => {
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
</style>
