<template>
	<div>
		<h2 class="page-header">Create New Agent</h2>
		<div class="page-subheader">Complete the settings below to create and deploy your new agent.</div>

		<div class="steps" :class="{ 'steps--loading': loading }">

			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Type -->
			<div class="step-section-header span-2">Type</div>

			<div class="step">
				<div class="step__header">What type of agent?</div>
				<div class="step-container cursor-pointer" @click="handleAgentTypeSelect('knowledge')">
					<div class="step__radio">
						<RadioButton v-model="agentType" name="agentType" value="knowledge" />
						<div class="step-container__header">Knowledge Management</div>
					</div>
					<div>Best for Q&A, summarization and reasoning over textual data.</div>
				</div>
			</div>

			<div class="step">
				<div class="step__header" style="visibility: hidden;">What type of agent?</div>
				<div class="step-container cursor-pointer" @click="handleAgentTypeSelect('analytics')">
					<div class="step__radio">
						<RadioButton v-model="agentType" name="agentType" value="analytics" />
						<div class="step-container__header">Analytics</div>
					</div>
					<div>Best to query, analyze, calculate and report on tabular data.</div>
				</div>
			</div>

			<!-- Knowledge source -->
			<div class="step-section-header span-2">Knowledge Source</div>

			<div class="step">
				<div class="step__header">Where is the data?</div>
				<div class="step-container">
					<div class="step-container__header">Blob Storage</div>
					<div>
						<span class="step-option__header">Storage account name:</span>
						<span>filmaksl4sa</span>
					</div>
					<div>
						<span class="step-option__header">Container name:</span>
						<span>documents</span>
					</div>
					<div>
						<span class="step-option__header">Data Format(s):</span>
						<span>pdf</span>
					</div>
				</div>
			</div>

			<div class="step">
				<div class="step__header">Where should the data be indexed?</div>
				<div class="step-container" @click="handleIndexSourceClicked">
					<!-- Options -->
					<div
						v-if="editIndexSources"
						class="step-container--edit"
						tabindex="0"
						@focusout="editIndexSources = false"
					>
						<div class="step-container--edit__header">Please select an index source.</div>
						<div
							v-for="indexSource in indexSources"
							:key="indexSource.Name"
							class="step-container--edit__option"
							:class="{
								'step-container--edit__option--selected':
									indexSource.Name === selectedIndexSource?.Name,
							}"
							@click.stop="handleIndexSourceSelected(indexSource)"
						>
							<div class="step-container__header">{{ indexSource.Name }}</div>
							<div>
								<span class="step-option__header">URL:</span>
								<span>{{ indexSource.ConfigurationReferences.Endpoint }}</span>
							</div>
							<div>
								<span class="step-option__header">Index Name:</span>
								<span>{{ indexSource.Settings.IndexName }}</span>
							</div>
						</div>
					</div>

					<!-- Select option -->
					<template v-if="selectedIndexSource">
						<div class="step-container__header">{{ selectedIndexSource.Name }}</div>
						<div>
							<span class="step-option__header">URL:</span>
							<span>{{ selectedIndexSource.ConfigurationReferences.Endpoint }}</span>
						</div>
						<div>
							<span class="step-option__header">Index Name:</span>
							<span>{{ selectedIndexSource.Settings.IndexName }}</span>
						</div>
					</template>
					<template v-else>Please select an index source.</template>
				</div>
			</div>

			<div class="step">
				<div class="step__header">How should the data be processed for indexing?</div>
				<div class="step-container">
					<div class="step-container__header">Splitting & Chunking</div>
					<div>
						<span class="step-option__header">Chunk size:</span>
						<span>2000 tokens</span>
					</div>
					<div>
						<span class="step-option__header">Overlap size:</span>
						<span>No Overlap</span>
					</div>
				</div>
			</div>

			<div class="step">
				<div class="step__header">When should the data be indexed?</div>
				<div class="step-container">
					<div class="step-container__header">Trigger</div>
					<div>Runs every time a new tile is added to the data source.</div>
					<div>
						<span class="step-option__header">Frequency:</span>
						<span>Manual</span>
					</div>
				</div>
			</div>

			<!-- Agent configuration -->
			<div class="step-section-header span-2">Agent Configuration</div>

			<div class="step">
				<div class="step__header">Should conversations be saved?</div>
				<div class="step-container">
					<div class="step-container__header">Conversation History</div>
					<div>
						<span class="step-option__header">Enabled:</span>
						<span>
							<span>No</span>
							<span class="pi pi-times-circle ml-1" style="color: var(--red-400); font-size: 0.8rem;"></span>
						</span>
					</div>
				</div>
			</div>

			<div class="step">
				<div class="step__header">How should user-agent interactions be gated?</div>
				<div class="step-container">
					<div class="step-container__header">Gatekeeper</div>
					<div>
						<span class="step-option__header">Enabled:</span>
						<span>
							<span>Yes</span>
							<span class="pi pi-check-circle ml-1" style="color: var(--green-400); font-size: 0.8rem;"></span>
						</span>
					</div>
					<div>
						<span class="step-option__header">Content Safety:</span>
						<span>Azure Content Safety</span>
					</div>
					<div>
						<span class="step-option__header">Data Protection:</span>
						<span>Microsoft Presidio</span>
					</div>
				</div>
			</div>

			<!-- System prompt -->
			<div class="step-section-header span-2">System Prompt</div>

			<div class="span-2">
				<div class="step__header">What is the persona of the agent?</div>
				<Textarea v-model="systemPrompt" class="w-100" auto-resize rows="5" type="text" />
			</div>

			<!-- Create agent -->
			<Button
				class="primary-button column-2 justify-self-end"
				style="width: 200px;"
				label="Create Agent"
				@click="handleCreateAgent"
			/>
		</div>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { MockCreateAgentRequest, AgentIndex } from '@/js/types';

const defaultSystemPrompt: string = 'You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional.';

export default {
	name: 'CreateAgent',

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			dataSources: [],
			gatekeepers: [],

			indexSources: [] as AgentIndex[],
			editIndexSources: false,
			selectedIndexSource: null as null | AgentIndex,

			agentType: 'knowledge' as MockCreateAgentRequest['type'],
			systemPrompt: defaultSystemPrompt as string,
		};
	},

	async created() {
		this.loading = true;

		// Uncomment to remove mock loading screen
		// api.mockLoadTime = 0;

		this.loadingStatusText = 'Retrieving indexes...';
		this.indexSources = await api.getAgentIndexes();

		this.loadingStatusText = 'Retrieving data sources...';
		this.dataSources = await api.getAgentDataSources();

		this.loadingStatusText = 'Retrieving gatekeepers...';
		this.gatekeepers = await api.getAgentGatekeepers();

		this.loading = false;
	},

	methods: {
		handleAgentTypeSelect(type: AgentType) {
			this.agentType = type;
		},

		handleIndexSourceClicked() {
			this.editIndexSources = true;
		},

		handleIndexSourceSelected(indexSource) {
			this.selectedIndexSource = indexSource;
			this.editIndexSources = false;
		},

		async handleCreateAgent() {
			this.loading = true;
			this.loadingStatusText = 'Creating agent...';
			await api.createAgent({
				prompt: this.systemPrompt,
			});
			this.loading = false;
			// Route to created agent's page
		},
	},
};
</script>

<style lang="scss" scoped>
.steps {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 24px;
	position: relative;
}

.steps--loading {
	pointer-events: none;
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

.step-section-header {
	background-color: rgba(150, 150, 150, 1);
	color: white;
	font-size: 1rem;
	font-weight: 600;
	padding: 16px;
}

.step {
	display: flex;
	flex-direction: column;

	&__header {
		font-weight: bold;
		margin-bottom: 16px;
	}
}

.step-container {
	padding: 16px;
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

$editStepPadding: 16px;
.step-container--edit {
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
}

.step-container--edit__header {
	padding: $editStepPadding;
}

.step-container--edit__option {
	padding: $editStepPadding;
	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}
}

// .step-container--edit__option + .step-container--edit__option{
// 	border-top: 2px solid #e1e1e1;
// }

.step-container--edit__option--selected {
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
</style>
