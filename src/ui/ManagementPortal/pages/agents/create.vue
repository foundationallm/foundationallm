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

			<div style="display: flex; align-items: center">
				<!-- Private storage -->
				<PrivateStorage v-if="isOpenAIAssistantWorkflow" :agent-name="agentName" />
				<PrivateStorage v-if="isFoundationaLLMFunctionCallingWorkflow" :agent-name="agentName" :tools="agentTools.map(tool => tool.name)" />

				<!-- Edit access control -->
				<AccessControl
					v-if="editAgent"
					:scopes="[
						{
							label: 'Agent',
							value: `providers/FoundationaLLM.Agent/agents/${agentName}`,
						},
						{
							label: 'Prompt',
							value: `providers/FoundationaLLM.Prompt/prompts/${agentPrompt?.resource?.name}`,
						},
					]"
				/>
			</div>
		</div>

		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite" aria-label="Loading content">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="col-span-2">
				<div id="aria-agent-name" class="step-header !mb-2">Agent name:</div>
				<div id="aria-agent-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="agentName"
						:disabled="editAgent"
						type="text"
						class="w-full"
						placeholder="Enter agent name"
						aria-labelledby="aria-agent-name aria-agent-name-desc"
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
						class="icon invalid"
						:title="validationMessage"
						:aria-label="validationMessage"
					>
						❌
					</span>
				</div>
				<div v-if="showValidationErrors && formErrors.agentName" class="error-message">{{ formErrors.agentName }}</div>
				<div v-if="showValidationErrors && formErrors.nameValidation" class="error-message">{{ formErrors.nameValidation }}</div>
			</div>

			<div class="col-span-2">
				<div class="step-header !mb-2">Agent Display Name:</div>
				<div class="mb-2">
					This is the name that will be displayed to users when interacting with the agent.
				</div>
				<InputText
					v-model="agentDisplayName"
					type="text"
					:class="['w-full', { 'input-error': formErrors.agentDisplayName }]"
					placeholder="Enter agent display name"
				/>
				<div v-if="formErrors.agentDisplayName" class="error-message">{{ formErrors.agentDisplayName }}</div>
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
			<div class="col-span-2">
				<div class="step-header !mb-2">Welcome message:</div>
				<div id="aria-welcome-message-desc" class="mb-2">
					Provide a message to display when a user starts a new conversation with the agent. If a
					message is not provided, the default welcome message will be displayed.
				</div>
				<CustomQuillEditor
					v-model="agentWelcomeMessage"
					:initial-content="JSON.parse(JSON.stringify(agentWelcomeMessage))"
					class="w-full"
					placeholder="Enter agent welcome message"
					aria-labelledby="aria-welcome-message-desc"
					@content-update="updateAgentWelcomeMessage($event)"
				/>
			</div>

			<!-- Agent configuration -->
			<section aria-labelledby="agent-configuration" class="col-span-2 steps">
				<h3 id="agent-configuration" class="step-section-header col-span-2">Agent Configuration</h3>

				<div class="step-header">Should conversations be included in the context?</div>
				<div class="step-header">How should user-agent interactions be gated?</div>

				<!-- Conversation history -->
				<CreateAgentStepItem focus-query=".conversation-history-toggle input">
					<div class="step-container__header">Conversation History</div>

					<div>
						<span class="step-option__header">Enabled:</span>
						<span>
							<span>{{ conversationHistory ? 'Yes' : 'No' }}</span>
							<span
								v-if="conversationHistory"
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
						<span>{{ conversationMaxMessages }}</span>
					</div>

					<template #edit>
						<div id="aria-conversation-history" class="step-container__header">
							Conversation History
						</div>

						<div class="flex items-center mt-2">
							<span id="aria-conversation-history-enabled" class="step-option__header"
								>Enabled:</span
							>
							<span>
								<ToggleButton
									v-model="conversationHistory"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-conversation-history aria-conversation-history-enabled"
									class="conversation-history-toggle"
								/>
							</span>
						</div>

						<div>
							<span id="aria-max-messages" class="step-option__header">Max Messages:</span>
							<InputText
								v-model="conversationMaxMessages"
								type="number"
								class="mt-2"
								aria-label="aria-max-messages"
							/>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Gatekeeper -->
				<CreateAgentStepItem focus-query=".gatekeeper-toggle input">
					<div class="step-container__header">Gatekeeper</div>

					<div>
						<span class="step-option__header">Use system default:</span>
						<span>
							<span>{{ gatekeeperUseSystemDefault ? 'Yes' : 'No' }}</span>
							<span
								v-if="gatekeeperUseSystemDefault"
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

					<div v-if="!gatekeeperUseSystemDefault">
						<span class="step-option__header">Content Safety:</span>
						<span>{{
							Array.isArray(selectedGatekeeperContentSafety)
								? selectedGatekeeperContentSafety.map((item) => item.name).join(', ')
								: ''
						}}</span>
					</div>

					<div v-if="!gatekeeperUseSystemDefault">
						<span class="step-option__header">Data Protection:</span>
						<span>{{
							Array.isArray(selectedGatekeeperDataProtection)
								? selectedGatekeeperDataProtection.map((item) => item.name).join(', ')
								: ''
						}}</span>
					</div>

					<template #edit>
						<div id="aria-gatekeeper" class="step-container__header">Gatekeeper</div>

						<!-- Gatekeeper toggle -->
						<div class="flex items-center mt-2">
							<span id="aria-gatekeeper-enabled" class="step-option__header"
								>Use system default:</span
							>
							<span>
								<ToggleButton
									v-model="gatekeeperUseSystemDefault"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-gatekeeper aria-gatekeeper-enabled"
									class="gatekeeper-toggle"
								/>
							</span>
						</div>

						<!-- Content safety -->
						<div v-if="!gatekeeperUseSystemDefault" class="mt-2">
							<span id="aria-content-safety" class="step-option__header">Content Safety:</span>
							<MultiSelect
								v-model="selectedGatekeeperContentSafety"
								class="dropdown--agent"
								:options="gatekeeperContentSafetyOptions"
								option-label="name"
								display="chip"
								placeholder="--Select--"
								aria-labelledby="aria-content-safety"
							/>
						</div>

						<!-- Data protection -->
						<div v-if="!gatekeeperUseSystemDefault" class="mt-2">
							<span id="aria-data-prot" class="step-option__header">Data Protection:</span>
							<!-- <span>Microsoft Presidio</span> -->
							<MultiSelect
								v-model="selectedGatekeeperDataProtection"
								class="dropdown--agent"
								:options="gatekeeperDataProtectionOptions"
								option-label="name"
								display="chip"
								placeholder="--Select--"
								aria-labelledby="aria-data-prot"
							/>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- <div class="step-header">Which AI model should the orchestrator use?</div>
				<div class="step-header">Which capabilities should the agent have?</div> -->

				<!-- AI model -->
				<!-- <CreateAgentStepItem v-model="editAIModel" focusQuery=".step-container__edit__option">
					<template v-if="selectedAIModel">
						<div v-if="selectedAIModel.object_id !== ''">
							<div class="step-container__header">{{ selectedAIModel.name }}</div>
							<div>
								<span class="step-option__header">Deployment name:</span>
								<span>{{ selectedAIModel.deployment_name }}</span>
							</div>
						</div>
					</template>
					<template v-else>Please select an AI model.</template>

					<template #edit>
						<div class="step-container__edit__header">Please select an AI model.</div>
						<div
							v-for="aiModel in aiModelOptions"
							:key="aiModel.name"
							class="step-container__edit__option"
							:class="{
								'step-container__edit__option--selected': aiModel.name === selectedAIModel?.name,
							}"
							tabindex="0"
							@click.stop="handleAIModelSelected(aiModel)"
							@keydown.enter="handleAIModelSelected(aiModel)"
						>
							<div v-if="aiModel.object_id !== ''">
								<div class="step-container__header">{{ aiModel.name }}</div>
								<div v-if="aiModel.deployment_name">
									<span class="step-option__header">Deployment name:</span>
									<span>{{ aiModel.deployment_name }}</span>
								</div>
							</div>
						</div>
					</template>
				</CreateAgentStepItem> -->

				<!-- Agent capabilities -->
				<!-- <CreateAgentStepItem focusQuery=".agent-capabilities-dropdown input">
					<div>
						<span class="step-option__header">Agent Capabilities:</span>
						<span>{{
							Array.isArray(selectedAgentCapabilities)
								? selectedAgentCapabilities.map((item) => item.name).join(', ')
								: ''
						}}</span>
					</div>

					<template #edit>
						<div class="mt-2">
							<span class="step-option__header">Agent Capabilities:</span>
							<MultiSelect
								v-model="selectedAgentCapabilities"
								class="dropdown--agent agent-capabilities-dropdown"
								:options="agentCapabilitiesOptions"
								option-label="name"
								display="chip"
								placeholder="--Select--"
								aria-labelledby="aria-content-safety"
							/>
						</div>
					</template>
				</CreateAgentStepItem> -->

				<div class="step-header">Should user prompts be rewritten?</div>
				<div class="step-header">Should semantic cache be used?</div>

				<!-- User prompt rewrite -->
				<CreateAgentStepItem focus-query=".user-prompt-rewrite-toggle input">
					<div class="step-container__header">User Prompt Rewrite</div>

					<div>
						<span class="step-option__header">Enabled:</span>
						<span>
							<span>{{ userPromptRewriteEnabled ? 'Yes' : 'No' }}</span>
							<span
								v-if="userPromptRewriteEnabled"
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

					<template v-if="userPromptRewriteEnabled">
						<div>
							<span class="step-option__header">Rewrite Model:</span>
							<span>{{
								aiModelOptions.find((model) => model.object_id === userPromptRewriteAIModel)?.name
							}}</span>
						</div>

						<div>
							<span class="step-option__header">Rewrite Prompt:</span>
							<span>{{
								promptOptions.find((prompt) => prompt.object_id === userPromptRewritePrompt)?.name
							}}</span>
						</div>

						<div>
							<span class="step-option__header">Rewrite Window Size:</span>
							<span>{{ userPromptRewriteWindowSize }}</span>
						</div>
					</template>

					<template #edit>
						<div id="aria-gatekeeper" class="step-container__header">User Prompt Rewrite</div>

						<!-- User prompt rewrite toggle -->
						<div class="flex items-center mt-2">
							<span id="aria-user-prompt-rewrite-enabled" class="step-option__header"
								>Enabled:</span
							>
							<span>
								<ToggleButton
									v-model="userPromptRewriteEnabled"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-user-prompt-rewrite-enabled"
									class="user-prompt-rewrite"
								/>
							</span>
						</div>

						<!-- User prompt rewrite model -->
						<div v-if="userPromptRewriteEnabled" class="mt-2">
							<!-- What model should be used for the prompt rewrite? -->
							<span id="aria-user-prompt-rewrite-model" class="step-option__header"
								>Rewrite Model:</span
							>
							<Dropdown
								v-model="userPromptRewriteAIModel"
								:options="aiModelOptions"
								option-label="name"
								option-value="object_id"
								class="dropdown--agent"
								placeholder="--Select--"
								aria-labelledby="aria-user-prompt-rewrite-model"
							/>
							<div v-if="showValidationErrors && formErrors.userPromptRewriteAIModel" class="error-message">{{ formErrors.userPromptRewriteAIModel }}</div>
						</div>

						<!-- User prompt rewrite prompt -->
						<div v-if="userPromptRewriteEnabled" class="mt-2">
							<!-- What prompt should be used to rewrite the user prompt? -->
							<span id="aria-user-prompt-rewrite-prompt" class="step-option__header"
								>Rewrite Prompt:</span
							>
							<Dropdown
								v-model="userPromptRewritePrompt"
								:options="promptOptions"
								option-label="name"
								option-value="object_id"
								class="dropdown--agent"
								placeholder="--Select--"
								aria-labelledby="aria-user-prompt-rewrite-prompt"
							/>
							<div v-if="showValidationErrors && formErrors.userPromptRewritePrompt" class="error-message">{{ formErrors.userPromptRewritePrompt }}</div>
						</div>

						<!-- User prompt rewrite window size -->
						<div v-if="userPromptRewriteEnabled" class="mt-2">
							<!-- What should the rewrite window size be? -->
							<span id="aria-user-prompt-rewrite-window-size" class="step-option__header"
								>Rewrite Window Size:</span
							>
							<InputNumber
								v-model="userPromptRewriteWindowSize"
								:min-fraction-digits="0"
								:max-fraction-digits="0"
								placeholder="Window size"
								aria-labelledby="aria-user-prompt-rewrite-window-size"
							/>
							<div v-if="showValidationErrors && formErrors.userPromptRewriteWindowSize" class="error-message">{{ formErrors.userPromptRewriteWindowSize }}</div>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Semantic cache  -->
				<CreateAgentStepItem focus-query=".semantic-cache-toggle input">
					<div class="step-container__header">Semantic Cache</div>

					<div>
						<span class="step-option__header">Enabled:</span>
						<span>
							<span>{{ semanticCacheEnabled ? 'Yes' : 'No' }}</span>
							<span
								v-if="semanticCacheEnabled"
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

					<template v-if="semanticCacheEnabled">
						<div>
							<span class="step-option__header">Model:</span>
							<span>{{
								aiModelOptions.find((model) => model.object_id === semanticCacheAIModel)?.name
							}}</span>
						</div>

						<div>
							<span class="step-option__header">Embedding Dimensions:</span>
							<span>{{ semanticCacheEmbeddingDimensions }}</span>
						</div>

						<div>
							<span class="step-option__header">Minimum Similarity Threshold:</span>
							<span>{{ semanticCacheMinimumSimilarityThreshold }}</span>
						</div>
					</template>

					<template #edit>
						<div id="aria-gatekeeper" class="step-container__header">Semantic Cache</div>

						<!-- Semantic cache toggle -->
						<div class="flex items-center mt-2">
							<span id="aria-semantic-cache-enabled" class="step-option__header">Enabled:</span>
							<span>
								<ToggleButton
									v-model="semanticCacheEnabled"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-semantic-cache-enabled"
									class="semantic-cache-toggle"
								/>
							</span>
						</div>

						<!-- Semantic cache model -->
						<div v-if="semanticCacheEnabled" class="mt-2">
							<!-- What model should be used for the semantic cache? -->
							<span id="aria-semantic-cache-model" class="step-option__header">Model:</span>
							<Dropdown
								v-model="semanticCacheAIModel"
								:options="aiModelOptions"
								option-label="name"
								option-value="object_id"
								class="dropdown--agent"
								placeholder="--Select--"
								aria-labelledby="aria-semantic-cache-model"
							/>
							<div v-if="showValidationErrors && formErrors.semanticCacheAIModel" class="error-message">{{ formErrors.semanticCacheAIModel }}</div>
						</div>

						<!-- Semantic cache embedding dimensions -->
						<div v-if="semanticCacheEnabled" class="mt-2">
							<!-- How many embedding dimensions to use? -->
							<span id="aria-semantic-cache-embedding-dimensions" class="step-option__header"
								>Embedding Dimensions:</span
							>
							<InputNumber
								v-model="semanticCacheEmbeddingDimensions"
								:min-fraction-digits="0"
								:max-fraction-digits="0"
								placeholder="Embedding dimensions size"
								aria-labelledby="aria-semantic-cache-embedding-dimensions"
							/>
							<div v-if="showValidationErrors && formErrors.semanticCacheEmbeddingDimensions" class="error-message">{{ formErrors.semanticCacheEmbeddingDimensions }}</div>
						</div>

						<!-- Semantic cache minimum similarity threshold -->
						<div v-if="semanticCacheEnabled" class="mt-2">
							<!-- What should the minimum similarity threshold be? -->
							<span id="aria-semantic-cache-minimum-similarity" class="step-option__header"
								>Minimum Similarity Threshold:</span
							>
							<InputNumber
								v-model="semanticCacheMinimumSimilarityThreshold"
								:min-fraction-digits="0"
								:max-fraction-digits="2"
								placeholder="Minimum Similarity Threshold"
								aria-labelledby="aria-semantic-cache-minimum-similarity"
							/>
							<div v-if="showValidationErrors && formErrors.semanticCacheMinimumSimilarityThreshold" class="error-message">{{ formErrors.semanticCacheMinimumSimilarityThreshold }}</div>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Realtime Speech Configuration -->
				<CreateAgentStepItem focus-query=".realtime-speech-toggle input">
					<div class="step-container__header">Realtime Speech Configuration</div>

					<div>
						<span class="step-option__header">Enabled:</span>
						<span>
							<span>{{ realtimeSpeechEnabled ? 'Yes' : 'No' }}</span>
							<span
								v-if="realtimeSpeechEnabled"
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

					<template v-if="realtimeSpeechEnabled">
						<div>
							<span class="step-option__header">Model:</span>
							<span>{{
								realtimeSpeechModelOptions.find((model) => model.object_id === realtimeSpeechAIModel)?.name
							}}</span>
						</div>

						<div>
							<span class="step-option__header">Stop Words:</span>
							<span>{{ realtimeSpeechStopWords.join(', ') }}</span>
						</div>

						<div>
							<span class="step-option__header">Show Transcriptions:</span>
							<span>{{ realtimeSpeechShowTranscriptions ? 'Yes' : 'No' }}</span>
						</div>

						<div>
							<span class="step-option__header">Include Conversation History:</span>
							<span>{{ realtimeSpeechIncludeHistory ? 'Yes' : 'No' }}</span>
						</div>
					</template>

					<template #edit>
						<div id="aria-realtime-speech-config" class="step-container__header">Realtime Speech Configuration</div>

						<!-- Realtime speech toggle -->
						<div class="flex items-center mt-2">
							<span id="aria-realtime-speech-enabled" class="step-option__header">Enabled:</span>
							<span>
								<ToggleButton
									v-model="realtimeSpeechEnabled"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-realtime-speech-enabled"
									class="realtime-speech-toggle"
								/>
							</span>
						</div>

						<!-- Realtime speech model -->
						<div v-if="realtimeSpeechEnabled" class="mt-2">
							<span id="aria-realtime-speech-model" class="step-option__header">Model:</span>
							<Dropdown
								v-model="realtimeSpeechAIModel"
								:options="realtimeSpeechModelOptions"
								option-label="name"
								option-value="object_id"
								class="dropdown--agent"
								placeholder="--Select--"
								aria-labelledby="aria-realtime-speech-model"
							/>
							<div v-if="showValidationErrors && formErrors.realtimeSpeechAIModel" class="error-message">{{ formErrors.realtimeSpeechAIModel }}</div>
						</div>

						<!-- Stop words -->
						<div v-if="realtimeSpeechEnabled" class="mt-2">
							<span id="aria-realtime-speech-stop-words" class="step-option__header">Stop Words (comma-separated):</span>
							<InputText
								v-model="realtimeSpeechStopWordsInput"
								type="text"
								class="w-full"
								placeholder="stop, end conversation, goodbye"
								aria-labelledby="aria-realtime-speech-stop-words"
								@update:model-value="updateRealtimeSpeechStopWords"
							/>
						</div>

						<!-- Show transcriptions -->
						<div v-if="realtimeSpeechEnabled" class="flex items-center mt-2">
							<span id="aria-realtime-speech-show-transcriptions" class="step-option__header">Show Transcriptions:</span>
							<span>
								<ToggleButton
									v-model="realtimeSpeechShowTranscriptions"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-realtime-speech-show-transcriptions"
								/>
							</span>
						</div>

						<!-- Include conversation history -->
						<div v-if="realtimeSpeechEnabled" class="flex items-center mt-2">
							<span id="aria-realtime-speech-include-history" class="step-option__header">Include Conversation History:</span>
							<span>
								<ToggleButton
									v-model="realtimeSpeechIncludeHistory"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-realtime-speech-include-history"
								/>
							</span>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Cost center -->
				<div id="aria-cost-center" class="step-header col-span-2">
					Would you like to assign this agent to a cost center?
				</div>
				<div class="col-span-2">
					<InputText
						v-model="cost_center"
						type="text"
						class="w-50"
						placeholder="Enter cost center name"
						aria-labelledby="aria-cost-center"
					/>
				</div>

				<!-- Expiration -->
				<div class="step-header col-span-2">Would you like to set an expiration on this agent?</div>
				<div class="col-span-2">
					<Calendar
						v-model="expirationDate"
						show-icon
						show-button-bar
						placeholder="Enter expiration date"
						type="text"
					/>
				</div>
			</section>

			<!-- User portal experience -->
			<section aria-labelledby="user-portal-experience" class="col-span-2 steps">
				<h3 id="user-portal-experience" class="step-section-header col-span-2">
					User Portal Experience
				</h3>

				<div id="aria-show-message-tokens" class="step-header">
					Would you like to show the message tokens?
				</div>
				<div id="aria-show-message-rating" class="step-header">
					Would you like to allow the user to rate the agent responses?
				</div>

				<!-- Message tokens -->
				<div>
					<ToggleButton
						v-model="showMessageTokens"
						on-label="Yes"
						on-icon="pi pi-check-circle"
						off-label="No"
						off-icon="pi pi-times-circle"
						aria-labelledby="aria-show-message-tokens"
					/>
				</div>

				<!-- Rate messages -->
				<div>
					<ToggleButton
						v-model="showMessageRating"
						on-label="Yes"
						on-icon="pi pi-check-circle"
						off-label="No"
						off-icon="pi pi-times-circle"
						aria-labelledby="aria-show-message-rating"
					/>
				</div>

				<div id="aria-show-view-prompt" class="step-header">
					Would you like to allow the user to see the message prompts?
				</div>
				<div id="aria-show-file-upload" class="step-header">
					Would you like to allow the user to upload files?
				</div>

				<!-- Show view prompt -->
				<div>
					<ToggleButton
						v-model="showViewPrompt"
						on-label="Yes"
						on-icon="pi pi-check-circle"
						off-label="No"
						off-icon="pi pi-times-circle"
						aria-labelledby="aria-show-view-prompt"
					/>
				</div>

				<!-- Show file upload -->
				<div>
					<ToggleButton
						v-model="showFileUpload"
						on-label="Yes"
						on-icon="pi pi-check-circle"
						off-label="No"
						off-icon="pi pi-times-circle"
						aria-labelledby="aria-show-file-upload"
					/>
				</div>

				<div id="aria-show-content-artifacts" class="step-header">
					Would you like to show content artifacts in messages?
				</div>
				<div class="step-header">
					<!-- Empty placeholder for grid alignment -->
				</div>

				<!-- Show content artifacts -->
				<div>
					<ToggleButton
						v-model="showContentArtifacts"
						on-label="Yes"
						on-icon="pi pi-check-circle"
						off-label="No"
						off-icon="pi pi-times-circle"
						aria-labelledby="aria-show-content-artifacts"
					/>
				</div>
				<div>
					<!-- Empty placeholder for grid alignment -->
				</div>
			</section>

			<!-- Workflow -->
			<div class="step-section-header col-span-2">Workflow</div>
			<div id="aria-workflow" class="step-header col-span-2">
				What workflow should the agent use?
			</div>

			<!-- Workflow selection -->
			<div class="col-span-2">
				<Dropdown
					:model-value="selectedWorkflow?.type"
					:options="workflowOptions"
					option-label="name"
					option-value="type"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-workflow"
					@change="handleWorkflowSelection"
				/>
				<Button
					class="ml-2"
					severity="primary"
					:label="showWorkflowConfiguration ? 'Hide Workflow Configuration' : 'Configure Workflow'"
					:disabled="!selectedWorkflow?.type"
					@click="showWorkflowConfiguration = !showWorkflowConfiguration"
				/>
				<div v-if="showValidationErrors && formErrors.selectedWorkflow" class="error-message">{{ formErrors.selectedWorkflow }}</div>
			</div>

			<!-- Workflow configuration -->
					<div v-if="showWorkflowConfiguration" class="col-span-2">
				<!-- Workflow name -->
				<div class="mb-6">
					<div id="aria-workflow-name" class="step-header !mb-3">Workflow name:</div>
					<InputText
						v-model="workflowName"
						type="text"
						class="w-50"
						placeholder="Enter workflow name"
						aria-labelledby="aria-workflow-name"
					/>
						<div v-if="showValidationErrors && formErrors.workflowName" class="error-message">{{ formErrors.workflowName }}</div>
				</div>

				<!-- Workflow package name -->
				<div class="mb-6">
					<div id="aria-workflow-package-name" class="step-header !mb-3">
						Workflow package name:
					</div>
					<InputText
						v-model="workflowPackageName"
						type="text"
						class="w-50"
						placeholder="Enter workflow package name"
						aria-labelledby="aria-workflow-package-name"
					/>
						<div v-if="showValidationErrors && formErrors.workflowPackageName" class="error-message">{{ formErrors.workflowPackageName }}</div>
				</div>

				<!-- Workflow class name -->
				<div class="mb-6">
					<div id="aria-workflow-class-name" class="step-header !mb-3">Workflow class name:</div>
					<InputText
						v-model="workflowClassName"
						type="text"
						class="w-50"
						placeholder="Enter workflow class name"
						aria-labelledby="aria-workflow-class-name"
					/>
				</div>

				<!-- Workflow host -->
				<div class="mb-6">
					<div id="aria-workflow-host" class="step-header !mb-3">Workflow host:</div>
					<div class="col-span-2">
						<Dropdown
							v-model="workflowHost"
							:options="orchestratorOptions"
							option-label="label"
							option-value="value"
							class="dropdown--agent"
							placeholder="--Select--"
							aria-labelledby="aria-workflow-host"
						/>
						<div v-if="showValidationErrors && formErrors.workflowHost" class="error-message">{{ formErrors.workflowHost }}</div>
					</div>
				</div>

				<!-- Workflow main model -->
				<div class="mb-6">
					<div id="aria-workflow-model" class="step-header !mb-3">Workflow main model:</div>
					<Dropdown
						:model-value="workflowMainAIModel?.object_id"
						:options="aiModelOptions"
						option-label="name"
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
						<div v-if="showValidationErrors && formErrors.workflowMainAIModel" class="error-message">{{ formErrors.workflowMainAIModel }}</div>
				</div>

				<!-- Workflow main model parameters -->
				<div class="mb-6">
					<div class="step-header !mb-3">Workflow main model parameters:</div>
					<PropertyBuilder v-model="workflowMainAIModelParameters" />
				</div>

				<!-- Workflow main prompt -->
				<div class="mb-6">
					<div id="aria-persona" class="step-header !mb-3">What is the main workflow prompt?</div>
					<div class="col-span-2">
						<Textarea
							v-model="systemPrompt"
							class="w-full"
							auto-resize
							rows="5"
							type="text"
							placeholder="You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional."
							aria-labelledby="aria-persona"
						/>
						<div v-if="showValidationErrors && formErrors.systemPrompt" class="error-message">{{ formErrors.systemPrompt }}</div>
					</div>
				</div>

				<!-- Workflow additional resources -->
				<div class="mb-6">
					<div class="step-header !mb-3">Additional workflow resources:</div>
					<ResourceTable
						:resources="workflowExtraResources"
						@delete="workflowResourceToDelete = $event"
					/>
					<CreateResourceObjectDialog
						v-if="showCreateWorkflowResourceObjectDialog"
						:visible="showCreateWorkflowResourceObjectDialog"
						@update:visible="showCreateWorkflowResourceObjectDialog = false"
						@update:model-value="handleAddWorkflowResource"
					/>
					<ConfirmationDialog
						v-if="workflowResourceToDelete !== null"
						:visible="workflowResourceToDelete !== null"
						header="Delete Workflow Resource"
						confirmText="Yes"
						@cancel="workflowResourceToDelete = null"
						@confirm="handleDeleteWorkflowResource"
					>
						<div>
							Are you sure you want to delete the "{{
								getResourceNameFromId(workflowResourceToDelete!.object_id)
							}}" workflow resource?
						</div>
					</ConfirmationDialog>
					<div class="flex justify-end mt-4">
						<Button
							severity="primary"
							label="Add Workflow Resource"
							@click="showCreateWorkflowResourceObjectDialog = true"
						/>
					</div>
				</div>
			</div>

			<!-- Tools -->
			<div class="step-section-header col-span-2">Tools</div>
			<div id="aria-orchestrator" class="step-header col-span-2">
				What tools should the agent use?
			</div>

			<!-- Tools table -->
			<div class="col-span-2">
				<DataTable
					:value="agentTools"
					striped-rows
					scrollable
					table-style="max-width: 100%"
					size="small"
				>
					<template #empty>No agent tools added.</template>

					<template #loading>Loading agent tools. Please wait.</template>

					<!-- Tool name -->
					<Column
						field="name"
						header="Name"
						sortable
						:pt="{
							headerCell: {
								style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
							},
							sortIcon: { style: { color: 'var(--primary-text)' } },
						}"
					/>

					<!-- Tool package name -->
					<Column
						field="package_name"
						header="Package Name"
						sortable
						:pt="{
							headerCell: {
								style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
							},
							sortIcon: { style: { color: 'var(--primary-text)' } },
						}"
					/>

					<!-- Edit tool -->
					<Column
						header="Edit"
						header-style="width:6rem"
						style="text-align: center"
						:pt="{
							headerCell: {
								style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
							},
							headerContent: { style: { justifyContent: 'center' } },
						}"
					>
						<template #body="{ data }">
							<Button link @click="toolToEdit = data">
								<i class="pi pi-cog" style="font-size: 1.2rem"></i>
							</Button>

							<ConfigureToolDialog
								v-if="toolToEdit?.name === data.name"
								:model-value="toolToEdit"
								:visible="!!toolToEdit"
								:existing-tools="agentTools"
								@update:visible="toolToEdit = null"
								@update:model-value="handleUpdateTool"
							/>
						</template>
					</Column>

					<!-- Delete tool -->
					<Column
						header="Delete"
						header-style="width:6rem"
						style="text-align: center"
						:pt="{
							headerCell: {
								style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
							},
							headerContent: { style: { justifyContent: 'center' } },
						}"
					>
						<template #body="{ data }">
							<Button link @click="toolToRemove = data">
								<i class="pi pi-trash" style="font-size: 1.2rem"></i>
							</Button>

							<ConfirmationDialog
								v-if="toolToRemove !== null"
								:visible="toolToRemove !== null"
								header="Delete Tool"
								confirmText="Yes"
								@cancel="toolToRemove = null"
								@confirm="handleRemoveTool"
							>
								<div>
									Are you sure you want to delete the "{{ toolToRemove!.name }}" tool from this
									agent?
								</div>
							</ConfirmationDialog>
						</template>
					</Column>
				</DataTable>

				<!-- Add new tool -->
				<div class="flex justify-end mt-4">
					<Button @click="showNewToolDialog = true">Add New Tool</Button>
				</div>

				<ConfigureToolDialog
					v-if="showNewToolDialog"
					:visible="!!showNewToolDialog"
					:existing-tools="agentTools"
					@update:visible="showNewToolDialog = false"
					@update:model-value="handleAddNewTool"
				/>
			</div>

			<!-- Security -->
			<div v-if="virtualSecurityGroupId" class="step-section-header col-span-2">Security</div>

			<!-- Virtual security group id -->
			<template v-if="virtualSecurityGroupId">
				<div class="step-header">Virtual security group ID</div>
				<div class="col-span-2 flex gap-4">
					<InputText
						:value="virtualSecurityGroupId"
						disabled
						type="text"
						class="w-50"
						placeholder="Enter cost center name"
						aria-labelledby="aria-cost-center"
					/>
					<Button label="Copy" severity="primary" @click="handleCopySecurityGroupId" />
				</div>
			</template>

			<!-- Access tokens -->
			<template v-if="virtualSecurityGroupId">
				<div class="step-header">Agent access tokens</div>
				<div class="col-span-2">
					<AgentAccessTokens :agent-name="agentName" />
				</div>
			</template>

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
import api from '@/js/api';
import type {
	Agent,
	AgentDataSource,
	AgentIndex,
	AgentTool,
	AIModel,
	CreateAgentRequest,
	DataSource,
	ExternalOrchestrationService,
	Prompt,
	TextEmbeddingProfile,
	Workflow
} from '@/js/types';
import { useConfirmationStore } from '@/stores/confirmationStore';
import { CronLight } from '@vue-js-cron/light';
import '@vue-js-cron/light/dist/light.css';
import { clone, debounce } from 'lodash';
import type { PropType } from 'vue';
import { ref } from 'vue';
const defaultSystemPrompt: string = '';

const getDefaultFormValues = () => {
	return {
		accessControlModalOpen: false,

		agentName: '',
		agentDescription: '',
		agentDisplayName: '',
		agentWelcomeMessage: '',
		object_id: '',
		prompt_object_id: '',
		inline_context: true,
		agentType: 'generic-agent' as CreateAgentRequest['type'],

		cost_center: '',
		expirationDate: null as string | null,

		// editAIModel: false as boolean,
		selectedAIModel: null as null | AIModel,

		error: '',

		conversationHistory: false as boolean,
		conversationMaxMessages: 5 as number,

		gatekeeperUseSystemDefault: false as boolean,

		selectedGatekeeperContentSafety: ref(),
		selectedGatekeeperDataProtection: ref(),
		gatekeeperContentSafety: { label: 'None', value: null },
		gatekeeperDataProtection: { label: 'None', value: null },

		// selectedAgentCapabilities: ref(),
		agentCapabilities: { label: 'None', value: null },

		systemPrompt: defaultSystemPrompt as string,
		agentPrompt: null as null | Prompt,

		// orchestration_settings: {
		// 	orchestrator: 'LangChain' as string,
		// },

		selectedWorkflow: null,

		toolToEdit: null,
		toolToRemove: null,
		agentTools: [] as AgentTool[],

		showMessageTokens: true as boolean,
		showMessageRating: true as boolean,
		showViewPrompt: true as boolean,
		showFileUpload: false as boolean,
		showContentArtifacts: true as boolean,

		userPromptRewriteEnabled: false as boolean,
		realtimeSpeechEnabled: false as boolean,
		realtimeSpeechAIModel: null as string | null,
		realtimeSpeechModelOptions: [] as Array<{ name: string; object_id: string }>,
		realtimeSpeechStopWords: ['stop', 'end conversation', 'goodbye'] as string[],
		realtimeSpeechStopWordsInput: 'stop, end conversation, goodbye' as string,
		realtimeSpeechShowTranscriptions: true as boolean,
		realtimeSpeechIncludeHistory: true as boolean,
		userPromptRewriteAIModel: null as string | null,
		userPromptRewritePrompt: null as string | null,
		userPromptRewriteWindowSize: 3 as number,

		semanticCacheEnabled: false as boolean,
		semanticCacheAIModel: null as string | null,
		semanticCacheEmbeddingDimensions: 2048 as number,
		semanticCacheMinimumSimilarityThreshold: 0.97 as number,

		inheritable_authorizable_actions: [
			"FoundationaLLM.Agent/workflows/read",
			"FoundationaLLM.Agent/tools/read",
			"FoundationaLLM.Prompt/prompts/read",
			"FoundationaLLM.AIModel/aiModels/read",
			"FoundationaLLM.Configuration/apiEndpointConfigurations/read",
			"FoundationaLLM.DataPipeline/dataPipelines/read",
			"FoundationaLLM.Vector/vectorDatabases/read",
			"FoundationaLLM.Context/knowledgeSources/read",
			"FoundationaLLM.Context/knowledgeUnits/read",
			"FoundationaLLM.DataSource/dataSources/read",
			"FoundationaLLM.AzureAI/projects/read"
		] as string[]
	};
};

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
			...getDefaultFormValues(),
			formErrors: {} as Record<string, string>,
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			showValidationErrors: false as boolean,
			editable: false as boolean,
			hasAgentPrivateStorage: false as boolean,
			isOpenAIAssistantWorkflow: false as boolean,
			isFoundationaLLMFunctionCallingWorkflow: false as boolean,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: '' as string,

			aiModelOptions: [] as AIModel[],

			workflowOptions: [] as Workflow[],
			showWorkflowConfiguration: false,
			workflowMainAIModel: null as AIModel | null,
			// workflowMainPrompt: '' as string,
			workflowMainAIModelParameters: {} as object,
			workflowAssistantId: '' as string,
			workflowVectorStoreId: '' as string,
			workflowName: '' as string,
			workflowPackageName: 'FoundationaLLM' as string,
			workflowClassName: '' as string,
			workflowHost: '' as string,
			workflowExtraResources: {},
			showCreateWorkflowResourceObjectDialog: false,
			workflowResourceToDelete: null,

			virtualSecurityGroupId: null as string | null,

			showNewToolDialog: false,

			promptOptions: [] as Prompt[],
			promptCache: new Map<string, Prompt[]>(), // Cache for different contexts
			promptLoadingStates: new Map<string, boolean>(), // Track loading states

			orchestratorOptions: [
				{
					label: 'LangChain',
					value: 'LangChain',
				}
			],

			gatekeeperContentSafetyOptions: ref([
				{
					name: 'Azure Content Safety',
					code: 'AzureContentSafety',
				},
				{
					name: 'Azure Content Safety Prompt Shield',
					code: 'AzureContentSafetyPromptShield',
				},
				{
					name: 'Lakera Guard',
					code: 'LakeraGuard',
				},
				{
					name: 'Enkrypt Guardrails',
					code: 'EnkryptGuardrails',
				},
			]),

			gatekeeperDataProtectionOptions: ref([
				{
					name: 'Microsoft Presidio',
					code: 'MicrosoftPresidio',
				},
			]),

			// agentCapabilitiesOptions: ref([
			// 	{
			// 		name: 'OpenAI Assistants',
			// 		code: 'OpenAI.Assistants',
			// 	},
			// 	{
			// 		name: 'FLLM Knowledge Management',
			// 		code: 'FoundationaLLM.KnowledgeManagement',
			// 	},
			// ]),
		};
	},

	watch: {
		  $data: {
			handler(newVal) {
			Object.keys(this.formErrors).forEach((field) => {
				if (newVal[field] !== undefined && this.formErrors[field]) {
				//  Change detected in the field with an error
				if (newVal[field] !== '' && newVal[field] !== null) {
					this.formErrors[field] = '';
				}
				}
			});
			},
			deep: true
		},
		selectedWorkflow() {
			this.workflowName = this.workflowName || this.selectedWorkflow?.name;
			this.workflowHost = this.selectedWorkflow?.workflow_host ?? this.workflowHost;

			// Only set the class name to the selected workflow name if it doesn't already have a custom value
			const workflowClassNameIsResourceDefault = this.workflowOptions.some(
				(workflowOption) => this.workflowClassName === workflowOption.name,
			);
			if (workflowClassNameIsResourceDefault || !this.workflowClassName) {
				this.workflowClassName = this.selectedWorkflow?.name ?? '';
			}

			// if (this.selectedWorkflow?.resource_object_ids) {
			// 	const existingMainModel = Object.values(this.selectedWorkflow.resource_object_ids).find(
			// 		(resource) => resource.properties?.object_role === 'main_model',
			// 	);
			// 	this.workflowMainAIModel = existingMainModel ?? null;
			// } else {
			// 	this.workflowMainAIModel = null;
			// }

			this.showWorkflowConfiguration = true;

			// if (!this.selectedWorkflow?.type) {
			// 	this.showWorkflowConfiguration = false;
			// 	this.selectedWorkflow = null;
			// }
		},

		workflowMainAIModel() {
			const mainModel = this.workflowMainAIModel;
			const existingMainModelParamters =
				mainModel?.model_parameters ?? mainModel?.properties?.model_parameters;
			this.workflowMainAIModelParameters = existingMainModelParamters ?? {};
		},

		// Watch for changes to userPromptRewriteEnabled to load prompts lazily
		userPromptRewriteEnabled() {
			this.loadPromptsIfNeeded('userPromptRewrite');
		},
	},

	async created() {
		this.loading = true;
		// Uncomment to remove mock loading screen
		// api.mockLoadTime = 0;

		try {

			this.loadingStatusText = 'Retrieving AI models...';
			const aiModelsResult = await api.getAIModels();
			this.aiModelOptions = aiModelsResult.map((result) => result.resource);
			// Filter the AIModels so we only display the ones where the type is 'completion'.
			this.aiModelOptions = this.aiModelOptions.filter((model) => model.type === 'completion');

			// Load realtime speech models
			this.realtimeSpeechModelOptions = aiModelsResult
				.map((result) => result.resource)
				.filter((model) => model.type === 'realtime-speech')
				.map((model) => ({ name: model.name || '', object_id: model.object_id || '' }));

			this.loadingStatusText = 'Retrieving workflows...';
			this.workflowOptions = [
				// {
				// 	type: null,
				// 	workflow_name: 'None',
				// },
				// {
				// 	type: 'langgraph-react-agent-workflow',
				// 	workflow_name: 'LangGraph ReAct Agent Workflow',
				// },
				// {
				// 	type: 'azure-openai-assistants-workflow',
				// 	workflow_name: 'Azure OpenAI Assistants Workflow',
				// },
				...(await api.getAgentWorkflows()).map((workflow) => workflow.resource),
			];

			// Note: Prompts are now loaded lazily when userPromptRewriteEnabled is true
			// This improves performance by not loading all prompts upfront
		} catch (error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
				life: 5000,
				closable: true,

			});
		}

		if (this.editAgent) {
			this.loadingStatusText = `Retrieving agent "${this.editAgent}"...`;
			const agentGetResult = await api.getAgent(this.editAgent);
			this.editable = agentGetResult.actions.includes('FoundationaLLM.Agent/agents/write');

			const agent = agentGetResult.resource;
			this.virtualSecurityGroupId = agent.virtual_security_group_id;

			if (agent.prompt_object_id) {
				this.loadingStatusText = `Retrieving prompt...`;
				const parentResource = `FoundationaLLM.Agent|agents|${agent.name}`;
				const prompt = await api.getPrompt(agent.prompt_object_id, parentResource);
				if (prompt && prompt.resource) {
					this.agentPrompt = prompt;
					this.systemPrompt = prompt.resource.prefix;
				}
			} else if (agent.workflow?.resource_object_ids) {
				this.loadingStatusText = `Retrieving prompt...`;

				const existingMainPrompt = Object.values(agent.workflow.resource_object_ids).find(
					(resource) => resource.properties?.object_role === 'main_prompt',
				);

				if (existingMainPrompt) {
					const parentResource = `FoundationaLLM.Agent|agents|${agent.name}`;
					const prompt = await api.getPrompt(existingMainPrompt.object_id, parentResource);
					if (prompt && prompt.resource) {
						this.agentPrompt = prompt;
						this.systemPrompt = prompt.resource.prefix;
					}
				}
			}

			if (agent.workflow) {
				this.workflowAssistantId = agent.workflow.assistant_id ?? '';
				this.workflowVectorStoreId = agent.workflow.vector_store_id ?? '';
				this.workflowName = agent.workflow.name ?? '';
				this.workflowPackageName = agent.workflow.package_name ?? '';
				this.workflowClassName = agent.workflow.class_name ?? '';
				this.workflowHost = agent.workflow.workflow_host ?? '';

				const existingMainModel = Object.values(agent.workflow.resource_object_ids).find(
					(resource) => resource.properties?.object_role === 'main_model',
				);
				this.workflowMainAIModel = existingMainModel ?? null;
				this.workflowExtraResources = Object.fromEntries(
					Object.entries(agent.workflow.resource_object_ids).filter(([key, resource]) => {
						const objectRole = resource.properties?.object_role;
						const workflowPrefix = `/instances/${this.$appConfigStore.instanceId}/providers/FoundationaLLM.Agent/workflows`;
						return (
							objectRole !== 'main_model' &&
							objectRole !== 'main_prompt' &&
							!key.startsWith(workflowPrefix)
						);
					}),
				);
			}

			this.loadingStatusText = `Mapping agent values to form...`;
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
			this.agentWelcomeMessage = agent.properties?.welcome_message || this.agentWelcomeMessage;
			this.agentType = agent.type || this.agentType;
			this.object_id = agent.object_id || this.object_id;
			this.inline_context = agent.inline_context || this.inline_context;
			this.cost_center = agent.cost_center || this.cost_center;
			this.hasOpenAIAssistantCapability =
				agent.workflow?.type === 'azure-openai-assistants-workflow';
			this.expirationDate = agent.expiration_date
				? new Date(agent.expiration_date)
				: this.expirationDate;

			// this.orchestration_settings.orchestrator =
			// 	agent.orchestration_settings?.orchestrator || this.orchestration_settings.orchestrator;
			// this.selectedAIModel =
			// 	this.aiModelOptions.find((aiModel) => aiModel.object_id === agent.ai_model_object_id) ||
			// 	null;

			this.conversationHistory =
				agent.conversation_history_settings?.enabled || this.conversationHistory;
			this.conversationMaxMessages =
				agent.conversation_history_settings?.max_history || this.conversationMaxMessages;

			this.gatekeeperUseSystemDefault = Boolean(agent.gatekeeper_settings?.use_system_setting);

			if (agent.gatekeeper_settings && agent.gatekeeper_settings.options) {
				this.selectedGatekeeperContentSafety =
					this.gatekeeperContentSafetyOptions.filter((localOption) =>
						agent.gatekeeper_settings?.options?.includes(localOption.code),
					) || this.selectedGatekeeperContentSafety;

				this.selectedGatekeeperDataProtection =
					this.gatekeeperDataProtectionOptions.filter((localOption) =>
						agent.gatekeeper_settings?.options?.includes(localOption.code),
					) || this.selectedGatekeeperDataProtection;
			}

			// if (agent.capabilities) {
			// 	this.selectedAgentCapabilities =
			// 		this.agentCapabilitiesOptions.filter((localOption) =>
			// 			agent.capabilities?.includes(localOption.code),
			// 		) || this.selectedAgentCapabilities;
			// }

			this.agentTools = agent.tools;

			this.selectedWorkflow = clone(
				this.workflowOptions.find((workflow) => workflow.type === agent.workflow?.type),
			);
			this.hasAgentPrivateStorage = agent.workflow?.type === 'azure-openai-assistants-workflow' || agent.workflow?.class_name === 'FoundationaLLMFunctionCallingWorkflow';
			this.isOpenAIAssistantWorkflow = agent.workflow?.type === 'azure-openai-assistants-workflow';
			this.isFoundationaLLMFunctionCallingWorkflow = agent.workflow?.class_name === 'FoundationaLLMFunctionCallingWorkflow';
			this.showMessageTokens = agent.show_message_tokens ?? false;
			this.showMessageRating = agent.show_message_rating ?? false;
			this.showViewPrompt = agent.show_view_prompt ?? false;
			this.showFileUpload = agent.show_file_upload ?? false;
			this.showContentArtifacts = agent.show_content_artifacts ?? true;

			const userPromptRewriteSettings = agent.text_rewrite_settings?.user_prompt_rewrite_settings;
			this.userPromptRewriteEnabled =
				agent.text_rewrite_settings?.user_prompt_rewrite_enabled ?? false;
			this.userPromptRewriteAIModel =
				userPromptRewriteSettings?.user_prompt_rewrite_ai_model_object_id ?? null;
			this.userPromptRewritePrompt =
				userPromptRewriteSettings?.user_prompt_rewrite_prompt_object_id ?? null;
			this.userPromptRewriteWindowSize = userPromptRewriteSettings?.user_prompts_window_size ?? 3;

			// Load prompts lazily if userPromptRewriteEnabled is true in edit mode
			if (this.userPromptRewriteEnabled) {
				this.loadPromptsIfNeeded('editMode');
			}

			const semanticCacheSettings = agent.cache_settings?.semantic_cache_settings;
			this.semanticCacheEnabled = agent.cache_settings?.semantic_cache_enabled ?? false;

			// Load realtime speech settings
			const realtimeSpeechSettings = agent.realtime_speech_settings;
			this.realtimeSpeechEnabled = realtimeSpeechSettings?.enabled ?? false;
			this.realtimeSpeechAIModel = realtimeSpeechSettings?.realtime_speech_ai_model_object_id ?? null;
			this.realtimeSpeechStopWords = realtimeSpeechSettings?.stop_words ?? ['stop', 'end conversation', 'goodbye'];
			this.realtimeSpeechStopWordsInput = this.realtimeSpeechStopWords.join(', ');
			this.realtimeSpeechShowTranscriptions = realtimeSpeechSettings?.show_transcriptions ?? true;
			this.realtimeSpeechIncludeHistory = realtimeSpeechSettings?.include_conversation_history ?? true;
			this.semanticCacheAIModel = semanticCacheSettings?.embedding_ai_model_object_id ?? null;
			this.semanticCacheEmbeddingDimensions = semanticCacheSettings?.embedding_dimensions ?? 2048;
			this.semanticCacheMinimumSimilarityThreshold =
				semanticCacheSettings?.minimum_similarity_threshold ?? 0.97;

			this.inheritable_authorizable_actions = agent.inheritable_authorizable_actions || [];
			// this.showFileUpload = agent.show_file_upload ?? false;
		},

		updateAgentWelcomeMessage(newContent: string) {
			this.agentWelcomeMessage = newContent;
		},

		updateRealtimeSpeechStopWords() {
			this.realtimeSpeechStopWords = this.realtimeSpeechStopWordsInput
				.split(',')
				.map((word) => word.trim())
				.filter((word) => word.length > 0);
		},

		async checkName() {
			try {
				const response = await api.checkAgentName(this.agentName, this.agentType);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.error_message;
					// this.$toast.add({
					// 	severity: 'warn',
					// 	detail: `Agent name "${this.agentName}" is already taken for the selected ${response.type} agent type. Please choose another name.`,
					// life: 5000,
					// });
				}
			} catch (error) {
				console.error('Error checking agent name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the agent name. Please try again.';
			}
		},

		resetForm() {
			const defaultFormValues = getDefaultFormValues();
			for (const key in defaultFormValues) {
				this[key] = defaultFormValues[key];
			}
		},

		/**
		 * Loads prompts lazily based on context and requirements
		 * This improves performance by not loading all prompts upfront
		 * @param context - The context for loading prompts ('userPromptRewrite', 'workflowResource', 'editMode')
		 * @param filterCriteria - Optional filter criteria for prompts
		 */
		async loadPromptsIfNeeded(context: 'userPromptRewrite' | 'workflowResource' | 'editMode' = 'userPromptRewrite', filterCriteria?: string): Promise<void> {
			const cacheKey = `${context}_${filterCriteria || 'default'}`;

			// Check if prompts are already cached for this context
			if (this.promptCache.has(cacheKey)) {
				this.promptOptions = this.promptCache.get(cacheKey)!;
				return;
			}

			// Check if already loading for this context
			if (this.promptLoadingStates.get(cacheKey)) {
				return;
			}

			// Mark as loading
			this.promptLoadingStates.set(cacheKey, true);
			this.loadingStatusText = 'Retrieving prompts...';

			try {
				const promptOptionsResult = await api.getPrompts();
				if (!promptOptionsResult) {
					this.promptOptions = [];
					return;
				}
				let filteredPrompts = promptOptionsResult.map((result) => result.resource);

				// Apply context-specific filtering
				if (context === 'editMode' && this.editAgent) {
					// In edit mode, only load prompts related to this agent
					// This includes prompts with the agent name in their name or description
					const agentName = this.agentName.toLowerCase();
					filteredPrompts = filteredPrompts.filter(prompt =>
						prompt.name.toLowerCase().includes(agentName) ||
						prompt.description?.toLowerCase().includes(agentName) ||
						prompt.category === 'Workflow' // Include workflow-related prompts
					);
				} else if (context === 'workflowResource') {
					// For workflow resources, filter to relevant prompt categories
					filteredPrompts = filteredPrompts.filter(prompt =>
						prompt.category === 'Workflow' ||
						prompt.category === 'System' ||
						prompt.name.toLowerCase().includes('workflow')
					);
				}

				// Apply additional filter criteria if provided
				if (filterCriteria) {
					const criteria = filterCriteria.toLowerCase();
					filteredPrompts = filteredPrompts.filter(prompt =>
						prompt.name.toLowerCase().includes(criteria) ||
						prompt.description?.toLowerCase().includes(criteria)
					);
				}

				// Cache the filtered results
				this.promptCache.set(cacheKey, filteredPrompts);
				this.promptOptions = filteredPrompts;
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: undefined,
					closable: true,
				});
			} finally {
				// Mark as not loading
				this.promptLoadingStates.set(cacheKey, false);
			}
		},

		/**
		 * Clear prompt cache when needed (e.g., when switching contexts)
		 */
		clearPromptCache(): void {
			this.promptCache.clear();
			this.promptLoadingStates.clear();
			this.promptOptions = [];
		},

		/**
		 * Get cached prompts for a specific context
		 * @param context - The context to get prompts for
		 * @param filterCriteria - Optional filter criteria
		 */
		getCachedPrompts(context: string, filterCriteria?: string): Prompt[] {
			const cacheKey = `${context}_${filterCriteria || 'default'}`;
			return this.promptCache.get(cacheKey) || [];
		},

		async handleCancel() {
			const confirmationStore = useConfirmationStore();
			const confirmed = await confirmationStore.confirmAsync({
				title: 'Cancel Agent Creation',
				message: 'Are you sure you want to cancel?',
				confirmText: 'Yes',
				cancelText: 'Cancel',
				confirmButtonSeverity: 'danger',
			});

			if (confirmed) {
				this.$router.push('/agents/public');
			}
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.agentName = sanitizedValue;

			// Check if the name is available if we are creating a new agent.
			if (!this.editAgent) {
				this.debouncedCheckName();
			}
		},

		handleWorkflowSelection(event) {
			this.selectedWorkflow = clone(
				this.workflowOptions.find((workflow) => workflow.type === event.value),
			);
		},

		getResourceNameFromId(resourceId: string): string {
			const parts = resourceId.split('/').filter(Boolean);
			return parts.slice(-1)[0];
		},

		handleAddWorkflowResource(resourceToAdd) {
			this.workflowExtraResources[resourceToAdd.object_id] = resourceToAdd;
			this.showCreateWorkflowResourceObjectDialog = false;
		},

		handleDeleteWorkflowResource() {
			delete this.workflowExtraResources[this.workflowResourceToDelete.object_id];
			this.workflowResourceToDelete = null;
		},

		handleAgentTypeSelect(type: Agent['type']) {
			this.agentType = type;
		},

		// handleAIModelSelected(aiModel: AIModel) {
		// 	this.selectedAIModel = aiModel;
		// 	this.editAIModel = false;
		// },

		handleCopySecurityGroupId() {
			if (this.virtualSecurityGroupId) {
				navigator.clipboard.writeText(this.virtualSecurityGroupId);
				this.$toast.add({
					severity: 'success',
					detail: 'Virtual Security Group ID copied to clipboard!',
					life: 5000,
					closable: true,
				});
			}
		},

		handleAddNewTool(newTool) {
			const index = this.agentTools.findIndex((tool) => tool.name === newTool.name);
			if (index > -1) {
				this.agentTools[index] = newTool;
			} else {
				this.agentTools.push(newTool);
			}

			this.showNewToolDialog = false;
		},

		handleUpdateTool(updatedTool) {
			const index = this.agentTools.findIndex((tool) => tool.name === updatedTool.name);
			this.agentTools[index] = updatedTool;
			this.toolToEdit = null;
		},

		handleRemoveTool() {
			const index = this.agentTools.findIndex((tool) => tool.name === this.toolToRemove.name);
			this.agentTools.splice(index, 1);
			this.toolToRemove = null;
		},
				// Scroll to the first field with an error
		scrollToFirstError() {
			this.$nextTick(() => {
				const errorFields = Object.keys(this.formErrors).filter((key) => this.formErrors[key]);
				if (errorFields.length > 0) {
					const field = errorFields[0];
					// Try to find by ref (Vue way)
					const refEl = this.$refs[field];
					if (refEl && typeof refEl.focus === 'function') {
						refEl.focus();
						if (typeof refEl.scrollIntoView === 'function') {
							refEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
						}
						return;
					}
					// Try to find by id
					const idEl = document.getElementById(field);
					if (idEl) {
						if (typeof idEl.focus === 'function') idEl.focus();
						idEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
						return;
					}
					// Fallback: try to find the error message div
					let el = Array.from(document.querySelectorAll('.error-message')).find((div) =>
						div.textContent?.includes(this.formErrors[field])
					);
					if (el && typeof el.scrollIntoView === 'function') {
						el.scrollIntoView({ behavior: 'smooth', block: 'center' });
					}
				}
			});
		},
	async handleCreateAgent() {
				this.showValidationErrors = true;
				// errors object, keys are field names, values are error messages
				const errors: Record<string, string> = {
					agentName: '',
					nameValidation: '',
					selectedWorkflow: '',
					workflowName: '',
					workflowPackageName: '',
					workflowHost: '',
					workflowMainAIModel: '',
					systemPrompt: '',
					userPromptRewriteAIModel: '',
					userPromptRewritePrompt: '',
					userPromptRewriteWindowSize: '',
					semanticCacheAIModel: '',
					semanticCacheEmbeddingDimensions: '',
					semanticCacheMinimumSimilarityThreshold: '',
				};

				if (!this.agentName) {
					errors.agentName = 'Please give the agent a name.';
				}
				if (this.nameValidationStatus === 'invalid') {
					errors.nameValidation = this.validationMessage;
				}

				if (!this.selectedWorkflow) {
					errors.selectedWorkflow = 'Please select a workflow.';
				}

				if (!this.workflowName) {
					errors.workflowName = 'Please provide a workflow name.';
				}

				if (!this.workflowPackageName) {
					errors.workflowPackageName = 'Please provide a workflow package name.';
				}

				if (!this.workflowHost) {
					errors.workflowHost = 'Please select a workflow host.';
				}

				if (!this.workflowMainAIModel) {
					errors.workflowMainAIModel = 'Please select an AI model for the workflow.';
				}

				if (this.systemPrompt === '') {
					errors.systemPrompt = 'Please provide a system prompt.';
				}

				if (this.userPromptRewriteEnabled) {
					if (!this.userPromptRewriteAIModel) {
						errors.userPromptRewriteAIModel = 'Please select a model for the user prompt rewrite.';
					}

					if (!this.userPromptRewritePrompt) {
						errors.userPromptRewritePrompt = 'Please select a prompt for the user prompt rewrite.';
					}

					if (this.userPromptRewriteWindowSize === null) {
						errors.userPromptRewriteWindowSize = 'Please input a window size for the user prompt rewrite';
					}
				}

				if (this.semanticCacheEnabled) {
					if (!this.semanticCacheAIModel) {
						errors.semanticCacheAIModel = 'Please select a model for the semantic cache.';
					}

					if (!this.semanticCacheEmbeddingDimensions === null) {
						errors.semanticCacheEmbeddingDimensions = 'Please input the embedding dimensions for the semantic cache.';
					}

					if (this.semanticCacheMinimumSimilarityThreshold === null) {
						errors.semanticCacheMinimumSimilarityThreshold = 'Please input the minimum similarity threshold for the semantic cache.';
					}
				}

				// Remove empty error fields
				const filteredErrors = Object.fromEntries(Object.entries(errors).filter(([_, v]) => v));
				this.formErrors = { ...errors }; // Save all errors for per-input display

				if (Object.keys(filteredErrors).length > 0) {
					// Optionally, show a toast with the first error or all errors joined
					this.scrollToFirstError();
					return;
				}

			this.loading = true;
			this.loadingStatusText = 'Saving agent...';

			const promptRequest = {
				type: 'multipart',
				name: this.agentPrompt?.resource.name || this.agentName,
				cost_center: this.cost_center,
				description: `System prompt for the ${this.agentName} agent`,
				prefix: this.systemPrompt,
				suffix: '',
				category: 'Workflow',
			};

			let successMessage = null;
			try {
				// Handle Prompt creation/update.
				let promptObjectId = '';
				if (promptRequest.prefix !== '') {
					const promptResponse = await api.createOrUpdatePrompt(promptRequest.name, promptRequest);
					promptObjectId = promptResponse.object_id;
				}

				// if (this.selectedWorkflow) {
				// 	const workflowPromptRequest = {
				// 		type: 'multipart',
				// 		name: this.selectedWorkflow.prompt_object_ids.main_prompt,
				// 		cost_center: null,
				// 		description: `Workflow prompt for the ${this.selectedWorkflowModelId} model`,
				// 		prefix: this.workflowMainPrompt,
				// 		suffix: '',
				// 	};

				// 	const workflowPromptResponse = await api.createOrUpdatePrompt(this.selectedWorkflow.prompt_object_ids.main_prompt, promptRequest);
				// 	workflowPromptResponse = promptResponse.object_id;
				// }

				let workflow = null;
				if (this.selectedWorkflow) {
					workflow = {
						...this.selectedWorkflow,
						workflow_host: this.workflowHost,
						assistant_id: this.workflowAssistantId,
						vector_store_id: this.workflowVectorStoreId,
						name: this.workflowName,
						package_name: this.workflowPackageName,
						class_name: this.workflowClassName,

						resource_object_ids: {
							// ...this.selectedWorkflow.resource_object_ids,

							[this.workflowMainAIModel.object_id]: {
								object_id: this.workflowMainAIModel.object_id,
								properties: {
									object_role: 'main_model',
									model_parameters: this.workflowMainAIModelParameters,
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

							...(this.selectedWorkflow?.object_id
								? {
										[this.selectedWorkflow.object_id]: {
											object_id: this.selectedWorkflow.object_id,
											properties: {},
										},
									}
								: {}),

							...this.workflowExtraResources,
						},
					};
				}

				const agentRequest: CreateAgentRequest = {
					type: this.agentType,
					name: this.agentName,
					description: this.agentDescription,
					display_name: this.agentDisplayName,
					properties: {
						welcome_message: this.agentWelcomeMessage,
					},
					object_id: this.object_id,
					inline_context: this.inline_context,
					cost_center: this.cost_center,
					expiration_date: this.expirationDate?.toISOString(),

					show_message_tokens: this.showMessageTokens,
					show_message_rating: this.showMessageRating,
					show_view_prompt: this.showViewPrompt,
					show_file_upload: this.showFileUpload,
					show_content_artifacts: this.showContentArtifacts,

					vectorization: null,

					conversation_history_settings: {
						enabled: this.conversationHistory,
						max_history: Number(this.conversationMaxMessages),
					},

					gatekeeper_settings: {
						use_system_setting: this.gatekeeperUseSystemDefault,
						options: [
							...(this.selectedGatekeeperContentSafety || []).map((option: any) => option.code),
							...(this.selectedGatekeeperDataProtection || []).map((option: any) => option.code),
						].filter((option) => option !== null),
					},

					// capabilities: (this.selectedAgentCapabilities || []).map((option: any) => option.code),

					sessions_enabled: true,

					prompt_object_id: promptObjectId,
					// orchestration_settings: this.orchestration_settings,
					// ai_model_object_id: this.selectedAIModel.object_id,

					tools: this.agentTools,

					workflow,

					virtual_security_group_id: this.virtualSecurityGroupId,

					text_rewrite_settings: {
						user_prompt_rewrite_enabled: this.userPromptRewriteEnabled,
						user_prompt_rewrite_settings: {
							user_prompt_rewrite_ai_model_object_id: this.userPromptRewriteAIModel,
							user_prompt_rewrite_prompt_object_id: this.userPromptRewritePrompt,
							user_prompts_window_size: this.userPromptRewriteWindowSize,
						},
					},

					cache_settings: {
						semantic_cache_enabled: this.semanticCacheEnabled,
						semantic_cache_settings: {
							embedding_ai_model_object_id: this.semanticCacheAIModel,
							embedding_dimensions: this.semanticCacheEmbeddingDimensions,
							minimum_similarity_threshold: this.semanticCacheMinimumSimilarityThreshold,
						},
					},

					realtime_speech_settings: {
						enabled: this.realtimeSpeechEnabled,
						realtime_speech_ai_model_object_id: this.realtimeSpeechAIModel,
						stop_words: this.realtimeSpeechStopWords,
						max_session_duration_seconds: 0,
						show_transcriptions: this.realtimeSpeechShowTranscriptions,
						include_conversation_history: this.realtimeSpeechIncludeHistory,
					},

					inheritable_authorizable_actions: this.inheritable_authorizable_actions
				};

				if (this.editAgent) {
					await api.upsertAgent(this.editAgent, agentRequest);
					successMessage = `Agent "${this.agentName}" was successfully updated!`;
				} else {
					await api.upsertAgent(agentRequest.name, agentRequest);
					successMessage = `Agent "${this.agentName}" was successfully created!`;
					this.resetForm();
				}
			} catch (error) {
				this.loading = false;
				return this.$toast.add({
					severity: 'error',
					detail: error && error.response && error.response._data ? error.response._data : error,
					life: 5000,
					closable: true,
				});
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
				life: 5000,
				closable: true,
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

// .p-button-icon {
// 	color: var(--primary-button-text) !important;
// }

.valid {
	color: green;
}

.invalid {
	color: red;
}

.virtual-security-group-id {
	margin: 0 1rem 0 0;
	width: auto;
}
</style>
