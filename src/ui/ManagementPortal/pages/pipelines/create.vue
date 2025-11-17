<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editId ? 'Edit Pipeline' : 'Create Pipeline' }}</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your pipeline settings below.'
							: 'Complete the settings below to configure the pipeline.'
					}}
				</div>
			</div>
		</div>

		<!-- Steps -->
		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite" aria-label="Loading pipeline form">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header col-span-2">What is the name of the pipeline?</div>
			<div class="col-span-2">
				<div id="aria-pipeline-name" class="mb-2">Pipeline name:</div>
				<div id="aria-pipeline-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.name"
						:disabled="isEditing"
						type="text"
						class="w-full"
						placeholder="Enter pipeline name"
						aria-labelledby="aria-pipeline-name aria-pipeline-name-desc"
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
						:title="validationMessage || ''"
						class="icon invalid"
						:aria-label="validationMessage || ''"
					>
						❌
					</span>
				</div>

				<div id="aria-pipeline-display-name" class="mb-2 mt-2">Pipeline display name:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.display_name"
						type="text"
						class="w-full"
						placeholder="Enter a display name for this pipeline"
					/>
				</div>

				<div id="aria-pipeline-desc" class="mb-2 mt-2">Pipeline description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.description"
						type="text"
						class="w-full"
						placeholder="Enter a description for this pipeline"
						aria-labelledby="aria-pipeline-desc"
					/>
				</div>
			</div>

			<!-- Data Source -->
			<div class="step-header col-span-2">Select a data source</div>
			<div class="col-span-2">
				<Dropdown
					v-model="selectedDataSource"
					:options="dataSourceOptions"
					option-label="name"
					class="w-full"
					placeholder="Select a data source"
					@change="handleDataSourceChange"
				/>
				<template v-if="selectedDataSource">
					<div id="aria-data-source-name" class="mb-2 mt-2">Data source name:</div>
					<div class="input-wrapper">
						<InputText
							v-model="pipeline.data_source.name"
							type="text"
							class="w-full"
							placeholder="Enter a name for the data source"
							aria-labelledby="aria-data-source-name"
							@input="handleDataSourceNameInput"
						/>
					</div>

					<div id="aria-data-source-description" class="mb-2 mt-2">Data source description:</div>
					<div class="input-wrapper">
						<InputText
							v-model="pipeline.data_source.description"
							type="text"
							class="w-full"
							placeholder="Enter a description for the data source"
							aria-labelledby="aria-data-source-description"
						/>
					</div>

					<div id="aria-data-source-plugin" class="mb-2 mt-2">Data source plugin:</div>
					<div class="input-wrapper">
						<Dropdown
							v-model="selectedDataSourcePlugin"
							:options="dataSourcePluginOptions"
							option-label="display_name"
							class="w-full"
							placeholder="Select a data source plugin"
						/>
					</div>

					<template v-if="selectedDataSourcePlugin">
						<div id="aria-data-source-parameters" class="mb-2 mt-2">
							Data source default values:
						</div>
						<div class="input-wrapper">
							<div
								v-for="(param, index) in selectedDataSourcePlugin?.parameters"
								:key="index"
								style="width: 100%"
							>
								<label>{{ param.name }}:</label>
								<template
									v-if="
										param.type === 'string' ||
										param.type === 'int' ||
										param.type === 'float' ||
										param.type === 'datetime'
									"
								>
									<InputText v-model="param.default_value" style="width: 100%" />
								</template>
								<template v-else-if="param.type === 'bool'">
									<InputSwitch v-model="param.default_value" />
								</template>
								<template v-else-if="param.type === 'array'">
									<Chips
										v-model="param.default_value"
										style="width: 100%"
										placeholder="Enter values separated by commas"
										separator=","
									></Chips>
								</template>
								<template v-else-if="param.type === 'resource-object-id'">
									<Dropdown
										v-model="param.default_value"
										:options="param.parameter_metadata.parameter_selection_hints_options"
										option-label="display_name"
										option-value="value"
										class="w-full"
										placeholder="Select a resource"
									/>
								</template>
							</div>
						</div>
					</template>
				</template>
			</div>

			<!-- Pipeline Stages -->
			<div class="step-header col-span-2">Configure pipeline stages</div>
			<div class="col-span-2">
				<div class="stages-container">
					<div
						v-for="(stage, index) in selectedStagePlugins"
						:key="index"
						class="stage-wrapper"
						:class="{ 'stage-wrapper--root': index === 0 }"
						:style="getStageWrapperStyle(index)"
						draggable="true"
						@dragstart="handleDragStart(index)"
						@dragover.prevent
						@drop="handleDrop(index)"
					>
				<div v-if="index > 0" class="stage-connector">
					<span class="stage-connector__dot"></span>
				</div>
				<div class="stage-item">
						<div class="stage-header">
							<div class="stage-header__titles">
							<div class="stage-name">
								<label>Stage Name:</label>
								<InputText
									v-model="stage.name"
									class="w-full"
									@input="handleStageNameInput($event, index)"
								/>
							</div>
						</div>
						<div class="stage-header__actions">
							<Button icon="pi pi-trash" severity="danger"
								class="stage-action-button stage-action-button--danger" @click="stageToDelete = index" />
							<Button
								:icon="stage.collapsed ? 'pi pi-chevron-down' : 'pi pi-chevron-up'" class="stage-action-button"
								@click="toggleStageCollapse(index)"
							/>
						</div>
						</div>
						<div v-if="!stage.collapsed" class="stage-content">
							<div class="mb-2">
								<label>Description:</label>
								<InputText v-model="stage.description" class="w-full" />
							</div>
							<div class="mb-2">
								<label>Plugin:</label>
								<Dropdown
									v-model="stage.plugin_object_id"
									:options="stagePluginsOptions"
									option-label="display_name"
									option-value="object_id"
									class="w-full"
									placeholder="Select a plugin"
									@change="handleStagePluginChange($event, index)"
								/>
							</div>
							<div v-if="stage.plugin_parameters?.length > 0" class="mb-2">
								<label class="step-header">Parameters:</label>
								<div
									v-for="(param, paramIndex) in stage.plugin_parameters"
									:key="paramIndex"
									class="parameter-item"
								>
									<label>{{ param.parameter_metadata.name }}:</label>
									<div class="parameter-description">
										{{ param.parameter_metadata.description }}
									</div>
									<template
										v-if="
											param.parameter_metadata.type === 'string' ||
											param.parameter_metadata.type === 'int' ||
											param.parameter_metadata.type === 'float' ||
											param.parameter_metadata.type === 'datetime'
										"
									>
										<InputText v-model="param.default_value" style="width: 100%" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'bool'">
										<InputSwitch v-model="param.default_value" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'array'">
										<Chips
											v-model="param.default_value"
											style="width: 100%"
											placeholder="Enter values separated by commas"
											separator=","
										></Chips>
									</template>
									<template v-else-if="param.parameter_metadata.type === 'resource-object-id'">
										<Dropdown
											v-model="param.default_value"
											:options="
												stagePluginResourceOptions.find(
													(p) => p.parameter_metadata.name === param.parameter_metadata.name,
												)?.parameter_selection_hints_options || []
											"
											option-label="display_name"
											option-value="value"
											class="w-full"
											placeholder="Select a resource"
										/>
									</template>
								</div>
							</div>
							<div
								v-if="
									stagePluginsDependenciesOptions.find(
										(dep) => dep.plugin_object_id === stage.plugin_object_id,
									)
								"
								class="mb-2"
							>
								<label>Dependencies:</label>
								<Dropdown
									v-if="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.selection_type === 'Single' && stage.plugin_dependencies
									"
									v-model="stage.plugin_dependencies[0].plugin_object_id"
									:options="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.dependencyInfo || []
									"
									option-label="dependencyLabel"
									option-value="dependencies.plugin_object_id"
									class="w-full"
									placeholder="Select a dependency"
									@change="handleStagePluginDependencyChange($event, stage.plugin_object_id, index)"
								/>
								<MultiSelect
									v-if="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.selection_type === 'Multiple'
									"
									v-model="selectedDependencyIdsMap[stage.name]"
									:options="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.dependencyInfo || []
									"
									option-label="dependencyLabel"
									option-value="dependencies.plugin_object_id"
									class="w-full"
									placeholder="Select dependencies"
									@change="
										handleMultipleStagePluginDependencyChange($event, stage.plugin_object_id, index)
									"
								/>
							</div>
							<div
								v-for="dependency in stage.plugin_dependencies"
								:key="dependency.plugin_object_id"
								class="mb-2"
							>
								<template v-if="dependency.plugin_parameters?.length > 0">
									<label class="step-header"
										>{{
											stagePluginsDependenciesOptions
												.find((dep) => dep.plugin_object_id === stage.plugin_object_id)
												?.dependencyInfo.find(
													(d) => d.dependencies.plugin_object_id === dependency.plugin_object_id,
												)?.dependencyLabel
										}}
										Dependency Parameters:</label
									>
									<div
										v-for="(param, paramIndex) in dependency?.plugin_parameters"
										:key="paramIndex"
										class="parameter-item"
									>
										<label>{{ param.parameter_metadata.name }}:</label>
										<div class="parameter-description">
											{{ param.parameter_metadata.description }}
										</div>
										<template
											v-if="
												param.parameter_metadata.type === 'string' ||
												param.parameter_metadata.type === 'int' ||
												param.parameter_metadata.type === 'float' ||
												param.parameter_metadata.type === 'datetime'
											"
										>
											<InputText v-model="param.default_value" class="w-full" />
										</template>
										<template v-else-if="param.parameter_metadata.type === 'bool'">
											<InputSwitch v-model="param.default_value" />
										</template>
										<template v-else-if="param.parameter_metadata.type === 'array'">
											<Chips
												v-model="param.default_value"
												style="width: 100%"
												placeholder="Enter values separated by commas"
												separator=","
											></Chips>
										</template>
										<template v-else-if="param.parameter_metadata.type === 'resource-object-id'">
											<Dropdown
												v-model="param.default_value"
												:options="
													stagePluginDependencyResourceOptions.find(
														(p) => p.parameter_metadata.name === param.parameter_metadata.name,
													)?.parameter_selection_hints_options || []
												"
												option-label="display_name"
												option-value="value"
												class="w-full"
												placeholder="Select a resource"
											/>
										</template>
									</div>
								</template>
							</div>
						</div>
					</div>
				</div>
				<Button label="Add Stage" icon="pi pi-plus" class="add-stage-button" @click="addStage" />
				</div>
			</div>

			<!-- Pipeline Triggers -->
			<div class="step-header col-span-2">Configure pipeline triggers</div>
			<div class="col-span-2">
				<div class="trigger-container">
					<div
						v-for="(trigger, triggerIndex) in pipeline.triggers"
						:key="triggerIndex"
						class="mb-2 trigger-item"
					>
						<div class="trigger-header">
							<div class="mb-2">
								<label>Trigger Name:</label>
								<InputText
									v-model="trigger.name"
									class="w-full"
									@input="handleTriggerNameChange(triggerIndex)"
								/>
							</div>
							<Button
								icon="pi pi-trash"
								severity="danger"
								@click="triggerToDelete = triggerIndex"
							/>
							<Button
								:icon="
									triggerCollapseState[trigger.name] ? 'pi pi-chevron-down' : 'pi pi-chevron-up'
								"
								@click="toggleTriggerCollapse(triggerIndex)"
							/>
						</div>
						<div v-if="!triggerCollapseState[trigger.name]">
							<label>Trigger Type:</label>
							<Dropdown
								v-model="trigger.trigger_type"
								:options="triggerTypeOptions"
								option-label="label"
								option-value="value"
								class="w-full"
								placeholder="Select trigger type"
							/>
							<div v-if="trigger.trigger_type === 'Schedule'" class="mb-2">
								<label>Cron Schedule:</label>
								<InputText
									v-model="trigger.trigger_cron_schedule"
									class="w-full"
									placeholder="0 6 * * *"
								/>
							</div>
							<template v-if="triggerParameters[trigger.name]?.length > 0">
								<div class="step-header col-span-2 !mb-2">Trigger Parameters:</div>
								<div class="col-span-2">
									<div
										v-for="(param, index) in triggerParameters[trigger.name]"
										:key="index"
										class="mb-2"
									>
										<label>{{ param.parameter_metadata.name }}:</label>
										<div style="font-size: 12px">
											{{ param.key }}
										</div>
										<div style="font-size: 12px; color: #666">
											{{ param.parameter_metadata.description }}
										</div>
										<template
											v-if="
												param.parameter_metadata.type === 'string' ||
												param.parameter_metadata.type === 'int' ||
												param.parameter_metadata.type === 'float' ||
												param.parameter_metadata.type === 'datetime'
											"
										>
											<InputText
												v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
												class="w-full"
											/>
										</template>
										<template v-else-if="param.parameter_metadata.type === 'bool'">
											<InputSwitch
												v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
											/>
										</template>
										<template v-else-if="param.parameter_metadata.type === 'array'">
											<Chips
												v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
												style="width: 100%"
												placeholder="Enter values separated by commas"
												separator=","
											></Chips>
										</template>
										<template v-else-if="param.parameter_metadata.type === 'resource-object-id'">
											<Dropdown
												v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
												:options="param.resourceOptions"
												option-label="display_name"
												option-value="value"
												class="w-full"
												placeholder="Select a resource"
											/>
										</template>
									</div>
								</div>
							</template>
						</div>
					</div>
					<Button label="Add Trigger" icon="pi pi-plus" @click="addTrigger" />
				</div>
			</div>

			<!-- Buttons -->
			<div class="flex col-span-2 justify-end gap-4">
				<!-- Create pipeline -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Pipeline'"
					severity="primary"
					@click="handleCreatePipeline"
				/>

				<!-- Cancel -->
				<Button label="Cancel" severity="secondary" @click="handleCancel" />
			</div>
		</div>

		<Dialog :visible="stageToDelete !== null" modal header="Delete Stage" :closable="false">
			<p>Do you want to delete the stage "{{ selectedStagePlugins[stageToDelete].name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="stageToDelete = null" />
				<Button label="Delete" severity="danger" @click="removeStage(stageToDelete)" />
			</template>
		</Dialog>

		<Dialog :visible="triggerToDelete !== null" modal header="Delete Trigger" :closable="false">
			<p>Do you want to delete the trigger "{{ pipeline.triggers[triggerToDelete].name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="triggerToDelete = null" />
				<Button label="Delete" severity="danger" @click="removeTrigger(triggerToDelete)" />
			</template>
		</Dialog>
	</main>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';
import { useConfirmationStore } from '@/stores/confirmationStore';

export default {
	name: 'CreatePipeline',

	props: {
		editId: {
			type: [Boolean, String] as PropType<boolean | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: true as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null,
			validationMessage: null as string | null,

			dataSourceOptions: [] as any[],
			selectedDataSource: null as string | null,

			dataSourcePlugins: [] as any[],
			selectedDataSourcePlugin: null as any,
			dataSourcePluginOptions: [] as any[],

			stagePluginsOptions: [] as any[],
			selectedStagePlugins: [] as any[],
			stagePluginResourceOptions: [] as any[],

			stagePluginsDependenciesOptions: [] as any[],
			resolvedDependencies: [] as any[],
			stagePluginDependencyResourceOptions: [] as any[],
			selectedDependencyIdsMap: {} as any,

			triggerTypeOptions: [
				{ label: 'Schedule', value: 'Schedule' },
				{ label: 'Event', value: 'Event' },
				{ label: 'Manual', value: 'Manual' },
			],

			triggerParameters: [] as any[],
			triggerParametersMap: {} as any,

			pipeline: {
				type: 'data-pipeline',
				name: '',
				object_id: '',
				display_name: '',
				description: '',
				cost_center: null,
				active: false,
				data_source: {
					data_source_object_id: '',
					name: '',
					description: '',
					plugin_object_id: '',
					plugin_parameters: [],
					plugin_dependencies: [],
				},
				starting_stages: [],
				triggers: [],
				properties: null,
				created_on: '',
				updated_on: '',
				created_by: null,
				updated_by: null,
				deleted: false,
				expiration_date: null,
			},

			debouncedCheckName: null as (() => void) | null,
			draggedStageIndex: null as number | null,
			resourceOptions: [] as any[],
			resourceOptionsCache: {} as Record<string, any[]>, // Cache for resource options
			triggerCollapseState: {} as Record<string, boolean>,
			previousTriggerNames: {} as Record<number, string>,
			stageToDelete: null as number | null,
			triggerToDelete: null as number | null,
		};
	},

	computed: {
		isEditing(): boolean {
			return Boolean(this.editId);
		},

		pipelineId(): string | undefined {
			return typeof this.editId === 'string' ? this.editId : undefined;
		},
	},

	watch: {
		pipeline: {
			handler(newVal) {
				console.log(newVal);
			},
			deep: true,
		},
		selectedDataSourcePlugin: {
			async handler(newVal) {
				if (newVal) {
					this.pipeline.data_source.plugin_object_id = newVal.object_id;
					this.pipeline.data_source.plugin_parameters = newVal.parameters.map((param: any) => ({
						parameter_metadata: {
							name: param.name,
							type: param.type,
							description: param.description,
						},
						default_value: param.default_value,
					}));

					// Avoid multiple API requests
					const paramsToFetch = newVal.parameter_selection_hints
						? Object.keys(newVal.parameter_selection_hints)
						: [];

					// Use Promise.all to fetch all options in parallel
					const resourceOptions = await Promise.all(
						paramsToFetch.map((key) =>
							this.getResourceOptions(key, newVal.object_id).then((options) => ({ key, options })),
						),
					);

					// Assign options efficiently
					resourceOptions.forEach(({ key, options }) => {
						const param = this.selectedDataSourcePlugin.parameters.find((p) => p.name === key);
						if (param) {
							param.parameter_selection_hints_options = options;
						}
					});
				}
				this.buildTriggerParameters();
			},
			deep: true,
		},
		selectedStagePlugins: {
			handler(newVal) {
				this.transformPipelineStages();
				this.buildTriggerParameters();
				newVal.forEach((stage) => {
					this.loadStagePluginDependencies(stage.plugin_object_id);
				});
			},
			deep: true,
		},
	},

	async created() {
		this.loading = true;

		try {
			const [dataSources, dataSourcePlugins, stagePlugins] = await Promise.all([
				api.getAgentDataSources(),
				api.filterPlugins(['Data Source']),
				api.filterPlugins(['Data Pipeline Stage']),
			]);

			this.dataSourceOptions = dataSources.map((result) => result.resource);
			this.dataSourcePluginOptions = dataSourcePlugins;
			this.stagePluginsOptions = stagePlugins;

			this.stagePluginsOptions.map((plugin) => {
				if (Object.keys(plugin.parameter_selection_hints).length > 0) {
					const pluginParameters = plugin.parameters.filter(
						(param: any) => param.type === 'resource-object-id',
					);
					pluginParameters.forEach((param: any) => {
						if (
							this.stagePluginResourceOptions.find((p) => p.parameter_metadata.name === param.name)
						) {
							return;
						}
						this.stagePluginResourceOptions.push({
							parameter_metadata: param,
							parameter_selection_hints_options: [],
						});
					});
				}
			});

			if (this.pipelineId) {
				this.loadingStatusText = `Retrieving pipeline "${this.pipelineId}"...`;
				const pipelineResult = await api.getPipeline(this.pipelineId);
				this.pipeline = pipelineResult[0].resource;

				this.selectedDataSource = this.dataSourceOptions.find(
					(option) => option.object_id === this.pipeline.data_source.data_source_object_id,
				);

				this.selectedDataSourcePlugin = this.dataSourcePluginOptions.find(
					(plugin) => plugin.object_id === this.pipeline.data_source.plugin_object_id,
				);

				await this.handleNextStages(this.pipeline.starting_stages);
				this.buildTriggerParameters();

				// Initialize previousTriggerNames and triggerCollapseState for existing triggers
				this.pipeline.triggers.forEach((trigger, index) => {
					this.previousTriggerNames[index] = trigger.name;
					this.triggerCollapseState[trigger.name] = true; // Set initial state to collapsed
				});
			} else {
				this.addTrigger();
			}
		} catch (error) {
			console.error('Error loading data:', error);
			(this as any).$toast.add({
				severity: 'error',
				detail: 'Error loading pipeline data. Please try again.',
				life: 5000,
			});
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
		this.loading = false;
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkPipelineName(this.pipeline.name);

				if (response.status === 'Allowed') {
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
				}
			} catch (error) {
				console.error('Error checking pipeline name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the pipeline name. Please try again.';
			}
		},

		handleDataSourceChange(event: any) {
			// This needs to be updated to handle the data source change
			const selectedDataSource = this.dataSourceOptions.find(
				(p) => p.object_id === event.value.object_id,
			);
			if (selectedDataSource) {
				this.selectedDataSourcePlugin = null;

				this.pipeline.data_source = {
					data_source_object_id: selectedDataSource.object_id,
					name: '',
					description: '',
					plugin_object_id: '',
					plugin_parameters: [],
					plugin_dependencies: [],
				};
			}
		},

		async handleStagePluginChange(event: any, stageIndex: number) {
			const selectedPlugin = this.stagePluginsOptions.find((p) => p.object_id === event.value);

			this.selectedStagePlugins[stageIndex].plugin_object_id = selectedPlugin.object_id;
			this.selectedStagePlugins[stageIndex].plugin_parameters = 
				selectedPlugin.parameters.map(
					(param) => ({
						parameter_metadata: param,
						default_value: null,
					}))
				?? [];
			this.selectedStagePlugins[stageIndex].plugin_dependencies = [{ plugin_object_id: null }];

			for (const param of selectedPlugin.parameters) {
				const resourceOption = this.stagePluginResourceOptions.find(
					(p) => p.parameter_metadata.name === param.name,
				);
				if (resourceOption) {
					const options = await this.getResourceOptions(param.name, selectedPlugin.object_id);
					resourceOption.parameter_selection_hints_options = options;
				}
			}
		},

		handleStagePluginDependencyChange(event: any, pluginObjectId: string, index: number) {
			const selectedDependencyOption = this.stagePluginsDependenciesOptions.find(
				(p) => p.plugin_object_id === pluginObjectId,
			);
			const selectedDependency = selectedDependencyOption.dependencyInfo.find(
				(p) => p.dependencies.plugin_object_id === event.value,
			);
			this.selectedStagePlugins[index].plugin_dependencies[0].plugin_parameters =
				selectedDependency.dependencies.plugin_parameters;
		},

		handleMultipleStagePluginDependencyChange(event: any, pluginObjectId: string, index: number) {
			const dependencyOptions =
				this.stagePluginsDependenciesOptions.find((dep) => dep.plugin_object_id === pluginObjectId)
					?.dependencyInfo || [];
			const selectedDependencies = event.value.map(
				(id) =>
					dependencyOptions.find((option) => option.dependencies.plugin_object_id === id)
						?.dependencies,
			);
			this.selectedStagePlugins[index].plugin_dependencies = selectedDependencies;
		},

		addStage() {
			this.selectedStagePlugins.push({
				name: `Stage${this.selectedStagePlugins.length + 1}`,
				description: '',
				plugin_object_id: '',
				plugin_parameters: null,
				plugin_dependencies: [],
				collapsed: false,
			});
			this.transformPipelineStages();
		},

		removeStage(index: number) {
			this.selectedStagePlugins.splice(index, 1);
			this.transformPipelineStages();
			this.stageToDelete = null;
		},

		transformPipelineStages() {
			const nested = this.selectedStagePlugins.reduceRight((acc, stage) => {
				const { collapsed, ...stageData } = stage; // Exclude collapsed property
				return [
					{
						...stageData,
						next_stages: acc,
					},
				];
			}, []);

			this.pipeline.starting_stages = nested;
		},

		async handleCancel() {
			const confirmationStore = useConfirmationStore();
			const confirmed = await confirmationStore.confirmAsync({
				title: 'Cancel Pipeline Creation',
				message: 'Are you sure you want to cancel?',
				confirmText: 'Yes',
				cancelText: 'Cancel',
				confirmButtonSeverity: 'danger',
			});

			if (confirmed) {
				(this as any).$router.push('/pipelines');
			}
		},

		handleNameInput(event: Event) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.pipeline.name = sanitizedValue;
			// this.pipeline.display_name = sanitizedValue;

			if (!this.isEditing && this.debouncedCheckName) {
				this.debouncedCheckName();
			}
		},

		handleDataSourceNameInput(event: Event) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.pipeline.data_source.name = sanitizedValue;
		},

		handleStageNameInput(event: Event, index: number) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.selectedStagePlugins[index].name = sanitizedValue;
		},

		async handleCreatePipeline() {
			const pipeline = {
				name: this.pipeline.name,
				type: 'data-pipeline',
				display_name: this.pipeline.display_name,
				description: this.pipeline.description,
				active: false,
				data_source: this.pipeline.data_source,
				starting_stages: this.pipeline.starting_stages,
				triggers: this.pipeline.triggers,
			};
			try {
				if (this.isEditing) {
					await api.createPipeline(pipeline);
					this.$toast.add({
						severity: 'success',
						detail: 'Pipeline updated successfully!',
						life: 3000,
					});
				} else {
					await api.createPipeline(pipeline);
					this.$toast.add({
						severity: 'success',
						detail: 'Pipeline created successfully!',
						life: 3000,
					});
				}
				this.$router.push('/pipelines'); // Redirect to the pipelines list
			} catch (error) {
				console.error('Error saving pipeline:', error);
				this.$toast.add({
					severity: 'error',
					detail: 'Error saving pipeline. Please try again.',
					life: 5000,
				});
			}
		},

		async handleNextStages(stages: any[]) {
			for (const stage of stages) {
				this.selectedStagePlugins.push({
					name: stage.name,
					description: stage.description,
					plugin_object_id: stage.plugin_object_id,
					plugin_parameters: stage.plugin_parameters,
					plugin_dependencies: stage.plugin_dependencies,
					collapsed: true,
				});

				this.selectedDependencyIdsMap[stage.name] =
					stage.plugin_dependencies?.map((dep) => dep.plugin_object_id) ?? [];

				if (stage.plugin_parameters) {
					for (const param of stage.plugin_parameters) {
						const resourceOption = this.stagePluginResourceOptions.find(
							(p) => p.parameter_metadata.name === param.parameter_metadata.name,
						);
						if (resourceOption) {
							const options = await this.getResourceOptions(
								param.parameter_metadata.name,
								stage.plugin_object_id,
							);
							resourceOption.parameter_selection_hints_options = options;
						}
					}
				}

				this.loadStagePluginDependencies(stage.plugin_object_id);
				if (stage.next_stages) {
					await this.handleNextStages(stage.next_stages);
				}
			}
		},

		async buildTriggerParameters() {
			// // Check if there are existing trigger parameters
			// const existingTriggerParameters =
			// 	this.pipeline.triggers.length > 0 ? this.pipeline.triggers[0].parameter_values : {};
			const existingTriggerParameters = {};

			this.pipeline.triggers.forEach((trigger) => {
				existingTriggerParameters[trigger.name] = trigger.parameter_values;
			});

			this.pipeline.triggers.forEach(async (trigger) => {
				const parameterValues: any[] = [];
				// Data Source Parameters
				for (const param of this.pipeline.data_source.plugin_parameters) {
					const key = `DataSource.${this.pipeline.data_source.name}.${param.parameter_metadata.name}`;
					const value =
						existingTriggerParameters[trigger.name][key] !== undefined
							? existingTriggerParameters[trigger.name][key]
							: param.default_value;
					this.pipeline.triggers.forEach((trigger) => {
						if (!trigger.parameter_values[key]) {
							trigger.parameter_values[key] = null;
						}
					});
					const resourceOptions = await this.getResourceOptions(
						param.parameter_metadata.name,
						this.pipeline.data_source.plugin_object_id,
					);
					const type = 'data-source';
					parameterValues.push({
						parameter_metadata: param.parameter_metadata,
						key,
						value,
						resourceOptions,
						type,
					});
					if (!this.triggerParametersMap[key]) {
						this.triggerParametersMap[key] = param.default_value;
					}
				}

				// Recursive function to handle stages and their dependencies
				async function handleStages(stages: any[]) {
					for (const stage of stages) {
						// Stage Parameters
						if (stage.plugin_parameters) {
							for (const param of stage.plugin_parameters) {
								const key = `Stage.${stage.name}.${param.parameter_metadata.name}`;
								const value =
									existingTriggerParameters[trigger.name][key] !== undefined
										? existingTriggerParameters[trigger.name][key]
										: param.default_value;
								const resourceOptions = await this.getResourceOptions(
									param.parameter_metadata.name,
									stage.plugin_object_id,
								);
								const type = 'stage';
								const stageName = stage.name;
								parameterValues.push({
									parameter_metadata: param.parameter_metadata,
									key,
									value,
									resourceOptions,
									type,
									stageName,
								});
								if (!this.triggerParametersMap[key]) {
									this.triggerParametersMap[key] = param.default_value;
								}
							}
						}

						// Dependency Plugin Parameters
						if (stage.plugin_dependencies) {
							for (const dep of stage.plugin_dependencies) {
								if (dep.plugin_parameters?.length > 0) {
									for (const param of dep.plugin_parameters) {
										const depPluginName = dep.plugin_object_id.split('/').pop() || '';
										const key = `Stage.${stage.name}.Dependency.${depPluginName}.${param.parameter_metadata.name}`;
										const value =
											existingTriggerParameters[trigger.name][key] !== undefined
												? existingTriggerParameters[trigger.name][key]
												: param.default_value;
										const resourceOptions = await this.getResourceOptions(
											param.parameter_metadata.name,
											dep.plugin_object_id,
										);
										const type = 'dependency';
										const stageName = stage.name;
										parameterValues.push({
											parameter_metadata: param.parameter_metadata,
											key,
											value,
											resourceOptions,
											type,
											stageName,
										});
										if (!this.triggerParametersMap[key]) {
											this.triggerParametersMap[key] = param.default_value;
										}
									}
								}
							}
						}

						// Handle next stages recursively
						if (stage.next_stages) {
							await handleStages.call(this, stage.next_stages);
						}
					}
				}

				// Start with the initial stages
				await handleStages.call(this, this.pipeline.starting_stages);

				this.triggerParameters[trigger.name] = parameterValues;
			});
		},

		handleDragStart(index: number) {
			this.draggedStageIndex = index;
		},

		handleDrop(index: number) {
			if (this.draggedStageIndex !== null) {
				const draggedStage = this.selectedStagePlugins[this.draggedStageIndex];
				this.selectedStagePlugins.splice(this.draggedStageIndex, 1);
				this.selectedStagePlugins.splice(index, 0, draggedStage);
				this.draggedStageIndex = null;
				this.transformPipelineStages();
			}
		},

		addTrigger() {
			const newTriggerName = `Trigger${this.pipeline.triggers.length + 1}`;
			this.pipeline.triggers.push({
				name: newTriggerName,
				trigger_type: 'Schedule',
				trigger_cron_schedule: '0 6 * * *',
				parameter_values: { ...this.triggerParametersMap },
			});
			this.triggerCollapseState[newTriggerName] = false; // Initialize as collapsed
			this.previousTriggerNames[this.pipeline.triggers.length - 1] = newTriggerName; // Track the initial name
			this.buildTriggerParameters();
		},

		removeTrigger(index: number) {
			const triggerName = this.pipeline.triggers[index].name;
			this.pipeline.triggers.splice(index, 1);
			delete this.triggerCollapseState[triggerName]; // Remove the collapse state
			delete this.previousTriggerNames[index]; // Remove the previous name entry

			// Shift down the indexes in previousTriggerNames
			const updatedPreviousTriggerNames: Record<number, string> = {};
			this.pipeline.triggers.forEach((trigger, i) => {
				updatedPreviousTriggerNames[i] = this.previousTriggerNames[i >= index ? i + 1 : i];
			});
			this.previousTriggerNames = updatedPreviousTriggerNames;
			this.triggerToDelete = null;
		},

		toggleTriggerCollapse(index: number) {
			const triggerName = this.pipeline.triggers[index].name;
			this.triggerCollapseState[triggerName] = !this.triggerCollapseState[triggerName];
		},

		async loadStagePluginDependencies(pluginObjectId: string) {
			const plugin = this.stagePluginsOptions.find((p) => p.object_id === pluginObjectId);
			if (plugin && plugin.dependencies.length > 0) {
				const dependencies = plugin.dependencies[0]?.dependency_plugin_names || [];
				const dependencyPluginPromises = dependencies.map(async (dependency: string) => {
					const dependencyPlugin = await api.getPlugin(dependency);
					return dependencyPlugin[0].resource;
				});
				const resolvedDependencies = await Promise.all(dependencyPluginPromises);
				resolvedDependencies.map((dep) => {
					if (!this.resolvedDependencies.find((d) => d.object_id === dep.object_id)) {
						this.resolvedDependencies.push(dep);
					}

					if (Object.keys(dep.parameter_selection_hints).length > 0) {
						const depPluginParameters = dep.parameters.filter(
							(param: any) => param.type === 'resource-object-id',
						);
						depPluginParameters.map(async (param: any) => {
							if (
								this.stagePluginDependencyResourceOptions.find(
									(d) => d.parameter_metadata.name === param.name,
								)
							) {
								return;
							}
							const options = await this.getResourceOptions(param.name, dep.object_id);
							this.stagePluginDependencyResourceOptions.push({
								parameter_metadata: param,
								parameter_selection_hints_options: options,
							});
						});
					}
				});

				this.stagePluginsDependenciesOptions.push({
					plugin_object_id: plugin.object_id,
					selection_type: plugin.dependencies[0]?.selection_type || 'Single',
					dependencyInfo: resolvedDependencies.map((dep) => ({
						dependencyLabel: dep.display_name,
						dependencies: {
							plugin_object_id: dep.object_id,
							plugin_parameters: dep.parameters.map((param) => ({
								parameter_metadata: param,
								default_value:
									this.selectedStagePlugins
										.find((p) => p.plugin_object_id === plugin.object_id)
										?.plugin_dependencies.find((d) => d.plugin_object_id === dep.object_id)
										?.plugin_parameters.find((p) => p.parameter_metadata.name === param.name)
										?.default_value || null,
							})),
						},
					})),
				});
			}
		},

		async getResourceOptions(paramName: string, pluginObjectId: string) {
			const cacheKey = `${paramName}-${pluginObjectId}`;

			// Return cached options if available
			if (this.resourceOptionsCache[cacheKey]) {
				return this.resourceOptionsCache[cacheKey];
			}

			const plugin =
				this.stagePluginsOptions.find((p) => p.object_id === pluginObjectId) ||
				this.resolvedDependencies.find((dep) => dep.object_id === pluginObjectId) ||
				this.dataSourcePluginOptions.find((p) => p.object_id === pluginObjectId);

			if (
				!plugin ||
				!plugin.parameter_selection_hints ||
				!plugin.parameter_selection_hints[paramName]
			) {
				return [];
			}

			const hints = plugin.parameter_selection_hints[paramName];

			try {
				const response = await api.filterResources(hints.resourcePath, hints.filterActionPayload);
				const options = response.map((resource) => ({
					display_name: resource.display_name ?? resource.name,
					value: resource.object_id,
				}));

				// Cache the response to prevent redundant API calls
				this.resourceOptionsCache[cacheKey] = options;
				return options;
			} catch (error) {
				console.error('Error fetching resources:', error);
				return [];
			}
		},

		async updateResourceOptions(paramName: string, pluginObjectId: string) {
			this.resourceOptions = await this.getResourceOptions(paramName, pluginObjectId);
		},

		toggleStageCollapse(index: number) {
			this.selectedStagePlugins[index].collapsed = !this.selectedStagePlugins[index].collapsed;
		},

		getStageWrapperStyle(index: number) {
			return {
				'--indent': index.toString(),
			};
		},

		handleTriggerNameChange(triggerIndex: number) {
			const previousName = this.previousTriggerNames[triggerIndex];
			const newName = this.pipeline.triggers[triggerIndex].name;

			if (previousName !== newName) {
				// Update the collapse state with the new trigger name
				this.triggerCollapseState[newName] = this.triggerCollapseState[previousName];
				delete this.triggerCollapseState[previousName];

				// Update the trigger parameters with the new trigger name
				this.triggerParameters[newName] = this.triggerParameters[previousName];
				delete this.triggerParameters[previousName];

				// Update the previous name to the new name
				this.previousTriggerNames[triggerIndex] = newName;
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

.stages-container {
	display: flex;
	flex-direction: column;
	gap: 20px;
}

.stage-wrapper {
	position: relative;
	--indent: 0;
	padding-left: calc(var(--indent) * 32px);
}

.stage-wrapper--root {
	padding-left: 0;
}

.stage-item {
	position: relative;
	padding: 20px;
	border-radius: 16px;
	border: 1px solid #d7ddff;
	width: calc(100% - var(--indent) * 32px);
	max-width: 100%;
	backdrop-filter: blur(6px);
}

.stage-connector {
	position: absolute;
	top: -12px;
	bottom: -12px;
	left: calc(var(--indent) * 32px - 18px);
	width: 18px;
	pointer-events: none;
	display: flex;
	align-items: center;
	justify-content: center;
}

.stage-wrapper--root .stage-connector {
	display: none;
}

.stage-connector::before {
	content: '';
	position: absolute;
	top: 0;
	bottom: 0;
	left: 50%;
	width: 2px;
	transform: translateX(-50%);
	background: linear-gradient(180deg, rgba(76, 99, 255, 0.3) 0%, rgba(76, 99, 255, 0.75) 100%);
	border-radius: 999px;
}

.stage-connector__dot {
	position: absolute;
	bottom: -6px;
	left: 50%;
	transform: translate(-50%, 50%);
	width: 12px;
	height: 12px;
	border-radius: 50%;
	background: #4c63ff;
}

.stage-header {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
	margin-bottom: 16px;
}

.stage-header__titles {
	display: flex;
	flex-direction: column;
	gap: 12px;
	flex: 1;
}

.stage-header__actions {
	display: flex;
	align-items: center;
	gap: 10px;
}

.stage-order {
	display: inline-flex;
	align-items: center;
	align-self: flex-start;
	padding: 4px 12px;
	border-radius: 999px;
	background: rgba(76, 99, 255, 0.12);
	color: #3f4cff;
	font-size: 12px;
	font-weight: 600;
	letter-spacing: 0.02em;
	text-transform: uppercase;
}

.stage-name label {
	font-weight: 600;
	color: #222;
}

.stage-content {
	display: flex;
	flex-direction: column;
	gap: 16px;
	margin-top: 8px;
	padding-top: 16px;
	border-top: 1px solid rgba(76, 99, 255, 0.12);
}

.parameter-item {
	display: flex;
	flex-direction: column;
	gap: 6px;
	border-radius: 12px;
	padding: 12px;
}

.parameter-description {
	font-size: 12px;
	color: #6b7280;
	line-height: 1.4;
}

.trigger-item {
	border: 1px solid #e1e1e1;
	padding: 16px;
	border-radius: 4px;
}

.trigger-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;
}

.trigger-container {
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.add-stage-button {
	margin-top:30px;
}
</style>