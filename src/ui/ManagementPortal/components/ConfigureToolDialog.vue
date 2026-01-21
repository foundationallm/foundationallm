<template>
	<Dialog
		:visible="visible"
		modal
		header="Configure Tool"
		:style="{ minWidth: '80%' }"
		:closable="false"
	>
		<!-- 	<div id="aria-tool-type" class="mb-2 font-bold">Tool type:</div>
		<Dropdown
			v-model="toolType"
			:options="toolTypeOptions"
			option-label="label"
			option-value="value"
			placeholder="--Select--"
			aria-labelledby="aria-tool-type"
		/> -->

		<!-- 		<div id="aria-tool-name" class="mb-2 font-bold">Tool:</div>
		<Dropdown
			v-model="toolObject.name"
			:options="toolOptions"
			option-label="label"
			option-value="label"
			placeholder="--Select--"
			aria-labelledby="aria-tool-type"
			@change="handleToolSelection"
		/> -->

		<div id="aria-tool-name" class="mb-2 font-bold">Tool name:</div>
		<InputText
			v-model="toolObject.name"
			type="text"
			class="w-full"
			placeholder="Enter tool name"
			aria-labelledby="aria-tool-name"
		/>

		<div id="aria-tool-description" class="mt-6 mb-2 font-bold">Tool description:</div>
		<Textarea
			v-model="toolObject.description"
			auto-resize
			rows="5"
			type="text"
			class="w-full"
			placeholder="Enter tool description"
			aria-labelledby="aria-tool-description"
		/>

		<div id="aria-tool-package-name" class="mt-6 mb-2 font-bold">Tool package name:</div>
		<InputText
			v-model="toolObject.package_name"
			type="text"
			class="w-full"
			placeholder="Enter tool package name"
			aria-labelledby="aria-tool-package-name"
		/>

		<div id="aria-tool-class-name" class="mt-6 mb-2 font-bold">Tool class name:</div>
		<InputText
			v-model="toolObject.class_name"
			type="text"
			class="w-full"
			placeholder="Enter tool class name"
			aria-labelledby="aria-tool-class-name"
		/>

		<div class="mt-6 mb-2 font-bold">Tool resources:</div>
		<ResourceTable :resources="toolObject.resource_object_ids" @delete="handleDeleteToolResource" />

		<CreateResourceObjectDialog
			v-if="showCreateResourceObjectDialog"
			:visible="showCreateResourceObjectDialog"
			resource-context="tool"
			@update:visible="showCreateResourceObjectDialog = false"
			@update:model-value="handleAddToolResource"
		/>

		<div class="flex justify-end mt-4">
			<Button
				severity="primary"
				label="Add Tool Resource"
				@click="showCreateResourceObjectDialog = true"
			/>
		</div>

		<div class="mt-6 mb-2 font-bold">Tool properties:</div>
		<PropertyBuilder v-model="toolObject.properties" />

		<!-- Procedural Memory Settings (only for Code Interpreter tool) -->
		<div v-if="isCodeInterpreterTool" class="mt-8">
			<Divider />
			<div class="mt-6 mb-4 font-bold flex items-center gap-2">
				<i class="pi pi-cog" style="font-size: 1.2rem"></i>
				<span>Procedural Memory Settings</span>
			</div>

			<div class="procedural-memory-settings p-4 border-round" style="background-color: var(--surface-ground);">
				<!-- Enable Procedural Memory Toggle -->
				<div class="mb-4">
					<div class="flex align-items-center gap-2 mb-3">
						<InputSwitch
							v-model="proceduralMemorySettings.enabled"
							:disabled="false"
							aria-labelledby="aria-enable-procedural-memory"
						/>
						<label id="aria-enable-procedural-memory" class="font-semibold">
							Enable Procedural Memory
						</label>
					</div>
					<div v-if="proceduralMemorySettings.enabled" class="ml-6 text-sm text-color-secondary mb-4">
						When enabled, the Code Interpreter tool will automatically:
						<ul class="mt-2 ml-4" style="list-style-type: disc;">
							<li>Search for relevant skills before generating new code</li>
							<li>Use existing skills when found and suitable</li>
							<li>Optionally register successful code as reusable skills</li>
						</ul>
					</div>
				</div>

				<!-- Settings (only shown when enabled) -->
				<div v-if="proceduralMemorySettings.enabled" class="flex flex-col gap-4">
					<!-- Auto-Register Skills -->
					<div class="p-3 border-round" style="background-color: var(--surface-card);">
						<div class="flex align-items-center gap-2 mb-2">
							<InputSwitch
								v-model="proceduralMemorySettings.auto_register_skills"
								:disabled="!proceduralMemorySettings.enabled"
							/>
							<label class="font-medium">Auto-Register Skills</label>
						</div>
						<div class="text-sm text-color-secondary ml-6">
							Automatically save successful code executions as skills
						</div>
					</div>

					<!-- Require Skill Approval -->
					<div class="p-3 border-round" style="background-color: var(--surface-card);">
						<div class="flex align-items-center gap-2 mb-2">
							<InputSwitch
								v-model="proceduralMemorySettings.require_skill_approval"
								:disabled="!proceduralMemorySettings.enabled"
							/>
							<label class="font-medium">Require Skill Approval</label>
						</div>
						<div class="text-sm text-color-secondary ml-6">
							New skills require admin approval before becoming active
						</div>
					</div>

					<!-- Max Skills Per User -->
					<div class="flex flex-column gap-2">
						<label class="font-medium">Max Skills Per User</label>
						<InputNumber
							v-model="proceduralMemorySettings.max_skills_per_user"
							:disabled="!proceduralMemorySettings.enabled"
							:min="0"
							:max="10000"
							class="w-full"
							show-buttons
						/>
						<div class="text-sm text-color-secondary">
							(0 = unlimited)
						</div>
					</div>

					<!-- Skill Search Threshold -->
					<div class="flex flex-column gap-2">
						<label class="font-medium">
							Skill Search Threshold: {{ proceduralMemorySettings.skill_search_threshold.toFixed(2) }}
						</label>
						<Slider
							v-model="proceduralMemorySettings.skill_search_threshold"
							:disabled="!proceduralMemorySettings.enabled"
							:min="0"
							:max="1"
							:step="0.01"
							class="w-full"
						/>
						<div class="text-sm text-color-secondary">
							Minimum similarity score (0.0 to 1.0) for skill matching
						</div>
					</div>

					<!-- Prefer Skills Over New Code -->
					<div class="flex align-items-center gap-2">
						<InputSwitch
							v-model="proceduralMemorySettings.prefer_skills"
							:disabled="!proceduralMemorySettings.enabled"
						/>
						<label class="font-medium">Prefer Skills Over New Code</label>
					</div>
					<div class="text-sm text-color-secondary ml-6">
						When enabled, tool will prefer using existing skills over generating new code when similarity threshold is met
					</div>
				</div>
			</div>
		</div>

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
		</template>
	</Dialog>
</template>

<script lang="ts">
// import api from '@/js/api';

export default {
	props: {
		modelValue: {
			type: [Object, String],
			required: false,
			default: () => ({
				name: '' as string,
				description: '' as string,
				package_name: 'FoundationaLLM' as string,
				class_name: '' as string,
				resource_object_ids: {},
			}),
		},

		existingTools: {
			type: Array,
			required: false,
			default: () => [],
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	emits: ['update:modelValue', 'update:visible'],

	data() {
		return {
			toolObject: {
				type: 'tool' as string,
				name: '' as string,
				description: '' as string,
				package_name: 'FoundationaLLM' as string,
				class_name: '' as string,
				resource_object_ids: {} as Record<string, any>,
				properties: {} as Record<string, any>,
			},
			showCreateResourceObjectDialog: false,

			// Procedural memory settings
			proceduralMemorySettings: {
				enabled: false,
				auto_register_skills: true,
				require_skill_approval: false,
				max_skills_per_user: 0,
				skill_search_threshold: 0.8,
				prefer_skills: true,
			},

			// toolType: null,
			// toolTypeOptions: [
			// 	{
			// 		label: 'Internal',
			// 		value: 'internal',
			// 	},
			// 	{
			// 		label: 'Custom',
			// 		value: 'custom',
			// 	},
			// ] as Object[],

			toolOptions: [] as Object[],
		};
	},

	computed: {
		/**
		 * Detects if the current tool is the Code Interpreter tool.
		 * Code Interpreter tool has:
		 * - package_name: 'foundationallm_agent_plugins'
		 * - class_name: 'FoundationaLLMCodeInterpreterTool'
		 */
		isCodeInterpreterTool(): boolean {
			return (
				this.toolObject.package_name === 'foundationallm_agent_plugins' &&
				this.toolObject.class_name === 'FoundationaLLMCodeInterpreterTool'
			);
		},
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.toolObject)) return;
				const clonedValue = JSON.parse(JSON.stringify(this.modelValue));
				
				// Ensure properties object exists
				if (!clonedValue.properties) {
					clonedValue.properties = {};
				}
				
				this.toolObject = clonedValue;
				
				// Initialize procedural memory settings from tool properties
				this.initializeProceduralMemorySettings();
			},
		},
	},

	async created() {
		// const tools = await api.getAgentTools();
		// this.toolOptions = tools.map((tool) => ({
		// 	label: tool.resource.name,
		// 	value: tool.resource,
		// }));
	},

	methods: {
		handleToolSelection(event) {
			const tool = this.toolOptions.find((tool) => tool.label === event.value)?.value;
			this.toolObject.type = tool.type;
			this.toolObject.name = tool.name;
			this.toolObject.description = tool.description;
		},

		handleAddToolResource(resourceToAdd) {
			this.toolObject.resource_object_ids[resourceToAdd.object_id] = resourceToAdd;
			this.showCreateResourceObjectDialog = false;
		},

		// handleEditToolResource(resourceToEdit) {
		// 	this.toolObject.resource_object_ids[resourceToEdit.object_id] = resourceToEdit;
		// },

		handleDeleteToolResource(resourceToDelete) {
			delete this.toolObject.resource_object_ids[resourceToDelete.object_id];
		},

		/**
		 * Initialize procedural memory settings from tool properties.
		 * Handles both string (JSON) and object formats.
		 */
		initializeProceduralMemorySettings() {
			if (!this.isCodeInterpreterTool) {
				return;
			}

			// Ensure properties object exists
			if (!this.toolObject.properties) {
				this.toolObject.properties = {};
			}

			// Get procedural_memory_settings from tool properties
			const pmSettings = this.toolObject.properties['procedural_memory_settings'];
			
			if (pmSettings) {
				try {
					// Parse JSON if string, otherwise use directly
					let settings = pmSettings;
					if (typeof pmSettings === 'string') {
						settings = JSON.parse(pmSettings);
					}

					// Merge with defaults
					if (settings && typeof settings === 'object') {
						this.proceduralMemorySettings = {
							enabled: settings.enabled ?? false,
							auto_register_skills: settings.auto_register_skills ?? true,
							require_skill_approval: settings.require_skill_approval ?? false,
							max_skills_per_user: settings.max_skills_per_user ?? 0,
							skill_search_threshold: settings.skill_search_threshold ?? 0.8,
							prefer_skills: settings.prefer_skills ?? true,
						};
					}
				} catch (error) {
					console.error('Error parsing procedural memory settings:', error);
					// Use defaults on parse error
				}
			}
		},

		/**
		 * Validate procedural memory settings.
		 * Returns array of error messages, empty if valid.
		 */
		validateProceduralMemorySettings(): string[] {
			const errors: string[] = [];

			if (!this.isCodeInterpreterTool || !this.proceduralMemorySettings.enabled) {
				return errors;
			}

			// Validate skill_search_threshold
			const threshold = this.proceduralMemorySettings.skill_search_threshold;
			if (typeof threshold !== 'number' || threshold < 0 || threshold > 1) {
				errors.push('Skill search threshold must be between 0.0 and 1.0');
			}

			// Validate max_skills_per_user
			const maxSkills = this.proceduralMemorySettings.max_skills_per_user;
			if (typeof maxSkills !== 'number' || maxSkills < 0) {
				errors.push('Max skills per user must be 0 or greater (0 = unlimited)');
			}

			return errors;
		},

		handleSave() {
			const errors = [];

			if (
				this.modelValue.name !== this.toolObject.name &&
				this.existingTools.findIndex((tool) => tool.name === this.toolObject.name) !== -1
			) {
				errors.push('This tool name aleady exists on this agent.');
			}

			if (!this.toolObject.name) {
				errors.push('Please provide a tool name.');
			}

			if (!this.toolObject.description) {
				errors.push('Please provide a tool description.');
			}

			if (!this.toolObject.package_name) {
				errors.push('Please provide a tool package name.');
			}

			// Validate and persist procedural memory settings for Code Interpreter tool
			if (this.isCodeInterpreterTool) {
				// Only validate if enabled (disabled state doesn't need validation)
				if (this.proceduralMemorySettings.enabled) {
					const pmErrors = this.validateProceduralMemorySettings();
					errors.push(...pmErrors);
				}

				// Persist procedural memory settings to tool properties (even when disabled)
				// This preserves the configuration for future use
				if (errors.length === 0) {
					this.toolObject.properties = this.toolObject.properties || {};
					this.toolObject.properties['procedural_memory_settings'] = JSON.stringify(
						this.proceduralMemorySettings
					);
				}
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.$emit('update:modelValue', this.toolObject);
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
