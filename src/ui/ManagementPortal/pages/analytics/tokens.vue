<template>
	<div class="analytics-tokens-page">
		<h1>Token Usage Analytics</h1>

		<div v-if="loading" class="loading">
			<ProgressSpinner />
		</div>

		<div v-else-if="error" class="error">
			<Message severity="error">{{ error }}</Message>
		</div>

		<div v-else>
			<div class="filters-row">
				<div class="date-range-selector">
					<Button 
						:label="dateRangeLabel" 
						icon="pi pi-calendar" 
						@click="toggleDateRangePanel"
					/>
					<OverlayPanel ref="dateRangePanel" :dismissable="true">
						<div class="date-range-menu">
							<div class="date-range-menu-header">
								<Button 
									label="< PREVIOUS" 
									text 
									severity="secondary"
									@click="navigateDateRange(-1)"
								/>
								<Button 
									label="NEXT >" 
									text 
									severity="secondary"
									@click="navigateDateRange(1)"
								/>
							</div>
							
							<div class="date-range-section">
								<div class="section-title">Recommended</div>
								<div class="date-range-option" @click="setDateRange('last7days')">
									<span>Last 7 days</span>
									<span class="date-preview">{{ formatDateRange('last7days') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('thisMonth')">
									<span>This month</span>
									<span class="date-preview">{{ formatDateRange('thisMonth') }}</span>
								</div>
								<div class="date-range-option" @click="showCustomDateRange">
									<span>Custom date range</span>
									<span class="pi pi-chevron-right"></span>
								</div>
							</div>

							<Divider />

							<div class="date-range-section">
								<div class="section-title">Relative dates</div>
								<div class="date-range-option" @click="setDateRange('last7days')">
									<span>Last 7 days</span>
									<span class="date-preview">{{ formatDateRange('last7days') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last30days')">
									<span>Last 30 days</span>
									<span class="date-preview">{{ formatDateRange('last30days') }}</span>
								</div>
							</div>

							<Divider />

							<div class="date-range-section">
								<div class="section-title">Calendar months</div>
								<div class="date-range-option" @click="setDateRange('thisMonth')">
									<span>This month</span>
									<span class="date-preview">{{ formatDateRange('thisMonth') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('thisQuarter')">
									<span>This quarter</span>
									<span class="date-preview">{{ formatDateRange('thisQuarter') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('thisYear')">
									<span>This year</span>
									<span class="date-preview">{{ formatDateRange('thisYear') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('lastMonth')">
									<span>Last month</span>
									<span class="date-preview">{{ formatDateRange('lastMonth') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('lastQuarter')">
									<span>Last quarter</span>
									<span class="date-preview">{{ formatDateRange('lastQuarter') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last3months')">
									<span>Last 3 months</span>
									<span class="date-preview">{{ formatDateRange('last3months') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last6months')">
									<span>Last 6 months</span>
									<span class="date-preview">{{ formatDateRange('last6months') }}</span>
								</div>
								<div class="date-range-option" @click="setDateRange('last12months')">
									<span>Last 12 months</span>
									<span class="date-preview">{{ formatDateRange('last12months') }}</span>
								</div>
								<div class="date-range-option" @click="showCustomDateRange">
									<span>Custom date range</span>
									<span class="pi pi-chevron-right"></span>
								</div>
							</div>
						</div>
					</OverlayPanel>

					<Dialog 
						v-model:visible="showCustomDialog" 
						modal 
						header="Custom Date Range"
						:style="{ width: '400px' }"
					>
						<div class="custom-date-range-dialog">
							<div class="date-input-group">
								<label>Start Date:</label>
								<Calendar v-model="startDate" dateFormat="yy-mm-dd" placeholder="Start Date" />
							</div>
							<div class="date-input-group">
								<label>End Date:</label>
								<Calendar v-model="endDate" dateFormat="yy-mm-dd" placeholder="End Date" />
							</div>
							<div class="dialog-actions">
								<Button label="Cancel" severity="secondary" @click="showCustomDialog = false" />
								<Button label="Apply" @click="applyCustomDateRange" />
							</div>
						</div>
					</Dialog>
				</div>
			</div>

			<!-- Summary Cards -->
			<div class="metrics-grid">
				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumber(totalTokens) }}</div>
						<div class="metric-label">Total Tokens Used</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumberWithCommas(totalUsers) }}</div>
						<div class="metric-label">Total Users</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumberWithCommas(totalAgents) }}</div>
						<div class="metric-label">Total Agents</div>
					</template>
				</Card>

				<Card class="metric-card">
					<template #content>
						<div class="metric-value">{{ formatNumberWithCommas(totalModels) }}</div>
						<div class="metric-label">Total Models</div>
					</template>
				</Card>
			</div>

			<!-- Charts Row - Top Users and Model Distribution -->
			<div class="charts-row">
				<!-- Top Users Pie Chart -->
				<div v-if="topUsersChartData" class="chart-container chart-half">
					<Card>
						<template #title>Top 10 Users by Token Consumption</template>
						<template #content>
							<div class="pie-chart-wrapper">
								<canvas ref="topUsersPieCanvas"></canvas>
							</div>
						</template>
					</Card>
				</div>

				<!-- Model Token Distribution Pie Chart -->
				<div v-if="modelChartData.length > 0" class="chart-container chart-half">
					<Card>
						<template #title>Token Usage by Model</template>
						<template #content>
							<div class="pie-chart-wrapper">
								<canvas ref="modelPieCanvas"></canvas>
							</div>
						</template>
					</Card>
				</div>
			</div>

			<!-- Token Usage by Model -->
			<div class="table-section">
				<Card>
					<template #title>Token Usage by Model</template>
					<template #content>
						<Message severity="info" :closable="false" class="info-message">
							<span>
								<strong>Note:</strong> Token usage is attributed to each agent's primary model. 
								Agents may use additional models for tools, prompt rewriting, or caching, 
								which are shown in the "Agents Using" count but tokens are attributed to the main model.
							</span>
						</Message>
						<DataTable 
							:value="modelAnalytics" 
							:paginator="true" 
							:rows="10" 
							class="tokens-table"
							sortField="total_tokens"
							:sortOrder="-1"
						>
							<Column field="model_name" header="Model Name" sortable></Column>
							<Column field="deployment_name" header="Deployment" sortable></Column>
							<Column field="agent_count" header="Agents Using" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.agent_count ?? 0) }}
								</template>
							</Column>
							<Column field="total_tokens" header="Total Tokens" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_tokens ?? 0) }}
								</template>
							</Column>
							<Column header="% of Total">
								<template #body="slotProps">
									{{ getPercentageOfTotal(slotProps.data.total_tokens ?? 0, totalModelTokens) }}%
								</template>
							</Column>
						</DataTable>
					</template>
				</Card>
			</div>

			<!-- Token Usage by Agent -->
			<div class="table-section">
				<Card>
					<template #title>Token Usage by Agent</template>
					<template #content>
						<DataTable 
							:value="agentsWithModels" 
							:paginator="true" 
							:rows="10" 
							class="tokens-table"
							sortField="total_tokens"
							:sortOrder="-1"
						>
							<Column field="agent_name" header="Agent Name" sortable></Column>
							<Column field="model_name" header="Primary Model" sortable>
								<template #body="slotProps">
									<span>{{ slotProps.data.model_name }}</span>
									<span v-if="slotProps.data.model_count > 1" class="model-count-badge">
										+{{ slotProps.data.model_count - 1 }} more
									</span>
								</template>
							</Column>
							<Column field="unique_users" header="Unique Users" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.unique_users ?? 0) }}
								</template>
							</Column>
							<Column field="total_conversations" header="Conversations" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_conversations ?? 0) }}
								</template>
							</Column>
							<Column field="total_tokens" header="Total Tokens" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_tokens ?? 0) }}
								</template>
							</Column>
							<Column header="% of Total">
								<template #body="slotProps">
									{{ getPercentageOfTotal(slotProps.data.total_tokens ?? 0, totalAgentTokens) }}%
								</template>
							</Column>
						</DataTable>
					</template>
				</Card>
			</div>

			<!-- Token Usage by User -->
			<div class="table-section">
				<Card>
					<template #title>Token Usage by User</template>
					<template #content>
						<div class="filters-row-bottom">
							<div class="search-filter">
								<span class="p-input-icon-left">
									<i class="pi pi-search" />
									<InputText 
										v-model="usernameFilter" 
										placeholder="Search usernames" 
										class="p-inputtext-sm"
									/>
								</span>
							</div>
						</div>
						<DataTable 
							:value="filteredUsers" 
							:paginator="true" 
							:rows="10" 
							class="tokens-table"
							sortField="total_tokens"
							:sortOrder="-1"
						>
							<Column field="username" header="Username" sortable></Column>
							<Column field="total_conversations" header="Total Conversations" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_conversations ?? 0) }}
								</template>
							</Column>
							<Column field="total_messages" header="Total Messages" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_messages ?? 0) }}
								</template>
							</Column>
							<Column field="total_tokens" header="Total Tokens" sortable>
								<template #body="slotProps">
									{{ formatNumberWithCommas(slotProps.data.total_tokens ?? 0) }}
								</template>
							</Column>
							<Column header="% of Total">
								<template #body="slotProps">
									{{ getPercentageOfTotal(slotProps.data.total_tokens ?? 0, totalUserTokens) }}%
								</template>
							</Column>
						</DataTable>
					</template>
				</Card>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch, nextTick } from 'vue';
import api from '@/js/api';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

const loading = ref(true);
const error = ref<string | null>(null);
const agents = ref<any[]>([]);
const users = ref<any[]>([]);
const agentDefinitions = ref<any[]>([]);
const aiModels = ref<any[]>([]);
const usernameFilter = ref<string>('');
const topUsersPieCanvas = ref<HTMLCanvasElement | null>(null);
const modelPieCanvas = ref<HTMLCanvasElement | null>(null);
let topUsersPieChart: Chart | null = null;
let modelPieChart: Chart | null = null;
const topUsersChartData = ref<any[]>([]);
const modelChartData = ref<any[]>([]);

// Initialize date range to last 3 months
const endDate = ref<Date>(new Date());
const startDate = ref<Date>(new Date(new Date().setMonth(new Date().getMonth() - 3)));
const dateRangePanel = ref();
const showCustomDialog = ref(false);

const formatNumber = (num: number) => {
	if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
	if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
	return num.toString();
};

const formatNumberWithCommas = (num: number) => {
	return num.toLocaleString('en-US');
};

const getPercentageOfTotal = (value: number, total: number) => {
	if (total === 0) return '0.00';
	return ((value / total) * 100).toFixed(2);
};

// Build a mapping from agent name to ALL its model object IDs (main + tools + rewrite + cache)
const getAgentModelsMapping = () => {
	const mapping: { [agentName: string]: { mainModel: string | null; allModels: string[] } } = {};
	
	for (const agentResult of agentDefinitions.value) {
		const agent = agentResult.resource;
		if (!agent?.name) continue;
		
		const allModels: Set<string> = new Set();
		let mainModel: string | null = null;
		
		// 1. Find the main model from workflow.resource_object_ids
		if (agent.workflow?.resource_object_ids) {
			for (const [objectId, resourceInfo] of Object.entries(agent.workflow.resource_object_ids)) {
				const info = resourceInfo as any;
				if (info?.properties?.object_role === 'main_model') {
					mainModel = objectId;
					allModels.add(objectId);
					break;
				}
			}
		}
		
		// Fallback: use ai_model_object_id if available
		if (!mainModel && agent.ai_model_object_id) {
			mainModel = agent.ai_model_object_id;
			allModels.add(agent.ai_model_object_id);
		}
		
		// 2. Collect tool models
		if (agent.tools && Array.isArray(agent.tools)) {
			for (const tool of agent.tools) {
				if (tool.ai_model_object_ids) {
					for (const modelId of Object.values(tool.ai_model_object_ids)) {
						if (modelId) allModels.add(modelId as string);
					}
				}
			}
		}
		
		// 3. User prompt rewrite model
		if (agent.text_rewrite_settings?.user_prompt_rewrite_settings?.user_prompt_rewrite_ai_model_object_id) {
			allModels.add(agent.text_rewrite_settings.user_prompt_rewrite_settings.user_prompt_rewrite_ai_model_object_id);
		}
		
		// 4. Semantic cache embedding model
		if (agent.cache_settings?.semantic_cache_settings?.embedding_ai_model_object_id) {
			allModels.add(agent.cache_settings.semantic_cache_settings.embedding_ai_model_object_id);
		}
		
		mapping[agent.name] = {
			mainModel,
			allModels: Array.from(allModels)
		};
	}
	
	return mapping;
};

// Build a mapping from model object ID to model info
const getModelInfoMapping = () => {
	const mapping: { [objectId: string]: { name: string; deployment_name: string } } = {};
	
	for (const modelResult of aiModels.value) {
		const model = modelResult.resource;
		if (!model?.object_id) continue;
		
		mapping[model.object_id] = {
			name: model.name || model.display_name || 'Unknown Model',
			deployment_name: model.deployment_name || ''
		};
	}
	
	return mapping;
};

// Computed: agents with their model names attached
const agentsWithModels = computed(() => {
	const agentModelsMapping = getAgentModelsMapping();
	const modelInfoMapping = getModelInfoMapping();
	
	return agents.value.map(agent => {
		const agentModels = agentModelsMapping[agent.agent_name];
		const mainModelId = agentModels?.mainModel;
		const mainModelInfo = mainModelId ? modelInfoMapping[mainModelId] : null;
		
		// Get all model names for this agent
		const allModelNames = (agentModels?.allModels || [])
			.map(id => modelInfoMapping[id]?.name || 'Unknown')
			.filter((name, index, arr) => arr.indexOf(name) === index); // unique names
		
		return {
			...agent,
			model_object_id: mainModelId || null,
			model_name: mainModelInfo?.name || 'Unknown',
			deployment_name: mainModelInfo?.deployment_name || '',
			all_models: allModelNames,
			model_count: allModelNames.length
		};
	});
});

// Computed: model analytics aggregated from agent data
// Note: Since agents can use multiple models, we distribute tokens proportionally
// based on the number of models an agent uses, or attribute to main model only
const modelAnalytics = computed(() => {
	const agentModelsMapping = getAgentModelsMapping();
	const modelInfoMapping = getModelInfoMapping();
	
	const modelStats: { [modelId: string]: { 
		model_name: string;
		deployment_name: string;
		total_tokens: number;
		agent_count: number;
		agents: string[];
		is_main_model_usage: boolean;
	}} = {};
	
	for (const agent of agents.value) {
		const agentModels = agentModelsMapping[agent.agent_name];
		if (!agentModels) continue;
		
		const allModelIds = agentModels.allModels;
		const mainModelId = agentModels.mainModel;
		const agentTokens = agent.total_tokens ?? 0;
		
		if (allModelIds.length === 0) {
			// No models found, attribute to "Unknown"
			if (!modelStats['unknown']) {
				modelStats['unknown'] = {
					model_name: 'Unknown',
					deployment_name: '',
					total_tokens: 0,
					agent_count: 0,
					agents: [],
					is_main_model_usage: false
				};
			}
			modelStats['unknown'].total_tokens += agentTokens;
			modelStats['unknown'].agent_count += 1;
			modelStats['unknown'].agents.push(agent.agent_name);
		} else if (allModelIds.length === 1) {
			// Single model - attribute all tokens to it
			const modelId = allModelIds[0];
			const modelInfo = modelInfoMapping[modelId];
			
			if (!modelStats[modelId]) {
				modelStats[modelId] = {
					model_name: modelInfo?.name || 'Unknown',
					deployment_name: modelInfo?.deployment_name || '',
					total_tokens: 0,
					agent_count: 0,
					agents: [],
					is_main_model_usage: true
				};
			}
			modelStats[modelId].total_tokens += agentTokens;
			modelStats[modelId].agent_count += 1;
			if (!modelStats[modelId].agents.includes(agent.agent_name)) {
				modelStats[modelId].agents.push(agent.agent_name);
			}
		} else {
			// Multiple models - attribute tokens to main model (most likely source of majority tokens)
			// But track all models used by this agent
			const primaryModelId = mainModelId || allModelIds[0];
			
			for (const modelId of allModelIds) {
				const modelInfo = modelInfoMapping[modelId];
				const isMainModel = modelId === primaryModelId;
				
				if (!modelStats[modelId]) {
					modelStats[modelId] = {
						model_name: modelInfo?.name || 'Unknown',
						deployment_name: modelInfo?.deployment_name || '',
						total_tokens: 0,
						agent_count: 0,
						agents: [],
						is_main_model_usage: isMainModel
					};
				}
				
				// Attribute tokens only to the main/primary model
				if (isMainModel) {
					modelStats[modelId].total_tokens += agentTokens;
				}
				
				modelStats[modelId].agent_count += 1;
				if (!modelStats[modelId].agents.includes(agent.agent_name)) {
					modelStats[modelId].agents.push(agent.agent_name);
				}
			}
		}
	}
	
	// Convert to array and sort by total_tokens descending
	return Object.values(modelStats).sort((a, b) => b.total_tokens - a.total_tokens);
});

// Computed properties for summary metrics
const totalAgentTokens = computed(() => {
	return agents.value.reduce((sum, agent) => sum + (agent.total_tokens ?? 0), 0);
});

const totalUserTokens = computed(() => {
	return users.value.reduce((sum, user) => sum + (user.total_tokens ?? 0), 0);
});

const totalModelTokens = computed(() => {
	return modelAnalytics.value.reduce((sum, model) => sum + (model.total_tokens ?? 0), 0);
});

const totalTokens = computed(() => {
	return Math.max(totalAgentTokens.value, totalUserTokens.value);
});

const totalUsers = computed(() => {
	return users.value.length;
});

const totalAgents = computed(() => {
	return agents.value.length;
});

const totalModels = computed(() => {
	return modelAnalytics.value.length;
});

const filteredUsers = computed(() => {
	if (!usernameFilter.value || usernameFilter.value.trim() === '') {
		return users.value;
	}
	const filterLower = usernameFilter.value.toLowerCase().trim();
	return users.value.filter(user => 
		user.username?.toLowerCase().includes(filterLower)
	);
});

const dateRangeLabel = computed(() => {
	if (!startDate.value || !endDate.value) {
		return 'Select date range';
	}

	const start = startDate.value;
	const end = endDate.value;
	const startMonth = start.toLocaleDateString('en-US', { month: 'short' });
	const endMonth = end.toLocaleDateString('en-US', { month: 'short' });
	const startYear = start.getFullYear();
	const endYear = end.getFullYear();
	const startDay = start.getDate();
	const endDay = end.getDate();

	if (startMonth === endMonth && startYear === endYear) {
		return `${startMonth} ${startDay}-${endDay}`;
	}
	if (startYear === endYear) {
		return `${startMonth} ${startDay}-${endMonth} ${endDay}`;
	}
	return `${startMonth} ${startYear}-${endMonth} ${endYear}`;
});

const toggleDateRangePanel = (event: Event) => {
	dateRangePanel.value.toggle(event);
};

const navigateDateRange = (direction: number) => {
	const currentMonth = startDate.value.getMonth();
	const currentYear = startDate.value.getFullYear();
	const newDate = new Date(currentYear, currentMonth + direction, 1);
	startDate.value = newDate;
	endDate.value = new Date(newDate.getFullYear(), newDate.getMonth() + 1, 0);
	loadData();
};

const setDateRange = (range: string) => {
	const now = new Date();
	let start: Date;
	let end: Date = new Date();

	switch (range) {
		case 'last7days':
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
			break;
		case 'last30days':
			start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
			break;
		case 'thisMonth':
			start = new Date(now.getFullYear(), now.getMonth(), 1);
			end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
			break;
		case 'lastMonth':
			start = new Date(now.getFullYear(), now.getMonth() - 1, 1);
			end = new Date(now.getFullYear(), now.getMonth(), 0);
			break;
		case 'thisQuarter':
			const quarter = Math.floor(now.getMonth() / 3);
			start = new Date(now.getFullYear(), quarter * 3, 1);
			end = new Date(now.getFullYear(), (quarter + 1) * 3, 0);
			break;
		case 'lastQuarter':
			const lastQuarter = Math.floor(now.getMonth() / 3) - 1;
			start = new Date(now.getFullYear(), lastQuarter * 3, 1);
			end = new Date(now.getFullYear(), (lastQuarter + 1) * 3, 0);
			break;
		case 'last3months':
			start = new Date(now.getFullYear(), now.getMonth() - 3, 1);
			break;
		case 'last6months':
			start = new Date(now.getFullYear(), now.getMonth() - 6, 1);
			break;
		case 'last12months':
			start = new Date(now.getFullYear(), now.getMonth() - 12, 1);
			break;
		case 'thisYear':
			start = new Date(now.getFullYear(), 0, 1);
			end = new Date(now.getFullYear(), 11, 31);
			break;
		default:
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
	}

	startDate.value = start;
	endDate.value = end;
	dateRangePanel.value.hide();
	loadData();
};

const showCustomDateRange = () => {
	dateRangePanel.value.hide();
	showCustomDialog.value = true;
};

const applyCustomDateRange = () => {
	showCustomDialog.value = false;
	loadData();
};

const formatDateRange = (range: string) => {
	const now = new Date();
	let start: Date;
	let end: Date = new Date();

	switch (range) {
		case 'last7days':
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
			break;
		case 'last30days':
			start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
			break;
		case 'thisMonth':
			start = new Date(now.getFullYear(), now.getMonth(), 1);
			end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
			break;
		case 'lastMonth':
			start = new Date(now.getFullYear(), now.getMonth() - 1, 1);
			end = new Date(now.getFullYear(), now.getMonth(), 0);
			break;
		case 'thisQuarter':
			const quarter = Math.floor(now.getMonth() / 3);
			start = new Date(now.getFullYear(), quarter * 3, 1);
			end = new Date(now.getFullYear(), (quarter + 1) * 3, 0);
			break;
		case 'lastQuarter':
			const lastQuarter = Math.floor(now.getMonth() / 3) - 1;
			start = new Date(now.getFullYear(), lastQuarter * 3, 1);
			end = new Date(now.getFullYear(), (lastQuarter + 1) * 3, 0);
			break;
		case 'last3months':
			start = new Date(now.getFullYear(), now.getMonth() - 3, 1);
			break;
		case 'last6months':
			start = new Date(now.getFullYear(), now.getMonth() - 6, 1);
			break;
		case 'last12months':
			start = new Date(now.getFullYear(), now.getMonth() - 12, 1);
			break;
		case 'thisYear':
			start = new Date(now.getFullYear(), 0, 1);
			end = new Date(now.getFullYear(), 11, 31);
			break;
		default:
			start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
	}

	return `${start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${end.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}`;
};

const loadData = async () => {
	loading.value = true;
	error.value = null;
	try {
		const start = startDate.value?.toISOString();
		const end = endDate.value?.toISOString();
		
		// Fetch analytics data and resource definitions in parallel
		const [agentsAnalytics, usersAnalytics, agentDefs, models] = await Promise.all([
			api.getAllAgentsAnalytics(start, end),
			api.getAllUsersAnalytics(start, end),
			api.getAgents(),
			api.getAIModels()
		]);
		
		agents.value = agentsAnalytics;
		users.value = usersAnalytics;
		agentDefinitions.value = agentDefs;
		aiModels.value = models;
		
		// Prepare chart data
		prepareTopUsersChartData();
		prepareModelChartData();
		
		await nextTick();
		updateTopUsersPieChart();
		updateModelPieChart();
	} catch (err: any) {
		error.value = err.data?.error || err.data?.message || err.message || 'Failed to load token analytics';
	} finally {
		loading.value = false;
	}
};

const prepareTopUsersChartData = () => {
	// Sort users by total_tokens descending
	const sortedUsers = [...users.value].sort((a, b) => (b.total_tokens ?? 0) - (a.total_tokens ?? 0));
	
	// Get top 9 users
	const top9 = sortedUsers.slice(0, 9);
	
	// Calculate "Other" as sum of remaining users
	const otherUsers = sortedUsers.slice(9);
	const otherTokens = otherUsers.reduce((sum, user) => sum + (user.total_tokens ?? 0), 0);
	
	// Build chart data
	const chartData: any[] = top9.map(user => ({
		label: user.username || 'Unknown',
		value: user.total_tokens ?? 0
	}));
	
	// Add "Other" category if there are more than 9 users
	if (otherUsers.length > 0) {
		chartData.push({
			label: `Other (${otherUsers.length} users)`,
			value: otherTokens
		});
	}
	
	topUsersChartData.value = chartData;
};

const prepareModelChartData = () => {
	// Sort models by total_tokens descending
	const sortedModels = [...modelAnalytics.value].sort((a, b) => (b.total_tokens ?? 0) - (a.total_tokens ?? 0));
	
	// Get top 9 models
	const top9 = sortedModels.slice(0, 9);
	
	// Calculate "Other" as sum of remaining models
	const otherModels = sortedModels.slice(9);
	const otherTokens = otherModels.reduce((sum, model) => sum + (model.total_tokens ?? 0), 0);
	
	// Build chart data
	const chartData: any[] = top9.map(model => ({
		label: model.model_name || 'Unknown',
		value: model.total_tokens ?? 0
	}));
	
	// Add "Other" category if there are more than 9 models
	if (otherModels.length > 0) {
		chartData.push({
			label: `Other (${otherModels.length} models)`,
			value: otherTokens
		});
	}
	
	modelChartData.value = chartData;
};

const updateTopUsersPieChart = () => {
	if (!topUsersPieCanvas.value) {
		return;
	}

	// Destroy existing chart
	if (topUsersPieChart) {
		topUsersPieChart.destroy();
		topUsersPieChart = null;
	}

	if (!topUsersChartData.value || topUsersChartData.value.length === 0) {
		return;
	}

	const colors = [
		'#4A90E2', '#50C878', '#FF6B6B', '#FFD93D', '#9B59B6',
		'#1ABC9C', '#E74C3C', '#F39C12', '#3498DB', '#95A5A6'
	];

	const labels = topUsersChartData.value.map(item => item.label);
	const data = topUsersChartData.value.map(item => item.value);

	topUsersPieChart = new Chart(topUsersPieCanvas.value, {
		type: 'pie',
		data: {
			labels,
			datasets: [{
				data,
				backgroundColor: colors.slice(0, data.length),
				borderColor: '#ffffff',
				borderWidth: 2
			}]
		},
		options: {
			responsive: true,
			maintainAspectRatio: false,
			plugins: {
				legend: {
					display: true,
					position: 'right',
					labels: {
						generateLabels: (chart) => {
							const data = chart.data;
							if (data.labels && data.datasets.length) {
								const total = data.datasets[0].data.reduce((sum: number, val: any) => sum + val, 0);
								return data.labels.map((label, i) => {
									const value = data.datasets[0].data[i] as number;
									const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
									return {
										text: `${label}: ${formatNumber(value)} (${percentage}%)`,
										fillStyle: colors[i % colors.length],
										hidden: false,
										index: i,
										strokeStyle: '#ffffff',
										lineWidth: 2
									};
								});
							}
							return [];
						}
					}
				},
				tooltip: {
					callbacks: {
						label: (context) => {
							const value = context.parsed;
							const total = context.dataset.data.reduce((sum: number, val: any) => sum + val, 0);
							const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
							return `${context.label}: ${formatNumberWithCommas(value)} tokens (${percentage}%)`;
						}
					}
				}
			}
		}
	});
};

const updateModelPieChart = () => {
	if (!modelPieCanvas.value) {
		return;
	}

	// Destroy existing chart
	if (modelPieChart) {
		modelPieChart.destroy();
		modelPieChart = null;
	}

	if (!modelChartData.value || modelChartData.value.length === 0) {
		return;
	}

	// Use a different color palette for models
	const colors = [
		'#6366F1', '#8B5CF6', '#A855F7', '#D946EF', '#EC4899',
		'#F43F5E', '#EF4444', '#F97316', '#FACC15', '#84CC16'
	];

	const labels = modelChartData.value.map(item => item.label);
	const data = modelChartData.value.map(item => item.value);

	modelPieChart = new Chart(modelPieCanvas.value, {
		type: 'pie',
		data: {
			labels,
			datasets: [{
				data,
				backgroundColor: colors.slice(0, data.length),
				borderColor: '#ffffff',
				borderWidth: 2
			}]
		},
		options: {
			responsive: true,
			maintainAspectRatio: false,
			plugins: {
				legend: {
					display: true,
					position: 'right',
					labels: {
						generateLabels: (chart) => {
							const chartData = chart.data;
							if (chartData.labels && chartData.datasets.length) {
								const total = chartData.datasets[0].data.reduce((sum: number, val: any) => sum + val, 0);
								return chartData.labels.map((label, i) => {
									const value = chartData.datasets[0].data[i] as number;
									const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
									return {
										text: `${label}: ${formatNumber(value)} (${percentage}%)`,
										fillStyle: colors[i % colors.length],
										hidden: false,
										index: i,
										strokeStyle: '#ffffff',
										lineWidth: 2
									};
								});
							}
							return [];
						}
					}
				},
				tooltip: {
					callbacks: {
						label: (context) => {
							const value = context.parsed;
							const total = context.dataset.data.reduce((sum: number, val: any) => sum + val, 0);
							const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
							return `${context.label}: ${formatNumberWithCommas(value)} tokens (${percentage}%)`;
						}
					}
				}
			}
		}
	});
};

watch([topUsersChartData, topUsersPieCanvas], () => {
	if (topUsersChartData.value && topUsersPieCanvas.value) {
		nextTick(() => {
			updateTopUsersPieChart();
		});
	}
}, { deep: true });

watch([modelChartData, modelPieCanvas], () => {
	if (modelChartData.value && modelPieCanvas.value) {
		nextTick(() => {
			updateModelPieChart();
		});
	}
}, { deep: true });

onMounted(() => {
	loadData();
});

onUnmounted(() => {
	if (topUsersPieChart) {
		topUsersPieChart.destroy();
		topUsersPieChart = null;
	}
	if (modelPieChart) {
		modelPieChart.destroy();
		modelPieChart = null;
	}
});
</script>

<style scoped>
.analytics-tokens-page {
	padding: 2rem;
}

.filters-row {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 2rem;
	justify-content: flex-start;
}

.date-range-selector {
	display: flex;
	gap: 1rem;
	align-items: center;
}

.date-range-menu {
	min-width: 300px;
	max-height: 500px;
	overflow-y: auto;
}

.date-range-menu-header {
	display: flex;
	justify-content: space-between;
	margin-bottom: 0.5rem;
	padding-bottom: 0.5rem;
	border-bottom: 1px solid var(--surface-border);
}

.section-title {
	font-weight: 600;
	font-size: 0.75rem;
	color: var(--text-color-secondary);
	margin-bottom: 0.25rem;
	padding: 0.25rem 0;
	text-transform: uppercase;
	letter-spacing: 0.5px;
}

.date-range-section {
	margin-bottom: 0.5rem;
}

.date-range-option {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 0.5rem 0.75rem;
	cursor: pointer;
	border-radius: 4px;
	transition: background-color 0.2s;
	font-size: 0.875rem;
}

.date-range-option:hover {
	background-color: var(--surface-hover);
}

.date-preview {
	color: var(--text-color-secondary);
	font-size: 0.875rem;
}

.custom-date-range-dialog {
	display: flex;
	flex-direction: column;
	gap: 1rem;
}

.date-input-group {
	display: flex;
	flex-direction: column;
	gap: 0.5rem;
}

.date-input-group label {
	font-weight: 500;
}

.dialog-actions {
	display: flex;
	justify-content: flex-end;
	gap: 0.5rem;
	margin-top: 1rem;
}

.metrics-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
	gap: 1.5rem;
	margin-bottom: 2rem;
}

.metric-card {
	text-align: center;
}

.metric-value {
	font-size: 2rem;
	font-weight: bold;
	color: var(--primary-color);
}

.metric-label {
	margin-top: 0.5rem;
	color: var(--text-color-secondary);
}

.charts-row {
	display: flex;
	flex-wrap: wrap;
	gap: 1.5rem;
	margin-bottom: 2rem;
}

.chart-container {
	margin-bottom: 2rem;
}

.chart-half {
	flex: 1 1 calc(50% - 0.75rem);
	min-width: 400px;
	margin-bottom: 0;
}

@media (max-width: 900px) {
	.chart-half {
		flex: 1 1 100%;
	}
}

.pie-chart-wrapper {
	position: relative;
	height: 350px;
	width: 100%;
	display: flex;
	justify-content: center;
	align-items: center;
}

.table-section {
	margin-bottom: 2rem;
}

.tokens-table {
	margin-top: 0;
}

.filters-row-bottom {
	display: flex;
	gap: 1rem;
	align-items: center;
	margin-bottom: 1rem;
	justify-content: flex-end;
}

.search-filter {
	flex: 0 0 auto;
}

.loading,
.error {
	text-align: center;
	padding: 2rem;
}

.p-input-icon-left {
	position: relative;
}

.p-input-icon-left i {
	position: absolute;
	left: 0.75rem;
	top: 50%;
	margin-top: -0.5rem;
	color: var(--text-color-secondary);
}

.p-input-icon-left input {
	padding-left: 2.5rem;
}

.model-count-badge {
	display: inline-block;
	margin-left: 0.5rem;
	padding: 0.15rem 0.5rem;
	font-size: 0.75rem;
	font-weight: 500;
	color: var(--primary-color);
	background-color: var(--primary-100);
	border-radius: 1rem;
}

.info-message {
	margin-bottom: 1rem;
}

.info-message :deep(.p-message-text) {
	font-size: 0.875rem;
}
</style>
