<template>
	<div>
		<!-- Header -->
		<h2 class="page-header">{{ editId ? 'Edit API Endpoint' : 'Create API Endpoint' }}</h2>
		<div class="page-subheader">
			{{
				editId
					? 'Edit your API endpoint settings below.'
					: 'Complete the settings below to configure the API endpoint.'
			}}
		</div>

		<!-- Steps -->
		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header span-2">What is the API endpoint name?</div>
			<div class="span-2">
				<div id="aria-source-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="apiEndpoint.name"
						:disabled="editId"
						type="text"
						class="w-100"
						placeholder="Enter API endpoint name"
						aria-labelledby="aria-source-name aria-source-name-desc"
						@input="handleNameInput"
					/>
					<span
						v-if="nameValidationStatus === 'valid'"
						class="icon valid"
						title="Name is available"
					>
						✔️
					</span>
					<span
						v-else-if="nameValidationStatus === 'invalid'"
						:title="validationMessage"
						class="icon invalid"
					>
						❌
					</span>
				</div>
			</div>

			<!-- Model type -->
			<!-- <div class="step-header span-2">What is the model type?</div>
			<div class="span-2">
				<Dropdown
					v-model="apiEndpoint.name"
					:options="orchestratorOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
					class="dropdown--agent"
				/>
			</div> -->

			<!-- Description -->
			<div class="span-2">
				<div class="step-header mb-2">What are the endpoint details?</div>
				<div id="aria-description" class="mb-2">
					Provide a description to help others understand the API endpoint's purpose.
				</div>
				<InputText
					v-model="apiEndpoint.description"
					type="text"
					class="w-100 mb-4"
					placeholder="Enter API endpoint description"
					aria-labelledby="aria-description"
				/>

				<!-- Category -->
				<div class="mb-2">Category:</div>
				<Dropdown
					v-model="apiEndpoint.category"
					:options="categoryOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
					class="mb-4"
				/>

				<!-- Subcategory -->
				<div id="aria-subcategory" class="mb-2">Subcategory:</div>
				<InputText
					v-model="apiEndpoint.subcategory"
					type="text"
					class="w-100 mb-4"
					placeholder="Enter API endpoint subcategory"
					aria-labelledby="aria-subcategory"
				/>
			</div>

			<!-- Connection type -->
			<div class="step-header span-2">What is the connection type?</div>
			<div class="span-2">
				<div class="mb-2">Auth Type:</div>
				<Dropdown
					v-model="apiEndpoint.authentication_type"
					:options="authTypeOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
				/>
			</div>

			<!-- Connection details -->
			<div class="step-header span-2">What are the connection details?</div>
			<div class="span-2">
				<!-- Endpoint URL -->
				<div class="mb-2">Endpoint URL:</div>
				<InputText
					v-model="apiEndpoint.url"
					class="w-100 mb-4"
					type="text"
					placeholder="Enter API endpoint URL"
				/>

				<!-- API Key details -->
				<div v-if="apiEndpoint.authentication_type === 'APIKey'" class="span-2 mb-4">
					<!-- Header name -->
					<div class="mb-2">API Key Header Name:</div>
					<InputText
						v-model="apiEndpoint.authentication_parameters.api_key_header_name"
						class="w-100 mb-4"
						type="text"
					/>

					<!-- API Key -->
					<div id="aria-api-key" class="mb-2 mt-2">API Key:</div>
					<SecretKeyInput
						v-model="apiEndpoint.resolved_configuration_references.APIKey"
						placeholder="Enter API key"
						aria-labelledby="aria-api-key"
					/>
				</div>

				<!-- API Version -->
				<div class="mb-2">API Version:</div>
				<InputText v-model="apiEndpoint.api_version" class="w-100 mb-4" type="text" />

				<!-- Timeout -->
				<div class="mb-2">Timeout (seconds):</div>
				<InputNumber v-model="apiEndpoint.timeout_seconds" class="w-100 mb-4" />

				<!-- Status URL -->
				<div id="aria-status-url" class="mb-2">Status URL:</div>
				<InputText
					v-model="apiEndpoint.status_url"
					type="text"
					class="w-100 mb-4"
					placeholder="Enter API endpoint status URL"
					aria-labelledby="aria-status-url"
				/>

				<!-- URL Exceptions -->
				<div id="aria-status-url" class="mb-2">URL Exceptions:</div>
				<JsonEditorVue v-model="apiEndpoint.url_exceptions" />
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create API endpoint -->
				<Button
					:label="editId ? 'Save Changes' : 'Create API Endpoint'"
					severity="primary"
					@click="handleCreate"
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
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';

import api from '@/js/api';
import JsonEditorVue from 'json-editor-vue';

export default {
	name: 'CreateAPIEndpoint',

	components: {
		JsonEditorVue,
	},

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
			validationMessage: '' as string,

			apiEndpointName: '' as string,
			apiEndpoint: {
				description: null,
				cost_center: null,
				expiration_date: null as string | null,
				display_name: null as string | null,
				name: '' as string,
				url: '' as string,
				api_version: '' as string,
				status_url: null as string | null,
				timeout_seconds: 60 as number,
				retry_strategy_name: 'ExponentialBackoff',
				category: 'General',
				subcategory: 'AIModel',
				provider: 'microsoft',
				authentication_type: 'APIKey',
				operation_type: 'chat',
				url_exceptions: [],
				properties: null,

				authentication_parameters: {
					api_key_header_name: 'api-key',
				},

				resolved_configuration_references: {
					APIKey: '',
				},
			},

			// orchestratorOptions: [
			// 	{
			// 		label: 'Azure OpenAI',
			// 		value: 'AzureOpenAI',
			// 	},
			// 	{
			// 		label: 'Azure OpenAI DALLE',
			// 		value: 'AzureOpenAIDALLE',
			// 	},
			// 	{
			// 		label: 'Azure AI',
			// 		value: 'AzureAI',
			// 	},
			// 	{
			// 		label: 'OpenAI',
			// 		value: 'OpenAI',
			// 	},
			// ],

			categoryOptions: [
				{
					label: 'General',
					value: 'General',
				},
				{
					label: 'LLM',
					value: 'LLM',
				},
				{
					label: 'Gatekeeper',
					value: 'Gatekeeper',
				},
				{
					label: 'Orchestration',
					value: 'Orchestration',
				},
				{
					label: 'FileStoreConnector',
					value: 'FileStoreConnector',
				},
			],

			authTypeOptions: [
				{
					label: 'API Key',
					value: 'APIKey',
				},
				{
					label: 'Azure Identity',
					value: 'AzureIdentity',
				},
			],
		};
	},

	async created() {
		if (this.editId) {
			this.loading = true;
			this.loadingStatusText = `Retrieving API endpoint "${this.editId}"...`;
			this.apiEndpoint = (await api.getAPIEndpointConfiguration(this.editId)).resource;
			this.loading = false;
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkAPIEndpointConfigurationName(this.apiEndpoint.name);

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
				console.error('Error checking API endpoint name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the API endpoint name. Please try again.';
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/api-endpoints');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.apiEndpoint.name = sanitizedValue;

			// Check if the name is available if we are creating a new data source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async handleCreate() {
			const errors = [];
			if (!this.apiEndpoint.name) {
				errors.push('Please provide an name for the API endpoint.');
			}

			if (!this.apiEndpoint.url) {
				errors.push('Please provide a url for the API endpoint.');
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
				this.loadingStatusText = 'Saving API endpoint...';
				await api.createAPIEndpointConfiguration(this.apiEndpoint);
				successMessage = `API endpoint "${this.apiEndpoint.name}" was successfully saved.`;
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
				this.$router.push('/api-endpoints');
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

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
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
</style>
