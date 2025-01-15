<template>
	<div>
		<!-- Header -->
		<h2 class="page-header">{{ editId ? 'Edit Model Endpoint' : 'Create Model Endpoint' }}</h2>
		<div class="page-subheader">
			{{
				editId
					? 'Edit your model endpoint settings below.'
					: 'Complete the settings below to configure the model endpoint.'
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
			<div class="step-header span-2">What is the model endpoint name?</div>
			<div class="span-2">
				<div id="aria-source-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="aiModelEndpoint.name"
						:disabled="editId"
						type="text"
						class="w-100"
						placeholder="Enter model endpoint name"
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
					v-model="aiModelEndpoint.name"
					:options="orchestratorOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
					class="dropdown--agent"
				/>
			</div> -->

			<!-- Connection type -->
			<div class="step-header span-2">What is the connection type?</div>
			<div class="span-2">
				<div class="mb-2">Auth Type:</div>
				<Dropdown
					v-model="aiModelEndpoint.authentication_type"
					:options="authTypeOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
					class="dropdown--agent"
				/>
			</div>

			<!-- Connection details -->
			<div class="step-header span-2">What are the connection details?</div>
			<div class="span-2">
				<!-- Endpoint -->
				<div class="mb-2">Endpoint:</div>
				<InputText
					v-model="aiModelEndpoint.url"
					class="w-100 mb-4"
					type="text"
				/>

				<!-- API Key -->
				<template v-if="aiModelEndpoint.authentication_type === 'AzureIdentity'">
					<div class="mb-2">API Key:</div>
					<SecretKeyInput v-model="aiModelEndpoint.url" class="mb-4" />
				</template>

				<!-- API Version -->
				<template v-if="['AzureOpenAI', 'AzureAI'].includes(aiModelEndpoint.orchestrator)">
					<div class="mb-2">API Version:</div>
					<InputText
						v-model="aiModelEndpoint.api_version"
						class="w-100 mb-4"
						type="text"
					/>
				</template>
			</div>

			<!-- Model params -->
			<!-- <template v-if="aiModelEndpoint.orchestrator === 'AzureOpenAI'">
				<div class="step-header span-2">What are the model details?</div>
				<div class="span-2">
					<div class="mb-2">Deployment Name:</div>
					<SecretKeyInput v-model="aiModelEndpoint.api_key" class="mb-4" />
				</div>
			</template> -->

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create model -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Model Endpoint'"
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

export default {
	name: 'CreateModelOrEndpoint',

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

			// nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			// validationMessage: '' as string,

			aiModelEndpointName: '' as string,
			aiModelEndpoint: {
				display_name: '' as string,
				name: '' as string,
				authentication_type: null,
				url: '' as string,
				// endpoint_configuration: {
				// 	endpoint: '' as string,
				// 	api_key: '' as string,
				// 	api_version: '' as string,
				// 	// operation_type: 'chat' as string,
				// } as object,
			},

			orchestratorOptions: [
				{
					label: 'Azure OpenAI',
					value: 'AzureOpenAI',
				},
				{
					label: 'Azure OpenAI DALLE',
					value: 'AzureOpenAIDALLE',
				},
				{
					label: 'Azure AI',
					value: 'AzureAI',
				},
				{
					label: 'OpenAI',
					value: 'OpenAI',
				},
			],

			authTypeOptions: [
				{
					label: 'API Key',
					value: 'APIKey',
				},
				{
					label: 'Token',
					value: 'AzureIdentity',
				},
			],
		};
	},

	async created() {
		if (this.editId) {
			this.loading = true;
			this.loadingStatusText = `Retrieving AI model endpoint "${this.editId}"...`;
			this.aiModelEndpoint = (await api.getAIModelEndpoint(this.editId))[0].resource;
			this.loading = false;
		}
	},

	methods: {
		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/model-endpoints');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.aiModelEndpoint.name = sanitizedValue;

			// Check if the name is available if we are creating a new data source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async handleCreate() {
			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving AI model...';
				await api.createAIModelEndpoint(this.aiModelEndpoint);
				successMessage = `AI model endpoint "${this.aiModelEndpoint.name}" was successfully saved.`;
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
				this.$router.push('/model-endpoints');
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
