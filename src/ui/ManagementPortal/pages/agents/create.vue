<template>
	<div>
		<h2 class="page-header">{{ editAgent ? 'Edit Agent' : 'Create New Agent' }}</h2>
		<div class="page-subheader">
			{{
				editAgent
					? 'Edit your agent settings below.'
					: 'Complete the settings below to create and deploy your new agent.'
			}}
		</div>

		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="span-2">
				<div class="step-header mb-2">Agent name:</div>
				<div class="mb-2">
					No special characters or spaces, lowercase letters with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="formValues.agentName"
						:disabled="editAgent"
						placeholder="Enter agent name"
						type="text"
						class="w-100"
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
						class="icon invalid"
						:title="validationMessage"
					>
						❌
					</span>
				</div>
			</div>
			<div class="span-2">
				<div class="step-header mb-2">Description:</div>
				<div class="mb-2">Provide a description to help others understand the agent's purpose.</div>
				<InputText
					v-model="formValues.agentDescription"
					placeholder="Enter agent description"
					type="text"
					class="w-100"
				/>
			</div>

			<!-- Type -->
			<div class="step-section-header span-2">Type</div>

			<div class="step-header span-2">What type of agent?</div>

			<!-- Knowledge management agent -->
			<div class="step">
				<div
					class="step-container cursor-pointer"
					@click="handleAgentTypeSelect('knowledge-management')"
				>
					<div class="step-container__edit__inner">
						<div class="step__radio">
							<RadioButton
								v-model="formValues.agentType"
								name="agentType"
								value="knowledge-management" />
							<div class="step-container__header">Knowledge Management</div>
						</div>
						<div>Best for Q&A, summarization and reasoning over textual data.</div>
					</div>
				</div>
			</div>

			<!-- Analytics agent-->
			<div class="step step--disabled">
				<div class="step-container cursor-pointer" @click="handleAgentTypeSelect('analytics')">
					<div class="step-container__edit__inner">
						<div class="step__radio">
							<RadioButton v-model="formValues.agentType" name="agentType" value="analytics" />
							<div class="step-container__header">Analytics</div>
						</div>
						<div>Best to query, analyze, calculate and report on tabular data.</div>
					</div>
				</div>
			</div>

			<!-- Knowledge source -->
			<div class="step-section-header span-2">Knowledge Source</div>

			<div class="step-header">Do you want this agent to have a dedicated pipeline?</div>

			<div v-if="!formValues.dedicated_pipeline">
				<div class="step-header span-2">Does this agent have an inline context?</div>
			</div>
			<div v-else>
				<div class="step-header span-2"></div>
			</div>

			<div class="step">
				<div class="d-flex align-center mt-2">
					<span>
						<ToggleButton
							v-model="formValues.dedicated_pipeline"
							on-label="Yes"
							on-icon="pi pi-check-circle"
							off-label="No"
							off-icon="pi pi-times-circle"
						/>
					</span>
				</div>
			</div>

			<div v-if="!formValues.dedicated_pipeline">
				<div class="step">
					<div class="d-flex align-center mt-2">
						<span>
							<ToggleButton
								v-model="formValues.inline_context"
								on-label="Yes"
								on-icon="pi pi-check-circle"
								off-label="No"
								off-icon="pi pi-times-circle"
							/>
						</span>
					</div>
				</div>
			</div>
			<div v-else>
				<div class="step"></div>
			</div>

			<div v-if="formValues.dedicated_pipeline && !formValues.inline_context">
				<div class="step-header">Where is the data?</div>
			</div>
			<div v-if="!formValues.inline_context">
				<div class="step-header">Where should the data be indexed?</div>
			</div>
			<div v-if="!formValues.dedicated_pipeline && !formValues.inline_context">
				<div class="step-header">How should the data be processed for indexing?</div>
			</div>

			<!-- Data source -->
			<div v-if="formValues.dedicated_pipeline && !formValues.inline_context">
				<CreateAgentStepItem v-model="formValues.editDataSource">
					<template v-if="formValues.selectedDataSource">
						<div class="step-container__header">{{ formValues.selectedDataSource.type }}</div>
						<div>
							<div v-if="formValues.selectedDataSource.object_id !== ''">
								<span class="step-option__header">Name:</span>
							</div>
							<span>{{ formValues.selectedDataSource.name }}</span>
						</div>
						<!-- <div>
							<span class="step-option__header">Container name:</span>
							<span>{{ selectedDataSource.Container.Name }}</span>
						</div> -->

						<!-- <div>
							<span class="step-option__header">Data Format(s):</span>
							<span v-for="format in selectedDataSource.Formats" :key="format" class="mr-1">
								{{ format }}
							</span>
						</div> -->
					</template>
					<template v-else>Please select a data source.</template>

					<template #edit>
						<div class="step-container__edit__header">Please select a data source.</div>

						<div v-for="(group, type) in groupedDataSources" :key="type">
							<div class="step-container__edit__group-header">{{ type }}</div>

							<div
								v-for="dataSource in group"
								:key="dataSource.name"
								class="step-container__edit__option"
								:class="{
									'step-container__edit__option--selected':
										dataSource.name === formValues.selectedDataSource?.name,
								}"
								@click.stop="handleDataSourceSelected(dataSource)"
							>
								<div>
									<div v-if="dataSource.object_id !== ''">
										<span class="step-option__header">Name:</span>
									</div>
									<span>{{ dataSource.name }}</span>
								</div>
								<!-- <div>
									<span class="step-option__header">Container name:</span>
									<span>{{ dataSource.Container.Name }}</span>
								</div> -->

								<!-- <div>
									<span class="step-option__header">Data Format(s):</span>
									<span v-for="format in dataSource.Formats" :key="format" class="mr-1">
										{{ format }}
									</span>
								</div> -->
							</div>
						</div>
					</template>
				</CreateAgentStepItem>
			</div>

			<!-- Index source -->
			<div v-if="!formValues.inline_context">
				<CreateAgentStepItem v-model="formValues.editIndexSource">
					<template v-if="formValues.selectedIndexSource">
						<div v-if="formValues.selectedIndexSource.object_id !== ''">
							<div class="step-container__header">{{ formValues.selectedIndexSource.name }}</div>
							<div>
								<span class="step-option__header">URL:</span>
								<span>{{ formValues.selectedIndexSource.configuration_references.Endpoint }}</span>
							</div>
							<div>
								<span class="step-option__header">Index Name:</span>
								<span>{{ formValues.selectedIndexSource.settings.IndexName }}</span>
							</div>
						</div>
						<div v-else>
							<div class="step-container__header">DEFAULT</div>
							{{ formValues.selectedIndexSource.name }}
						</div>
					</template>
					<template v-else>Please select an index source.</template>

					<template #edit>
						<div class="step-container__edit__header">Please select an index source.</div>
						<div
							v-for="indexSource in indexSources"
							:key="indexSource.name"
							class="step-container__edit__option"
							:class="{
								'step-container__edit__option--selected':
									indexSource.name === formValues.selectedIndexSource?.name,
							}"
							@click.stop="handleIndexSourceSelected(indexSource)"
						>
							<div v-if="indexSource.object_id !== ''">
								<div class="step-container__header">{{ indexSource.name }}</div>
								<div v-if="indexSource.configuration_references.Endpoint">
									<span class="step-option__header">URL:</span>
									<span>{{ indexSource.configuration_references.Endpoint }}</span>
								</div>
								<div v-if="indexSource.settings.IndexName">
									<span class="step-option__header">Index Name:</span>
									<span>{{ indexSource.settings.IndexName }}</span>
								</div>
							</div>
							<div v-else>
								<div class="step-container__header">DEFAULT</div>
								{{ indexSource.name }}
							</div>
						</div>
					</template>
				</CreateAgentStepItem>
			</div>

			<div v-if="formValues.dedicated_pipeline && !formValues.inline_context">
				<div class="step-header">How should the data be processed for indexing?</div>
			</div>
			<div v-if="formValues.dedicated_pipeline && !formValues.inline_context">
				<div class="step-header">When should the data be indexed?</div>
			</div>

			<!-- Process indexing -->
			<div v-if="!formValues.inline_context">
				<CreateAgentStepItem>
					<div class="step-container__header">Splitting & Chunking</div>

					<div>
						<span class="step-option__header">Chunk size:</span>
						<span>{{ formValues.chunkSize }}</span>
					</div>

					<div>
						<span class="step-option__header">Overlap size:</span>
						<span>{{ formValues.overlapSize == 0 ? 'No Overlap' : formValues.overlapSize }}</span>
					</div>

					<template #edit>
						<div class="step-container__header">Splitting & Chunking</div>

						<div>
							<span class="step-option__header">Chunk size:</span>
							<InputText v-model="formValues.chunkSize" type="number" class="mt-2" />
						</div>

						<div>
							<span class="step-option__header">Overlap size:</span>
							<InputText v-model="formValues.overlapSize" type="number" class="mt-2" />
						</div>
					</template>
				</CreateAgentStepItem>
			</div>

			<!-- Trigger -->
			<div v-if="
				formValues.dedicated_pipeline && !formValues.inline_context
				">
				<CreateAgentStepItem>
					<div class="step-container__header">Trigger</div>
					<div>Runs every time a new item is added to the data source.</div>

					<div class="mt-2">
						<span class="step-option__header">Frequency:</span>
						<span>{{ formValues.triggerFrequency }}</span>
					</div>

					<div v-if="formValues.triggerFrequency === 'Schedule' && formValues.triggerFrequencyScheduled">
						<span class="step-option__header">Schedule:</span>
						<span>{{ formValues.triggerFrequencyScheduled }}</span>
					</div>

					<template #edit>
						<div class="step-container__header">Trigger</div>
						<div>Runs every time a new item is added to the data source.</div>

						<div class="mt-2">
							<span class="step-option__header">Frequency:</span>
							<Dropdown
								v-model="formValues.triggerFrequency"
								class="dropdown--agent"
								:options="triggerFrequencyOptions"
								placeholder="--Select--"
							/>
						</div>

						<div v-if="
							formValues.triggerFrequency === 'Schedule'
							" class="mt-2">
							<CronLight
								v-model="formValues.triggerFrequencyScheduled"
								format="quartz"
								@error="formValues.error = $event"
							/>
							<!-- editable cron expression -->
							<InputText
								class="mt-4"
								label="cron expression"
								:model-value="formValues.triggerFrequencyScheduled"
								:error-messages="formValues.error"
								@update:model-value="formValues.triggerFrequencyNextScheduled = $event"
								@blur="formValues.triggerFrequencyScheduled = formValues.triggerFrequencyNextScheduled"
							/>
						</div>
					</template>
				</CreateAgentStepItem>
			</div>

			<!-- Agent configuration -->
			<div class="step-section-header span-2">Agent Configuration</div>

			<div class="step-header">Should conversations be saved?</div>
			<div class="step-header">How should user-agent interactions be gated?</div>

			<!-- Conversation history -->
			<CreateAgentStepItem>
				<div class="step-container__header">Conversation History</div>

				<div>
					<span class="step-option__header">Enabled:</span>
					<span>
						<span>{{ formValues.conversationHistory ? 'Yes' : 'No' }}</span>
						<span
							v-if="formValues.conversationHistory"
							class="pi pi-check-circle ml-1"
							style="color: var(--green-400); font-size: 0.8rem"
						></span>
						<span
							v-else
							class="pi pi-times-circle ml-1"
							style="color: var(--red-400); font-size: 0.8rem"
						></span>
					</span>
				</div>

				<div>
					<span class="step-option__header">Max Messages:</span>
					<span>{{ formValues.conversationMaxMessages }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Conversation History</div>

					<div class="d-flex align-center mt-2">
						<span class="step-option__header">Enabled:</span>
						<span>
							<ToggleButton
								v-model="formValues.conversationHistory"
								on-label="Yes"
								on-icon="pi pi-check-circle"
								off-label="No"
								off-icon="pi pi-times-circle"
							/>
						</span>
					</div>

					<div>
						<span class="step-option__header">Max Messages:</span>
						<InputText v-model="formValues.conversationMaxMessages" type="number" class="mt-2" />
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Gatekeeper -->
			<CreateAgentStepItem>
				<div class="step-container__header">Gatekeeper</div>

				<div>
					<span class="step-option__header">Enabled:</span>
					<span>
						<span>{{ formValues.gatekeeperEnabled ? 'Yes' : 'No' }}</span>
						<span
							v-if="formValues.gatekeeperEnabled"
							class="pi pi-check-circle ml-1"
							style="color: var(--green-400); font-size: 0.8rem"
						></span>
						<span
							v-else
							class="pi pi-times-circle ml-1"
							style="color: var(--red-400); font-size: 0.8rem"
						></span>
					</span>
				</div>

				<div>
					<span class="step-option__header">Content Safety:</span>
					<span>{{ formValues.gatekeeperContentSafety.label }}</span>
				</div>

				<div>
					<span class="step-option__header">Data Protection:</span>
					<span>{{ formValues.gatekeeperDataProtection.label }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Gatekeeper</div>

					<div class="d-flex align-center mt-2">
						<span class="step-option__header">Enabled:</span>
						<span>
							<ToggleButton
								v-model="formValues.gatekeeperEnabled"
								on-label="Yes"
								on-icon="pi pi-check-circle"
								off-label="No"
								off-icon="pi pi-times-circle"
							/>
						</span>
					</div>

					<div class="mt-2">
						<span class="step-option__header">Content Safety:</span>
						<Dropdown
							v-model="formValues.gatekeeperContentSafety"
							class="dropdown--agent"
							:options="gatekeeperContentSafetyOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>

					<div class="mt-2">
						<span class="step-option__header">Data Protection:</span>
						<!-- <span>Microsoft Presidio</span> -->
						<Dropdown
							v-model="formValues.gatekeeperDataProtection"
							class="dropdown--agent"
							:options="gatekeeperDataProtectionOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Orchestrator -->
			<div class="step-header span-2">Which orchestrator should the agent use?</div>
			<div class="span-2">
				<Dropdown
					v-model="formValues.orchestration_settings.orchestrator"
					:options="orchestratorOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
					class="dropdown--agent"
				/>
			</div>

			<!-- <div class="step-header span-2">What are the orchestrator connection details?</div>
			<div
				v-if="
					['LangChain', 'AzureOpenAIDirect', 'AzureAIDirect'].includes(
						orchestration_settings.orchestrator,
					)
				"
			>
				<div class="mb-2 mt-2">API Key:</div>
				<SecretKeyInput v-model="orchestration_settings.endpoint_configuration.api_key" />

				<div class="mb-2 mt-2">Endpoint:</div>
				<InputText
					v-model="orchestration_settings.endpoint_configuration.endpoint"
					class="w-100"
					type="text"
				/>

				<div class="mb-2 mt-2">Version:</div>
				<InputText
					v-model="orchestration_settings.endpoint_configuration.api_version"
					class="w-100"
					type="text"
				/>

				<div class="mb-2 mt-2">Operation Type:</div>
				<InputText
					v-model="orchestration_settings.endpoint_configuration.operation_type"
					class="w-100"
					type="text"
				/>

				<div class="mb-2 mt-2">Model deployment name</div>
				<InputText
					v-model="orchestration_settings.model_parameters.deployment_name"
					class="w-100"
					type="text"
				/>

				<div class="mb-2 mt-2">Model temperature</div>
				<InputText
					v-model="orchestration_settings.model_parameters.temperature"
					class="w-100"
					type="number"
				/>
			</div> -->

			<!-- System prompt -->
			<div class="step-section-header span-2">System Prompt</div>

			<div class="step-header">What is the persona of the agent?</div>

			<div class="span-2">
				<Textarea
					v-model="formValues.systemPrompt"
					class="w-100"
					auto-resize
					rows="5"
					type="text"
					placeholder="You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional."
				/>
			</div>
			<div class="button-container column-2 justify-self-end">
				<!-- Create agent -->
				<Button
					:label="editAgent ? 'Save Changes' : 'Create Agent'"
					severity="primary"
					@click="handleCreateAgent"
				/>

				<!-- Cancel -->
				<Button
					v-if="editAgent"
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
import '@vue-js-cron/light/dist/light.css';
import type { PropType } from 'vue';
import { ref, watch } from 'vue';
import { debounce } from 'lodash';
import { CronLight } from '@vue-js-cron/light';
import api from '@/js/api';
import type {
	Agent,
	AgentIndex,
	AgentDataSource,
	CreateAgentRequest,
	// AgentCheckNameResponse,
} from '@/js/types';
import type { CreatePromptRequest } from '@/js/types';
import type { CreateTextPartitioningProfileRequest } from '@/js/types';

const defaultSystemPrompt: string = '';

const getDefaultFormValues = () => {
	return {
		agentName: '',
		agentDescription: '',
		object_id: '',
		text_partitioning_profile_object_id: '',
		text_embedding_profile_object_id: '',
		vectorization_data_pipeline_object_id: '',
		prompt_object_id: '',
		dedicated_pipeline: true,
		inline_context: false,
		agentType: 'knowledge-management' as CreateAgentRequest['type'],

		editDataSource: false as boolean,
		selectedDataSource: null as null | AgentDataSource,

		editIndexSource: false as boolean,
		selectedIndexSource: null as null | AgentIndex,

		chunkSize: 500,
		overlapSize: 50,

		triggerFrequency: 'Event' as string,
		triggerFrequencyScheduled: '* * * * *' as string,
		triggerFrequencyNextScheduled: '' as string,
		error: '',

		conversationHistory: false as boolean,
		conversationMaxMessages: 5 as number,

		gatekeeperEnabled: false as boolean,
		gatekeeperContentSafety: { label: 'None', value: null },
		gatekeeperDataProtection: { label: 'None', value: null },

		systemPrompt: defaultSystemPrompt as string,

		orchestration_settings: {
			orchestrator: 'LangChain' as string,
			endpoint_configuration: {
				endpoint: '' as string,
				api_key: '' as string,
				api_version: '' as string,
				operation_type: 'chat' as string,
			} as object,
			model_parameters: {
				deployment_name: '' as string,
				temperature: 0 as number,
			} as object,
		},

		// resolved_orchestration_settings: {
		// 	endpoint_configuration: {
		// 		endpoint: '' as string,
		// 		api_key: '' as string,
		// 		api_version: '' as string,
		// 		operation_type: 'chat' as string,
		// 	} as object,
		// },
	};
};

const formValues = ref(getDefaultFormValues());

watch(() => formValues.value.dedicated_pipeline, (newVal, oldVal) => {
  console.log(`dedicated_pipeline changed from ${oldVal} to ${newVal}`);
  if (newVal === true) {
    formValues.value.inline_context = false;
    console.log('inline_context set to false');
  }
}, {
  immediate: true, // Run the watcher immediately with the current value
  deep: true      // This is usually not necessary for simple types like boolean
});

const loading = ref(false as boolean);
const loadingStatusText = ref('Retrieving data...' as string);
const nameValidationStatus = ref(null as string | null); // 'valid', 'invalid', or null
const validationMessage = ref('' as string);
const dataSources = ref([] as AgentDataSource[]);
const indexSources = ref([] as AgentIndex[]);

const orchestratorOptions = ref([
	{
		label: 'LangChain',
		value: 'LangChain',
	},
	{
		label: 'AzureOpenAIDirect',
		value: 'AzureOpenAIDirect',
	},
	{
		label: 'AzureAIDirect',
		value: 'AzureAIDirect',
	},
	{
		label: 'SemanticKernel',
		value: 'SemanticKernel',
	},
]);

const triggerFrequencyOptions = ref(['Event', 'Manual', 'Schedule']);

const triggerFrequencyScheduledOptions = ref([
	'Never',
	'Every 30 minutes',
	'Hourly',
	'Every 12 hours',
	'Daily',
]);

const gatekeeperContentSafetyOptions = ref([
	{
		label: 'None',
		value: null,
	},
	{
		label: 'Azure Content Safety',
		value: 'ContentSafety',
	},
]);

const gatekeeperDataProtectionOptions = ref([
	{
		label: 'None',
		value: null,
	},
	{
		label: 'Microsoft Presidio',
		value: 'Presidio',
	},
]);

export default {
	name: 'CreateAgent',

	components: {
		CronLight,
	},

	props: {
		editAgent: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			formValues,
			loading,
			loadingStatusText,
			nameValidationStatus,
			validationMessage,
			dataSources,
			indexSources,
			orchestratorOptions,
			triggerFrequencyOptions,
			triggerFrequencyScheduledOptions,
			gatekeeperContentSafetyOptions,
			gatekeeperDataProtectionOptions
		};
	},

	computed: {
		groupedDataSources() {
			const grouped = {};
			this.dataSources.forEach((dataSource) => {
				if (!grouped[dataSource.type]) {
					grouped[dataSource.type] = [];
				}

				grouped[dataSource.type].push(dataSource);
			});

			return grouped;
		},
	},

	async created() {
		this.loading = true;
		// Uncomment to remove mock loading screen
		// api.mockLoadTime = 0;

		try {
			this.loadingStatusText = 'Retrieving indexes...';
			this.indexSources = await api.getAgentIndexes(true);

			this.loadingStatusText = 'Retrieving data sources...';
			this.dataSources = await api.getAgentDataSources(true);
		} catch (error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
			});
		}

		if (this.editAgent) {
			this.loadingStatusText = `Retrieving agent "${this.editAgent}"...`;
			const agent = await api.getAgent(this.editAgent);
			if (agent.vectorization && agent.vectorization.text_partitioning_profile_object_id) {
				this.loadingStatusText = `Retrieving text partitioning profile...`;
				const textPartitioningProfile = await api.getTextPartitioningProfile(
					agent.vectorization.text_partitioning_profile_object_id,
				);
				if (textPartitioningProfile) {
					this.formValues.chunkSize = Number(textPartitioningProfile.settings.ChunkSizeTokens);
					this.formValues.overlapSize = Number(textPartitioningProfile.settings.OverlapSizeTokens);
				}
			}
			if (agent.prompt_object_id !== '') {
				this.loadingStatusText = `Retrieving prompt...`;
				const prompt = await api.getPrompt(agent.prompt_object_id);
				if (prompt) {
					this.formValues.systemPrompt = prompt.prefix;
				}
			}
			this.loadingStatusText = `Mapping agent values to form...`;
			this.mapAgentToForm(agent);
		}

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		mapAgentToForm(agent: Agent) {
			this.formValues.agentName = agent.name || this.formValues.agentName;
			this.formValues.agentDescription = agent.description || this.formValues.agentDescription;
			this.formValues.agentType = agent.type || this.agentType;
			this.formValues.object_id = agent.object_id || this.formValues.object_id;

			this.formValues.orchestration_settings.orchestrator =
				agent.orchestration_settings?.orchestrator || this.formValues.orchestration_settings.orchestrator;
			this.formValues.orchestration_settings.endpoint_configuration.endpoint =
				agent.orchestration_settings?.endpoint_configuration.endpoint ||
				this.formValues.orchestration_settings.endpoint_configuration.endpoint;
			this.formValues.orchestration_settings.endpoint_configuration.api_key =
				agent.orchestration_settings?.endpoint_configuration.api_key ||
				this.formValues.orchestration_settings.endpoint_configuration.api_key;
			this.formValues.orchestration_settings.endpoint_configuration.api_version =
				agent.orchestration_settings?.endpoint_configuration.api_version ||
				this.formValues.orchestration_settings.endpoint_configuration.api_version;
			this.formValues.orchestration_settings.endpoint_configuration.operation_type =
				agent.orchestration_settings?.endpoint_configuration.operation_type ||
				this.formValues.orchestration_settings.endpoint_configuration.operation_type;

			this.formValues.orchestration_settings.model_parameters.deployment_name =
				agent.orchestration_settings?.model_parameters.deployment_name ||
				this.formValues.orchestration_settings.model_parameters.deployment_name;
			this.formValues.orchestration_settings.model_parameters.temperature =
				agent.orchestration_settings?.model_parameters.temperature ||
				this.formValues.orchestration_settings.model_parameters.temperature;

			// this.resolved_orchestration_settings = agent.resolved_orchestration_settings || this.resolved_orchestration_settings;

			if (agent.vectorization) {
				this.formValues.dedicated_pipeline = agent.vectorization.dedicated_pipeline;
				// If this is not a dedicated pipeline and the agent does not have an indexing profile, it has an inline context.
				this.formValues.inline_context = (!this.formValues.dedicated_pipeline && !agent.vectorization.indexing_profile_object_id) || false;
			}
			else {
				this.formValues.dedicated_pipeline = false;
				this.formValues.inline_context = true;
			}
			this.formValues.text_embedding_profile_object_id =
				agent.vectorization?.text_embedding_profile_object_id ||
				this.formValues.text_embedding_profile_object_id;

			this.formValues.triggerFrequency = agent.vectorization?.trigger_type || this.formValues.triggerFrequency;
			this.formValues.triggerFrequencyScheduled =
				agent.vectorization?.trigger_cron_schedule || this.formValues.triggerFrequencyScheduled;

			this.formValues.selectedIndexSource =
				this.indexSources.find(
					(indexSource) =>
						indexSource.object_id === agent.vectorization?.indexing_profile_object_id,
				) || null;

			this.formValues.selectedDataSource =
				this.dataSources.find(
					(dataSource) => dataSource.object_id === agent.vectorization?.data_source_object_id,
				) || null;

			this.formValues.conversationHistory = agent.conversation_history?.enabled || this.formValues.conversationHistory;
			this.formValues.conversationMaxMessages =
				agent.conversation_history?.max_history || this.formValues.conversationMaxMessages;

			this.formValues.gatekeeperEnabled = Boolean(agent.gatekeeper?.use_system_setting);

			this.formValues.gatekeeperContentSafety =
				this.gatekeeperContentSafetyOptions.find((localOption) =>
					agent.gatekeeper.options.find((option) => option === localOption.value),
				) || this.formValues.gatekeeperContentSafety;

			this.formValues.gatekeeperDataProtection =
				this.gatekeeperDataProtectionOptions.find((localOption) =>
					agent.gatekeeper.options.find((option) => option === localOption.value),
				) || this.formValues.gatekeeperDataProtection;
		},

		async checkName() {
			try {
				const response = await api.checkAgentName(this.formValues.agentName, this.formValues.agentType);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = '';
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
					// this.$toast.add({
					// 	severity: 'warn',
					// 	detail: `Agent name "${this.agentName}" is already taken for the selected ${response.type} agent type. Please choose another name.`,
					// });
				}
			} catch (error) {
				console.error('Error checking agent name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the agent name. Please try again.';
			}
		},

		resetForm() {
			for (const key in formValues) {
				(this as any)[key as string] = (formValues as any)[key];
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}
			this.$router.push('/agents/public');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.formValues.agentName = sanitizedValue;

			// Check if the name is available if we are creating a new agent.
			if (!this.editAgent) {
				this.debouncedCheckName();
			}
		},

		handleAgentTypeSelect(type: Agent['type']) {
			this.formValues.agentType = type;
		},

		handleDataSourceSelected(dataSource: AgentDataSource) {
			this.formValues.selectedDataSource = dataSource;
			this.formValues.editDataSource = false;
		},

		handleIndexSourceSelected(indexSource: AgentIndex) {
			this.formValues.selectedIndexSource = indexSource;
			this.formValues.editIndexSource = false;
		},

		async handleCreateAgent() {
			const errors = [];
			if (!this.formValues.agentName) {
				errors.push('Please give the agent a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage);
			}

			if (!this.formValues.inline_context) {
				if (this.formValues.text_embedding_profile_object_id === '') {
					const textEmbeddingProfiles = await api.getTextEmbeddingProfiles();
					if (textEmbeddingProfiles.length === 0) {
						errors.push('No vectorization text embedding profiles found.');
					} else {
						this.formValues.text_embedding_profile_object_id = textEmbeddingProfiles[0].object_id;
					}
				}
			}
			else {
				this.formValues.text_embedding_profile_object_id = '';
			}

			// if (!this.selectedDataSource) {
			// 	errors.push('Please select a data source.');
			// }

			// if (!this.selectedIndexSource) {
			// 	errors.push('Please select an index source.');
			// }

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.loading = true;
			this.loadingStatusText = 'Creating agent...';

			const promptRequest: CreatePromptRequest = {
				type: 'multipart',
				name: this.formValues.agentName,
				description: `System prompt for the ${this.formValues.agentName} agent`,
				prefix: this.formValues.systemPrompt,
				suffix: '',
			};

			const tokenTextPartitionRequest : CreateTextPartitioningProfileRequest = {
				text_splitter: 'TokenTextSplitter',
				name: this.formValues.agentName,
				settings: {
					Tokenizer: 'MicrosoftBPETokenizer',
					TokenizerEncoder: 'cl100k_base',
					ChunkSizeTokens: this.formValues.chunkSize.toString(),
					OverlapSizeTokens: this.formValues.overlapSize.toString(),
				},
			};

			let successMessage = null;
			try {
				// Handle Prompt creation/update.
				let promptObjectId = '';
				if (promptRequest.prefix !== '') {
					const promptResponse = await api.createOrUpdatePrompt(this.formValues.agentName, promptRequest);
					promptObjectId = promptResponse.objectId;
				}

				let textPartitioningProfileObjectId = '';
				let dataSourceObjectId = '';
				let indexingProfileObjectId = '';
				if (!this.formValues.inline_context) {
					// Handle TextPartitioningProfile creation/update.
					const tokenTextPartitionResponse = await api.createOrUpdateTextPartitioningProfile(
						this.formValues.agentName,
						tokenTextPartitionRequest,
					);
					textPartitioningProfileObjectId = tokenTextPartitionResponse.objectId;

					if (this.formValues.dedicated_pipeline) {
						// Select the default data source, if any.
						dataSourceObjectId = this.formValues.selectedDataSource?.object_id ?? '';
						if (dataSourceObjectId === '') {
							const defaultDataSource = await api.getDefaultDataSource();
							if (defaultDataSource !== null) {
								dataSourceObjectId = defaultDataSource.object_id;
							}
						}
					}

					// Select the default indexing profile, if any.
					indexingProfileObjectId = this.formValues.selectedIndexSource?.object_id ?? '';
					if (indexingProfileObjectId === '') {
						const defaultAgentIndex = await api.getDefaultAgentIndex();
						if (defaultAgentIndex !== null) {
							indexingProfileObjectId = defaultAgentIndex.object_id;
						}
					}
				}

				const agentRequest: CreateAgentRequest = {
					type: this.formValues.agentType,
					name: this.formValues.agentName,
					description: this.formValues.agentDescription,
					object_id: this.formValues.object_id,

					vectorization: {
						dedicated_pipeline: this.formValues.dedicated_pipeline,
						text_embedding_profile_object_id: this.formValues.text_embedding_profile_object_id,
						indexing_profile_object_id: indexingProfileObjectId,
						text_partitioning_profile_object_id: textPartitioningProfileObjectId,
						data_source_object_id: dataSourceObjectId,
						vectorization_data_pipeline_object_id: this.formValues.vectorization_data_pipeline_object_id,
						trigger_type: this.formValues.triggerFrequency,
						trigger_cron_schedule: this.formValues.triggerFrequencyScheduled,
					},

					conversation_history: {
						enabled: this.formValues.conversationHistory,
						max_history: Number(this.formValues.conversationMaxMessages),
					},

					gatekeeper: {
						use_system_setting: this.formValues.gatekeeperEnabled,
						options: [
							this.formValues.gatekeeperContentSafety.value as unknown as string,
							this.formValues.gatekeeperDataProtection.value as unknown as string,
						].filter((option) => option !== null),
					},

					language_model: {
						type: 'openai',
						provider: 'microsoft',
						temperature: 0,
						use_chat: true,
						api_endpoint: 'FoundationaLLM:AzureOpenAI:API:Endpoint',
						api_key: 'FoundationaLLM:AzureOpenAI:API:Key',
						api_version: 'FoundationaLLM:AzureOpenAI:API:Version',
						version: 'FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion',
						deployment: 'FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName',
					},

					sessions_enabled: true,

					prompt_object_id: promptObjectId,
					orchestration_settings: this.formValues.orchestration_settings,
				};

				if (this.editAgent) {
					await api.upsertAgent(this.editAgent, agentRequest);
					successMessage = `Agent "${this.formValues.agentName}" was successfully updated!`;
				} else {
					await api.upsertAgent(agentRequest.name, agentRequest);
					successMessage = `Agent "${this.formValues.agentName}" was successfully created!`;
					this.resetForm();
				}
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
			});

			this.loading = false;

			if (!this.editAgent) {
				this.$router.push('/agents/public');
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
	display: flex;
	flex-direction: column;
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
</style>
