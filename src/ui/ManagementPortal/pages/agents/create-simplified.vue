<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editAgent ? 'Edit Agent' : 'Create New Agent' }}</h2>
				<div class="page-subheader">
					{{
						editAgent
							? 'Edit your agent settings below.'
							: 'Complete the settings below to create and deploy your new agent.'
					}}
				</div>
			</div>
		</div>

		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Basic Agent Information -->
			<div class="col-span-2">
				<div class="step-header !mb-2">Agent Display Name:</div>
				<div class="mb-2">
					This is the name that will be displayed to users when interacting with the agent. The agent name will be automatically generated from this display name.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="agentDisplayName"
						type="text"
						class="w-full"
						placeholder="Enter agent display name"
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
						class="icon invalid"
						:title="validationMessage"
						:aria-label="validationMessage"
					>
						❌
					</span>
				</div>
			</div>

			<div class="col-span-2">
				<div class="step-header !mb-2">Description:</div>
				<div id="aria-description" class="mb-2">
					Provide a description to help others understand the agent's purpose.
				</div>
				<InputText
					v-model="agentDescription"
					type="text"
					class="w-full"
					placeholder="Enter agent description"
					aria-labelledby="aria-description"
				/>
			</div>

			<!-- System Prompt -->
			<div class="mb-6">
				<div id="aria-persona" class="step-header !mb-3">What is the main workflow prompt?</div>
				<div class="col-span-2">
					<Textarea
						v-model="systemPrompt"
						class="w-full"
						auto-resize
						rows="5"
						type="text"
						placeholder="You are an analytic agent that helps people find information. Provide concise answers that are polite and professional."
						aria-labelledby="aria-persona"
					/>
				</div>
			</div>

			<!-- Workflow main model -->
			<div class="mb-6">
				<div id="aria-workflow-model" class="step-header !mb-3">Workflow main model:</div>
				<Dropdown
					:model-value="workflowMainAIModel?.object_id"
					:options="aiModelOptions"
					:option-label="(model) => model.display_name || model.name"
					option-value="object_id"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-workflow-model"
					@change="
						workflowMainAIModel = JSON.parse(
							JSON.stringify(aiModelOptions.find((model) => model.object_id === $event.value)),
						)
					"
				/>
			</div>

			<!-- Expiration -->
			<div class="mb-6">
				<div class="step-header !mb-3">Would you like to set an expiration on this agent?</div>
				<Calendar
					v-model="expirationDate"
					show-icon
					show-button-bar
					placeholder="Enter expiration date"
					type="text"
				/>
			</div>

			<!-- Form buttons -->
			<div class="flex col-span-2 justify-end gap-4">
				<!-- Create agent -->
				<Button
					:label="editAgent ? 'Save Changes' : 'Create Agent'"
					severity="primary"
					:disabled="editable === false"
					@click="handleCreateAgent"
				/>

				<!-- Cancel -->
				<Button label="Cancel" severity="secondary" @click="handleCancel" />
			</div>
		</div>
	</main>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { ref } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';
import type {
	Agent,
	CreateAgentRequest,
	AIModel,
	WorkflowResourceObject,
} from '@/js/types';

const defaultSystemPrompt: string = 'You are a polite agent.';

const getDefaultFormValues = () => {
	return {
		agentName: '',
		agentDescription: '',
		agentDisplayName: '',
		agentType: 'knowledge-management' as CreateAgentRequest['type'],
		object_id: '',
		systemPrompt: defaultSystemPrompt as string,
		aiModelOptions: [] as AIModel[],
		workflowMainAIModel: null as AIModel | null,
		expirationDate: null as string | null,
	};
};

export default {
	name: 'CreateAgentSimplified',

	props: {
		editAgent: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: false,
			loadingStatusText: '',
			agentName: '',
			agentDescription: '',
			agentDisplayName: '',
			agentType: 'knowledge-management' as CreateAgentRequest['type'],
			object_id: '',
			systemPrompt: '',
			nameValidationStatus: 'unchecked' as 'unchecked' | 'valid' | 'invalid',
			validationMessage: '',
			debouncedCheckName: null as any,
			editable: false as boolean,
			aiModelOptions: [] as AIModel[],
			workflowMainAIModel: null as AIModel | null,
			expirationDate: null as string | null,
		};
	},

	watch: {
		agentDisplayName: {
			handler(newValue) {
				if (!newValue) {
					this.agentName = '';
					this.nameValidationStatus = 'unchecked';
					return;
				}

				// Sanitize the display name to create a valid agent name
				const sanitizedValue = newValue
					.toLowerCase()
					.replace(/\s+/g, '-') // Replace spaces with dashes
					.replace(/[^a-z0-9-_]/g, '') // Remove all non-alphanumeric characters except dashes and underscores
					.replace(/-+/g, '-') // Replace multiple dashes with single dash
					.replace(/_+/g, '_') // Replace multiple underscores with single underscore
					.replace(/-$/, ''); // Remove trailing dash if it exists

				this.agentName = sanitizedValue;
				this.debouncedCheckName();
			},
			immediate: true,
		},
	},

	async created() {
		this.loading = true;

		try {
			this.loadingStatusText = 'Retrieving AI models...';
			const aiModelsResult = await api.getAIModels();
			this.aiModelOptions = aiModelsResult.map((result) => result.resource);
			// Filter the AIModels so we only display the ones where the type is 'completion'.
			this.aiModelOptions = this.aiModelOptions.filter((model) => model.type === 'completion');
		} catch (error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
				life: 5000,
			});
		}

		if (this.editAgent) {
			this.loadingStatusText = `Retrieving agent "${this.editAgent}"...`;
			const agentGetResult = await api.getAgent(this.editAgent);
			this.editable = agentGetResult.actions.includes('FoundationaLLM.Agent/agents/write');

			const agent = agentGetResult.resource;
			this.mapAgentToForm(agent);
		} else {
			this.editable = true;
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
		this.loading = false;
	},

	methods: {
		mapAgentToForm(agent: Agent) {
			this.agentName = agent.name || this.agentName;
			this.agentDescription = agent.description || this.agentDescription;
			this.agentDisplayName = agent.display_name || this.agentDisplayName;
			this.agentType = agent.type || this.agentType;
			this.object_id = agent.object_id || this.object_id;
			this.expirationDate = agent.expiration_date ? new Date(agent.expiration_date) : this.expirationDate;

			if (agent.prompt_object_id) {
				this.loadingStatusText = `Retrieving prompt...`;
				const prompt = api.getPrompt(agent.prompt_object_id);
				if (prompt && prompt.resource) {
					this.systemPrompt = prompt.resource.prefix;
				}
			}

			if (agent.workflow?.resource_object_ids) {
				const existingMainModel = Object.values(agent.workflow.resource_object_ids).find(
					(resource: WorkflowResourceObject) => resource.properties?.object_role === 'main_model',
				);
				if (existingMainModel) {
					const model = this.aiModelOptions.find(m => m.object_id === existingMainModel.object_id);
					if (model) {
						this.workflowMainAIModel = model;
					}
				}
			}
		},

		async checkName() {
			if (!this.agentName) {
				this.nameValidationStatus = 'unchecked';
				return;
			}

			try {
				const response = await api.checkAgentName(this.agentName, this.agentType);
				this.nameValidationStatus = response.status === 'Allowed' ? 'valid' : 'invalid';
				this.validationMessage = response.status === 'Allowed' 
					? 'Name is available'
					: response.message || 'Name is already taken';
			} catch (error: any) {
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking name availability';
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},

		resetForm() {
			const defaultFormValues = getDefaultFormValues();
			for (const key in defaultFormValues) {
				this[key] = defaultFormValues[key];
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}
			this.$router.push('/agents/public');
		},

		async handleCreateAgent() {
			const errors = [];
			if (!this.agentName) {
				errors.push('Please give the agent a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage);
			}

			if (this.systemPrompt === '') {
				errors.push('Please provide a system prompt.');
			}

			if (!this.workflowMainAIModel) {
				errors.push('Please select a workflow main model.');
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
			this.loadingStatusText = 'Saving agent...';

			try {
				const promptRequest = {
					type: 'multipart',
					name: this.agentName,
					description: `System prompt for the ${this.agentName} agent`,
					prefix: this.systemPrompt,
					suffix: '',
					category: 'Workflow',
				};

				let promptObjectId = '';
				if (promptRequest.prefix !== '') {
					const promptResponse = await api.createOrUpdatePrompt(promptRequest.name, promptRequest);
					promptObjectId = promptResponse.objectId;
				}

				const agentRequest: CreateAgentRequest = {
					type: this.agentType,
					name: this.agentName,
					description: this.agentDescription,
					display_name: this.agentDisplayName,
					properties: {
						welcome_message: '',
					},
					object_id: '',
					cost_center: '',
					expiration_date: this.expirationDate?.toISOString(),
					inline_context: true,
					show_message_tokens: false,
					show_message_rating: false,
					show_view_prompt: false,
					show_file_upload: true,
					tools: [],
					sessions_enabled: true,
					workflow: {
						type: 'external-agent-workflow',
						name: 'FoundationaLLMFunctionCallingWorkflow',
						package_name: 'foundationallm_agent_plugins',
						class_name: 'FoundationaLLMFunctionCallingWorkflow',
						workflow_host: 'LangChain',
						resource_object_ids: {
							[this.workflowMainAIModel.object_id]: {
								object_id: this.workflowMainAIModel.object_id,
								properties: {
									object_role: 'main_model',
								},
							},
							...(promptObjectId
								? {
										[promptObjectId]: {
											object_id: promptObjectId,
											properties: {
												object_role: 'main_prompt',
											},
										},
									}
								: {}),
							[`/instances/${this.$appConfigStore.instanceId}/providers/FoundationaLLM.Agent/workflows/ExternalAgentWorkflow`]:
								{
									object_id: `/instances/${this.$appConfigStore.instanceId}/providers/FoundationaLLM.Agent/workflows/ExternalAgentWorkflow`,
									properties: {},
							},
						},
					},
				};

				const response = await api.upsertAgent(this.agentName, agentRequest);
				this.$toast.add({
					severity: 'success',
					detail: `Agent "${this.agentName}" ${this.editAgent ? 'updated' : 'created'} successfully.`,
					life: 5000,
				});
				this.$router.push('/agents/public');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			} finally {
				this.loading = false;
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
	pointer-events: auto;
}

.step-section-header {
	margin: 0px;
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