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
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header span-2">What is the name of the pipeline?</div>
			<div class="span-2">
				<div id="aria-pipeline-name" class="mb-2">Pipeline name:</div>
				<div id="aria-pipeline-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.name"
						:disabled="isEditing"
						type="text"
						class="w-100"
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
                        class="w-100"
                        placeholder="Enter a display name for this pipeline"
                    />
                </div>
                
				<div id="aria-pipeline-desc" class="mb-2 mt-2">Pipeline description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.description"
						type="text"
						class="w-100"
						placeholder="Enter a description for this pipeline"
						aria-labelledby="aria-pipeline-desc"
					/>
				</div>
			</div>

			<!-- Data Source -->
			<div class="step-header span-2">Select a data source</div>
			<div class="span-2">
				<Dropdown
					v-model="selectedDataSource"
					:options="dataSourceOptions"
					option-label="name"
					class="w-100"
					placeholder="Select a data source"
					@change="handleDataSourceChange"
				/>

                <div id="aria-data-source-name" class="mb-2 mt-2">Data source name:</div>
                <div class="input-wrapper">
                    <InputText
                        v-model="pipeline.data_source.name"
                        type="text"
                        class="w-100"
                        placeholder="Enter a name for the data source"
                        aria-labelledby="aria-data-source-name"
                    />
                </div>

                <div id="aria-data-source-description" class="mb-2 mt-2">Data source description:</div>
                <div class="input-wrapper">
                    <InputText
                        v-model="pipeline.data_source.description"
                        type="text"
                        class="w-100"
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
                        class="w-100"
                        placeholder="Select a data source plugin"
                    />
                </div>

                <div id="aria-data-source-parameters" class="mb-2 mt-2">Data source default values:</div>
                <div class="input-wrapper">
                    <div v-for="(param, index) in selectedDataSourcePlugin?.parameters" :key="index" style="width: 100%;">
                        <label>{{ param.name }}:</label>
                        <template v-if="param.type === 'string' || param.type === 'int' || param.type === 'float' || param.type === 'datetime' || param.type === 'resource_object_id'">
                            <InputText v-model="param.default_value" style="width: 100%;" />
                        </template>
                        <template v-else-if="param.type === 'bool'">
                            <InputSwitch v-model="param.default_value" />
                        </template>
                        <template v-else-if="param.type === 'array'">
                            <Chips v-model="param.default_value" style="width: 100%;" placeholder="Enter values separated by commas" separator="," ></Chips>
                        </template>
                    </div>
                </div>
			</div>

			<!-- Pipeline Stages -->
			<div class="step-header span-2">Configure pipeline stages</div>
			<div class="span-2">
				<div class="stages-container">
					<div v-for="(stage, index) in selectedStagePlugins" :key="index" class="stage-item">
						<div class="stage-header">
                            <div class="mb-2">
                                <label>Stage Name:</label>
                                <InputText v-model="stage.name" class="w-100" />
                            </div>
                            <Button icon="pi pi-trash" severity="danger" @click="removeStage(index)" />
						</div>
						<div class="stage-content">
							<div class="mb-2">
								<label>Description:</label>
								<InputText v-model="stage.description" class="w-100" />
							</div>
							<div class="mb-2">
								<label>Plugin:</label>
								<Dropdown
									v-model="stage.plugin_object_id"
									:options="stagePluginsOptions"
									option-label="display_name"
									option-value="object_id"
									class="w-100"
									placeholder="Select a plugin"
									@change="handleStagePluginChange($event, index)"
								/>
							</div>
							<div v-if="stage.plugin_parameters" class="mb-2">
								<label class="step-header">Parameters:</label>
								<div v-for="(param, paramIndex) in stage.plugin_parameters" :key="paramIndex" class="parameter-item">
									<label>{{ param.parameter_metadata.name }}:</label>
                                    <div style="font-size: 12px; color: #666;">{{ param.parameter_metadata.description }}</div>
									<template v-if="param.parameter_metadata.type === 'string' || param.parameter_metadata.type === 'int' || param.parameter_metadata.type === 'float' || param.parameter_metadata.type === 'datetime' || param.parameter_metadata.type === 'resource_object_id'">
										<InputText v-model="param.default_value" style="width: 100%;" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'bool'">
										<InputSwitch v-model="param.default_value" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'array'">
										<Chips v-model="param.default_value" style="width: 100%;" placeholder="Enter values separated by commas" separator="," ></Chips>
									</template>
								</div>
							</div>
                            <div v-if="stagePluginsDependenciesOptions.find(dep => dep.plugin_object_id === stage.plugin_object_id)" class="mb-2">
                                <label>Dependencies:</label>
                                <Dropdown
                                    v-model="stage.plugin_dependencies[0]"
                                    :options="stagePluginsDependenciesOptions.find(dep => dep.plugin_object_id === stage.plugin_object_id)?.dependencyInfo || []"
                                    option-label="dependencyLabel"
                                    option-value="dependencies"
                                    class="w-100"
                                    placeholder="Select a dependency"
                                />
                            </div>
                            <div v-if="stage.plugin_dependencies[0]?.plugin_parameters" class="mb-2">
                                <label class="step-header">Dependency Parameters:</label>
                                <div v-for="(param, paramIndex) in stage?.plugin_dependencies[0]?.plugin_parameters" :key="paramIndex" class="parameter-item">
                                    <label>{{ param.parameter_metadata.name }}:</label>
                                    <div style="font-size: 12px; color: #666;">{{ param.parameter_metadata.description }}</div>
                                    <template v-if="param.parameter_metadata.type === 'string' || param.parameter_metadata.type === 'int' || param.parameter_metadata.type === 'float' || param.parameter_metadata.type === 'datetime' || param.parameter_metadata.type === 'resource_object_id'">
                                        <InputText v-model="param.default_value" class="w-100" />
                                    </template>
                                    <template v-else-if="param.parameter_metadata.type === 'bool'">
                                        <InputSwitch v-model="param.default_value" />
                                    </template>
                                    <template v-else-if="param.parameter_metadata.type === 'array'">
                                        <Chips v-model="param.default_value" style="width: 100%;" placeholder="Enter values separated by commas" separator="," ></Chips>
                                    </template>
                                </div>
                            </div>
						</div>
					</div>
					<Button label="Add Stage" icon="pi pi-plus" @click="addStage" />
				</div>
			</div>

			<!-- Pipeline Trigger -->
			<div class="step-header span-2">Configure pipeline trigger</div>
			<div class="span-2">
				<div class="trigger-container">
					<div class="mb-2">
						<label>Trigger Type:</label>
						<Dropdown
							v-model="selectedTriggerType"
							:options="triggerTypeOptions"
							option-label="label"
							option-value="value"
							class="w-100"
							placeholder="Select trigger type"
						/>
					</div>
					<div v-if="selectedTriggerType === 'Schedule'" class="mb-2">
						<label>Cron Schedule:</label>
						<InputText v-model="cronSchedule" class="w-100" placeholder="0 6 * * *" />
					</div>
				</div>
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create pipeline -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Pipeline'"
					severity="primary"
					@click="handleCreatePipeline"
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
	name: 'CreatePipeline',

	props: {
		editId: {
			type: [Boolean, String] as PropType<boolean | string>,
			required: false,
			default: false,
		},
	},

	computed: {
		isEditing(): boolean {
			return Boolean(this.editId);
		},

		pipelineId(): string | undefined {
			return typeof this.editId === 'string' ? this.editId : undefined;
		}
	},

    watch: {
        pipeline: {
            handler(newVal) {
                console.log(newVal);
            },
            deep: true
        },
        // selectedDataSourcePlugin: {
        //     handler(newVal) {
        //         console.log(newVal);
        //     },
        //     deep: true
        // },
        // selectedDataSource: {
        //     handler(newVal) {
        //         console.log(newVal);
        //     },
        //     deep: true
        // }
        selectedStagePlugins: {
            handler(newVal) {
                console.log(newVal);
                this.transformPipelineStages();
            },
            deep: true
        }
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
            stagePluginsDependenciesOptions: [] as any[],

			selectedTriggerType: 'Schedule' as string,
			cronSchedule: '0 6 * * *' as string,

			triggerTypeOptions: [
				{ label: 'Schedule', value: 'Schedule' },
				{ label: 'Event', value: 'Event' },
				{ label: 'Manual', value: 'Manual' }
			],

            triggerParameterOptions: [] as any[],

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
					plugin_dependencies: []
				},
				starting_stages: [],
				triggers: [],
				properties: null,
				created_on: '',
				updated_on: '',
				created_by: null,
				updated_by: null,
				deleted: false,
				expiration_date: null
			},

			debouncedCheckName: null as (() => void) | null,
		};
	},

	async created() {
		this.loading = true;

		try {
            // Load data sources
            const dataSourceResponse = await api.getAgentDataSources();
            this.dataSourceOptions = dataSourceResponse.map(result => result.resource);

			// Load data source plugins
			const dataSourcePluginsResponse = await api.filterPlugins(['Data Source']);
			this.dataSourcePluginOptions = dataSourcePluginsResponse;

			// Load stage plugins
			const stagePluginsResponse = await api.filterPlugins(['Data Pipeline Stage']);
			this.stagePluginsOptions = stagePluginsResponse.map(result => ({
				...result,
				object_id: this.updateObjectId(result.object_id)
			}));

            const dependencyPromises = this.stagePluginsOptions.map(async (plugin: Plugin) => {
                // console.log(plugin);
                const dependencies = plugin.dependencies[0]?.dependency_plugin_names || [];
                const dependencyPluginPromises = dependencies.map(async (dependency: string) => {
                    const dependencyPlugin = await api.getPlugin(dependency);
                    return dependencyPlugin[0].resource;
                });
                const resolvedDependencies = await Promise.all(dependencyPluginPromises);
                console.log(resolvedDependencies);
                this.stagePluginsDependenciesOptions.push({
                    plugin_object_id: this.updateObjectId(plugin.object_id),
                    selection_type: plugin.dependencies[0]?.selection_type || 'Single',
                    dependencyInfo: resolvedDependencies.map(dep => ({
                        dependencyLabel: dep.display_name,
                        dependencies: {
                            plugin_object_id: this.updateObjectId(dep.object_id),
                            plugin_parameters: dep.parameters.map(param => ({
                                parameter_metadata: param,
                                default_value: null
                            }))
                        },
                    }))
                });
            });

            await Promise.all(dependencyPromises);
            console.log(this.stagePluginsDependenciesOptions);

			if (this.pipelineId) {
				this.loadingStatusText = `Retrieving pipeline "${this.pipelineId}"...`;
				const pipelineResult = await api.getPipeline(this.pipelineId);
				this.pipeline = pipelineResult[0].resource;

				this.selectedDataSource = this.dataSourceOptions.find(option => option.object_id === `/${this.pipeline.data_source.data_source_object_id}`);

                this.selectedDataSourcePlugin = this.dataSourcePluginOptions.find(plugin => this.updateObjectId(plugin.object_id) === this.pipeline.data_source.plugin_object_id);

                this.handleNextStages(this.pipeline.starting_stages);
                console.log(this.selectedStagePlugins);

                // const testGetPlugin = await api.getPlugin('Dotnet-FoundationaLLMDataPipelinePlugins-TokenContentTextPartitioning');

				// Set trigger values if they exist
				if (this.pipeline.triggers.length > 0) {
					const trigger = this.pipeline.triggers[0];
					this.selectedTriggerType = trigger.trigger_type;
					this.cronSchedule = trigger.trigger_cron_schedule;
				}
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
			const selectedDataSource = this.dataSourceOptions.find(p => p.object_id === event.value.object_id);
            console.log(this.dataSourceOptions);
            console.log(event.value);
            console.log(selectedPlugin);
			if (selectedDataSource) {
				// Create a unique name for the data source based on the plugin name
				const dataSourceName = selectedDataSource.name.split('-').pop() || selectedDataSource.name;
				
				this.pipeline.data_source = {
					data_source_object_id: selectedDataSource.object_id,
					name: dataSourceName,
					description: selectedDataSource.description,
					plugin_object_id: selectedDataSource.object_id,
					plugin_parameters: selectedDataSource.parameters.map(param => ({
						parameter_metadata: {
							name: param.name,
							type: param.type,
							description: param.description
						},
						default_value: param.type === 'array' ? [] : null
					})),
					plugin_dependencies: []
				};

				// Handle plugin dependencies
				if (selectedPlugin.dependencies) {
					selectedPlugin.dependencies.forEach(dep => {
						if (dep.selection_type === 'Multiple') {
							// For multiple selection, add all plugins since they're required for file type handling
							dep.dependency_plugin_names.forEach(pluginName => {
								this.pipeline.data_source.plugin_dependencies.push({
									plugin_object_id: pluginName,
									plugin_parameters: []
								});
							});
						}
					});
				}
			}
		},

		handleStagePluginChange(event: any, stageIndex: number) {
            // console.log(event);
            // console.log(stageIndex);
            const selectedPlugin = this.stagePluginsOptions.find(p => p.object_id === event.value);
            // console.log(selectedPlugin);
            // console.log(this.selectedStagePlugins);
            this.selectedStagePlugins[stageIndex].plugin_object_id = selectedPlugin.object_id;
            this.selectedStagePlugins[stageIndex].plugin_parameters = selectedPlugin.parameters.map(param => ({
                parameter_metadata: param,
                default_value: null
            }));
            this.selectedStagePlugins[stageIndex].plugin_dependencies = selectedPlugin?.dependencies;
            // console.log(this.selectedStagePlugins[stageIndex]);
            
            
			// const selectedPlugin = this.stagePluginsOptions.find(p => p.object_id === event.value);
			// if (selectedPlugin) {
			// 	this.pipeline.starting_stages[stageIndex].plugin_object_id = selectedPlugin.object_id;
			// 	this.pipeline.starting_stages[stageIndex].plugin_parameters = selectedPlugin.parameters.map(param => ({
			// 		parameter_metadata: param,
			// 		default_value: null
			// 	}));

			// 	// Handle plugin dependencies
			// 	this.pipeline.starting_stages[stageIndex].plugin_dependencies = [];
			// 	selectedPlugin.dependencies.forEach(dep => {
			// 		if (dep.selection_type === 'Single') {
			// 			// Add first plugin as default for single selection
			// 			this.pipeline.starting_stages[stageIndex].plugin_dependencies.push({
			// 				plugin_object_id: dep.dependency_plugin_names[0],
			// 				plugin_parameters: []
			// 			});
			// 		} else if (dep.selection_type === 'Multiple') {
			// 			// For multiple selection, user needs to select which plugins to use
			// 			dep.dependency_plugin_names.forEach(pluginName => {
			// 				this.pipeline.starting_stages[stageIndex].plugin_dependencies.push({
			// 					plugin_object_id: pluginName,
			// 					plugin_parameters: []
			// 				});
			// 			});
			// 		}
			// 	});
			// }
		},

        handleStagePluginDependencyChange(event: any, index: number) {
            const selectedDependency = event.value;
            console.log(selectedDependency);
            this.selectedStagePlugins[index].plugin_dependencies = [{
                plugin_object_id: selectedDependency.object_id,
                plugin_parameters: selectedDependency.parameters.map(param => ({
                    parameter_metadata: param,
                    default_value: null
                })),
            }];
            console.log(this.selectedStagePlugins[index]);
        },

		addStage() {
			// this.pipeline.starting_stages.push({
			// 	name: `Stage${this.pipeline.starting_stages.length + 1}`,
			// 	description: '',
			// 	plugin_object_id: '',
			// 	plugin_parameters: null,
			// 	plugin_dependencies: [],
			// 	next_stages: []
			// });
            this.selectedStagePlugins.push({
                name: `Stage${this.selectedStagePlugins.length + 1}`,
                description: '',
                plugin_object_id: '',
                plugin_parameters: null,
                plugin_dependencies: [],
            });
            this.transformPipelineStages();
		},

		removeStage(index: number) {
			// this.pipeline.starting_stages.splice(index, 1);
            this.selectedStagePlugins.splice(index, 1);
            this.transformPipelineStages();
		},

        transformPipelineStages() {
            let nested = this.selectedStagePlugins.reduceRight((acc, stage) => {
                return [{ 
                    ...stage, 
                    next_stages: acc 
                }];
            }, []);

            this.pipeline.starting_stages = nested;
        },

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			(this as any).$router.push('/pipelines');
		},

		handleNameInput(event: Event) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.pipeline.name = sanitizedValue;
			this.pipeline.display_name = sanitizedValue;

			if (!this.isEditing && this.debouncedCheckName) {
				this.debouncedCheckName();
			}
		},

		async handleCreatePipeline() {
			// const errors: string[] = [];
			// if (!this.pipeline.name) {
			// 	errors.push('Please give the pipeline a name.');
			// }
			// if (this.nameValidationStatus === 'invalid') {
			// 	errors.push(this.validationMessage || 'Invalid name');
			// }
			// if (!this.selectedDataSource) {
			// 	errors.push('Please select a data source.');
			// }
			// if (this.pipeline.starting_stages.length === 0) {
			// 	errors.push('Please add at least one stage to the pipeline.');
			// }

			// if (errors.length > 0) {
			// 	(this as any).$toast.add({
			// 		severity: 'error',
			// 		detail: errors.join('\n'),
			// 		life: 5000,
			// 	});

			// 	return;
			// }

			// // Add trigger
			// this.pipeline.triggers = [{
			// 	name: 'Default pipeline schedule',
			// 	trigger_type: this.selectedTriggerType,
			// 	trigger_cron_schedule: this.cronSchedule,
			// 	parameter_values: this.buildParameterValues()
			// }];

			// this.loading = true;
			// let successMessage = null as null | string;
			// try {
			// 	this.loadingStatusText = 'Saving pipeline...';
			// 	await api.createPipeline(this.pipeline);
			// 	successMessage = `Pipeline "${this.pipeline.name}" was successfully saved.`;
			// } catch (error: any) {
			// 	this.loading = false;
			// 	return (this as any).$toast.add({
			// 		severity: 'error',
			// 		detail: error?.response?._data || error,
			// 		life: 5000,
			// 	});
			// }

			// (this as any).$toast.add({
			// 	severity: 'success',
			// 	detail: successMessage,
			// 	life: 5000,
			// });

			// this.loading = false;

			// if (!this.editId) {
			// 	(this as any).$router.push('/pipelines');
			// }
		},

		// buildParameterValues(): Record<string, any> {
		// 	const parameterValues: Record<string, any> = {};

		// 	// Add data source parameters
		// 	if (this.pipeline.data_source.plugin_parameters) {
		// 		this.pipeline.data_source.plugin_parameters.forEach(param => {
		// 			const key = `DataSource.${this.pipeline.data_source.name}.${param.parameter_metadata.name}`;
		// 			parameterValues[key] = param.default_value;
		// 		});
		// 	}

		// 	// Add stage parameters
		// 	this.pipeline.starting_stages.forEach(stage => {
		// 		if (stage.plugin_parameters) {
		// 			stage.plugin_parameters.forEach(param => {
		// 				const key = `Stage.${stage.name}.${param.parameter_metadata.name}`;
		// 				parameterValues[key] = param.default_value;
		// 			});
		// 		}

		// 		// Add dependency parameters
		// 		stage.plugin_dependencies.forEach(dep => {
		// 			dep.plugin_parameters.forEach(param => {
		// 				const depPluginName = dep.plugin_object_id.split('/').pop() || '';
		// 				const key = `Stage.${stage.name}.Dependency.${depPluginName}.${param.parameter_metadata.name}`;
		// 				parameterValues[key] = param.default_value;
		// 			});
		// 		});
		// 	});

		// 	return parameterValues;
		// },

        updateObjectId(objectId: string): string {
			// Extract the plugin name from the object_id
			let parts = objectId.split('/');
            const pluginName = parts[parts.length - 1];
            const newPluginName = pluginName.split('-').pop() || pluginName;
            parts[parts.length - 1] = newPluginName;

            const newObjectId = parts.join('/');
			return newObjectId;
		},

        handleNextStages(stages: PipelineStage[]) {
            stages.forEach(stage => {
                // console.log(this.selectedStagePlugins);
                this.selectedStagePlugins.push({
                    name: stage.name,
                    description: stage.description,
                    plugin_object_id: stage.plugin_object_id,
                    plugin_parameters: stage.plugin_parameters,
                    plugin_dependencies: stage.plugin_dependencies,
                });
                if (stage.next_stages) {
                    this.handleNextStages(stage.next_stages);
                }
            });
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
	gap: 16px;
}

.stage-item {
	border: 1px solid #e1e1e1;
	padding: 16px;
	border-radius: 4px;
}

.stage-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;

	h3 {
		margin: 0;
	}
}

.stage-content {
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.parameter-item {
	display: flex;
	flex-direction: column;
	gap: 4px;
	margin-bottom: 8px;
}

.trigger-container {
	display: flex;
	flex-direction: column;
	gap: 8px;
}
</style>
