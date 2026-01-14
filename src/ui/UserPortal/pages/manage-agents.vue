<template>
    <!-- Access Denied Message for 403 Errors -->
    <div v-if="$appConfigStore.hasConfigurationAccessError" class="access-denied-overlay">
        <div class="access-denied-container">
            <div class="access-denied-icon">
                <i class="pi pi-ban" style="font-size: 4rem; color: #e74c3c;"></i>
            </div>
            <h2 class="access-denied-title">Access Denied</h2>
            <p class="access-denied-message">
                {{ $appConfigStore.configurationAccessErrorMessage }}
            </p>
        </div>
    </div>

		<div v-if="!$appConfigStore.agentSelfServiceFeatureEnabled" class="access-denied-overlay">
        <div class="access-denied-container">
            <div class="access-denied-icon">
                <i class="pi pi-ban" style="font-size: 4rem; color: #e74c3c;"></i>
            </div>
            <h2 class="access-denied-title">Feature not available</h2>
            <p class="access-denied-message">
                	This feature is currently disabled.
            </p>
        </div>
    </div>

    <!-- Normal Page Content -->
    <div v-else>
        <header role="banner">
            <NavBarSettings />
        </header>

        <main id="main-content">
        <Toast position="top-center" />

        <div class="w-full max-w-[1430px] mx-auto px-4 py-7">
            <div class="csm-backto-chats-1">
                <nuxt-link to="/" class="backto-chats">
                    <i class="pi pi-angle-left relative top-[2px]"></i> Return to Chats
                </nuxt-link>
            </div>

            <div class="flex flex-wrap items-center -mx-4">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <h2 class="page-header text-3xl text-[#334581]">Manage Agents</h2>
                </div>
            </div>

            <div class="flex flex-wrap items-center -mx-4 mb-5">
                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5 text-center md:text-left">
                    <nuxt-link
                        v-if="hasAgentsContributorRole && hasPromptsContributorRole"
                        :to="{ path: '/create-agent', query: { returnTo: 'manage-agents' } }"
                        class="p-button p-component create-agent-button">
                        New Agent <i class="pi pi-plus ml-3"></i>
                    </nuxt-link>
                </div>

                <div class="w-full max-w-full md:max-w-[50%] px-4 mb-5">
                    <div class="flex items-center relative">
                        <InputText
                            type="text"
                            class="w-full"
                            name="searchTabelInput1"
                            id="searchTabelInput1"
                            placeholder="Search by name"
                            v-model="searchByName"
                        />

                        <Button
                            aria-label="Clear Search"
                            severity="primary"
                            icon="pi pi-times"
                            class="min-h-[40px] min-w-[40px] w-auto absolute top-0 right-0 z-[2]"
                            @click="clearSearch"
                            v-if="searchByName"
                        />
                        <Button
                            aria-label="Search Users"
                            severity="primary"
                            icon="pi pi-search"
                            class="min-h-[40px] min-w-[40px] w-auto absolute top-0 right-0 z-[2]"
                            v-else
                        />
                    </div>
                </div>
            </div>

            <div class="csm-table-area-1">
                <!-- Loading State -->
                <div v-if="loading" class="loading-container">
                    <i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
                    <p>Loading agents...</p>
                </div>

                <!-- Error State -->
                <div v-else-if="error" class="error-message">
                    <i class="pi pi-exclamation-triangle" style="font-size: 2rem; color: #e74c3c;"></i>
                    <p>{{ error }}</p>
                    <Button label="Retry" @click="loadAgents" />
                </div>

                <!-- Data Table -->
                <DataTable
                    v-else
                    :value="filteredAgents"
                    scrollable
                    :scrollHeight="SCROLL_HEIGHT"
                    responsiveLayout="scroll"
                    :sortOrder="1"
                    class="p-datatable-sm csm-dataTable-1"
                    tableStyle="min-width: 50rem"
                >
                    <Column
                        header="Name"
                        :sortable="true"
                        style="min-width: 200px"
                    >
                        <template #body="slotProps">
                            <div class="agent-name">
                                {{ slotProps.data.resource.display_name || slotProps.data.resource.name }}
                            </div>
                        </template>
                    </Column>

                    <Column
                        header="Description"
                        :sortable="false"
                        style="min-width: 250px; max-width: 250px;"
                    >
                        <template #body="slotProps">
                            <div class="agent-description">
                                {{ slotProps.data.resource.description || '' }}
                            </div>
                        </template>
                    </Column>

                    <Column
                        header="Owner"
                        :sortable="true"
                        style="min-width: 200px"
                    >
                        <template #body="slotProps">
                            <div class="agent-owner">
                                {{ getOwnerDisplay(slotProps.data.resource) }}
                            </div>
                        </template>
                    </Column>

                    <Column
                        header="Role"
                        :sortable="true"
                        style="min-width: 150px"
                    >
                        <template #body="slotProps">
                            <div class="agent-role">
                                {{ getPrimaryRole(slotProps.data.roles) }}
                            </div>
                        </template>
                    </Column>

                    <Column
                        header="Expiration"
                        :sortable="true"
                        style="min-width: 120px"
                    >
                        <template #body="slotProps">
                            <div class="agent-expiration">
                                {{ formatExpirationDate(slotProps.data.resource.expiration_date) }}
                            </div>
                        </template>
                    </Column>

                    <Column
                        header="Actions"
                        style="min-width: 80px"
                    >
                        <template #body="slotProps">
                            <div style="display: flex; justify-content: center; align-items: center; gap: 0.5rem;">
                                <Button 
                                    icon="pi pi-pencil" 
                                    class="p-button-text p-button-rounded" 
                                    @click="onEditAgent(slotProps.data)" 
                                    :aria-label="`Edit ${slotProps.data.resource.display_name || slotProps.data.resource.name}`"
                                />
                                <Button 
                                    icon="pi pi-trash" 
                                    class="p-button-text p-button-rounded" 
                                    @click="onDeleteAgent(slotProps.data)" 
                                    :aria-label="`Delete ${slotProps.data.resource.display_name || slotProps.data.resource.name}`"
                                />
                            </div>
                        </template>
                    </Column>

                </DataTable>

                <!-- Empty State -->
                <div v-if="!loading && !error && filteredAgents.length === 0" class="empty-state">
                    <i class="pi pi-info-circle" style="font-size: 2rem; color: #6c757d;"></i>
                    <p>No agents found where you have Owner or Contributor access.</p>
                </div>
            </div>

            <!-- Delete Agent Confirmation Dialog -->
            <ConfirmationDialog
                v-if="agentToDelete !== null"
                :visible="agentToDelete !== null"
                header="Delete Agent"
                confirm-text="Yes"
                cancel-text="Cancel"
                confirm-button-severity="danger"
                @confirm="handleDeleteAgent"
                @cancel="agentToDelete = null"
                @update:visible="agentToDelete = null"
            >
                <div>
                    Are you sure you want to delete the agent "{{ agentToDelete?.resource?.display_name || agentToDelete?.resource?.name }}"? This action cannot be undone.
                </div>
            </ConfirmationDialog>
        </div>
    </main>
    </div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { AgentBase, ResourceProviderGetResult } from '@/js/types';
import '@/styles/agents.scss';
import { computed, defineComponent, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import NavBarSettings from '~/components/NavBarSettings.vue';
import '@/styles/loading.scss';
import '@/styles/access-denied.scss';

import Button from 'primevue/button';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';
import Toast from 'primevue/toast';
import ConfirmationDialog from '~/components/ConfirmationDialog.vue';

// Constants
const SCROLL_HEIGHT = '500px';
const SUPPORTED_ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const DEFAULT_LOCALE = 'en-US';

export default defineComponent({
    name: 'ManageAgents',

    components: {
        NavBarSettings,
        DataTable,
        Column,
        Button,
        Toast,
        ConfirmationDialog,
    },

    setup() {
        // Reactive state with ref
        const agents = ref<ResourceProviderGetResult<AgentBase>[]>([]);
        const loading = ref(false);
        const error = ref<string | null>(null);
        const searchByName = ref('');
        const hasAgentsContributorRole = ref(false);
        const hasPromptsContributorRole = ref(false);
        const router = useRouter();
        // Cache resolved owner emails by principal id
        const ownerEmailById = ref<Record<string, string>>({});
        const agentToDelete = ref<ResourceProviderGetResult<AgentBase> | null>(null);

        // Computed property
        const filteredAgents = computed(() => {
            let filtered = agents.value.filter(agent =>
                agent.roles.includes('Owner') || agent.roles.includes('Contributor')
            );

            // Filter by name if search query is provided
            if (searchByName.value.trim()) {
                const query = searchByName.value.toLowerCase().trim();
                filtered = filtered.filter(agent => {
                    const displayName = agent.resource.display_name || agent.resource.name || '';
                    return displayName.toLowerCase().includes(query);
                });
            }

            return filtered;
        });

        // Methods
        const loadAgents = async () => {
            loading.value = true;
            error.value = null;

            try {
                agents.value = await api.getAllowedAgents();

                // Try to resolve owner ids present in payload
                let ids = Array.from(new Set(
                    agents.value
                        .map(a => a.resource.owner_user_id)
                        .filter((id): id is string => !!id && typeof id === 'string')
                ));

                // Fallback: if none present, enrich from management endpoint by name
                if (ids.length === 0) {
                    try {
                        const managementAgents = await api.getAgents();
                        const ownerByName = new Map<string, string | null | undefined>();
                        for (const m of managementAgents) {
                            ownerByName.set(m.resource.name, (m.resource as AgentBase).owner_user_id);
                        }
                        let updated = false;
                        for (const a of agents.value) {
                            if (!a.resource.owner_user_id && ownerByName.has(a.resource.name)) {
                                a.resource.owner_user_id = ownerByName.get(a.resource.name) ?? null;
                                updated = true;
                            }
                        }
                        if (updated) {
                            ids = Array.from(new Set(
                                agents.value
                                    .map(a => a.resource.owner_user_id)
                                    .filter((id): id is string => !!id && typeof id === 'string')
                            ));
                        }
                    } catch (enrichErr) {
                        // no-op
                    }
                }

                if (ids.length > 0) {
                    try {
                        const principals = await api.getSecurityPrincipals(ids);
                        for (const p of principals) {
                            ownerEmailById.value[p.id] = p.email ?? p.name ?? 'N/A';
                        }
                    } catch (resolveErr) {
                        // no-op
                    }
                } else {
                    // no owner ids present
                }
            } catch (err) {
                console.error('Failed to load agents:', err);
                const errorMessage = err instanceof Error
                    ? `Failed to load agents: ${err.message}`
                    : 'Failed to load agents. Please try again later.';
                error.value = errorMessage;
            } finally {
                loading.value = false;
            }
        };

        const checkContributorRoles = async () => {
            try {
                const result = await api.checkContributorRoles();
                hasAgentsContributorRole.value = result.hasAgentsContributorRole;
                hasPromptsContributorRole.value = result.hasPromptsContributorRole;
            } catch (err) {
                console.error('Failed to check contributor roles:', err);
                hasAgentsContributorRole.value = false;
                hasPromptsContributorRole.value = false;
            }
        };

        const getPrimaryRole = (roles: string[]): string => {
            for (const role of SUPPORTED_ROLES) {
                if (roles.includes(role)) return role;
            }
            return roles[0] || '';
        };

        const formatExpirationDate = (expirationDate: string | null): string => {
            if (!expirationDate) return 'Never';

            try {
                const date = new Date(expirationDate);
                return date.toLocaleDateString(DEFAULT_LOCALE);
            } catch (err) {
                console.warn('Invalid expiration date format:', expirationDate);
                return 'Invalid date';
            }
        };

        const clearSearch = () => {
            searchByName.value = '';
        };

        const onEditAgent = (rowData: any) => {
            // Edit agent: navigate to create-agent with query params
            const agentName = rowData.resource.name;
            router.push({ path: '/create-agent', query: { edit: 'true', agentName, returnTo: 'manage-agents' } });
        };

        const onDeleteAgent = (rowData: any) => {
            // Set the agent to delete
            agentToDelete.value = rowData;
        };

        const handleDeleteAgent = async () => {
            if (!agentToDelete.value) return;

            const agentName = agentToDelete.value.resource.name;
            const agentDisplayName = agentToDelete.value.resource.display_name || agentName;

            try {
                await api.deleteAgent(agentName);
                agentToDelete.value = null;
                // Reload agents list
                await loadAgents();
                // Show success message
                const nuxtApp = useNuxtApp();
                nuxtApp.vueApp.config.globalProperties.$toast.add({
                    severity: 'success',
                    summary: 'Agent Deleted',
                    detail: `Agent "${agentDisplayName}" has been deleted successfully.`,
                    life: 3000,
                });
            } catch (err: any) {
                console.error('Failed to delete agent:', err);
                agentToDelete.value = null;
                
                // Extract error message
                let errorMessage = 'Failed to delete agent. Please try again later.';
                if (err?.data) {
                    if (typeof err.data === 'string') {
                        errorMessage = err.data;
                    } else {
                        errorMessage = err.data.message || err.data.title || err.data.detail || err.data.error || JSON.stringify(err.data);
                    }
                } else if (err?.response?.data) {
                    if (typeof err.response.data === 'string') {
                        errorMessage = err.response.data;
                    } else {
                        errorMessage = err.response.data.message || err.response.data.title || err.response.data.detail || err.response.data.error || JSON.stringify(err.response.data);
                    }
                } else if (err?.message) {
                    // Extract the actual error message from the error
                    const message = err.message;
                    // Check if it contains "is not available" which suggests a permissions or path issue
                    if (message.includes('is not available')) {
                        errorMessage = 'You do not have permission to delete this agent, or the agent may have already been deleted.';
                    } else {
                        errorMessage = message;
                    }
                }
                
                // Show error message
                const nuxtApp = useNuxtApp();
                nuxtApp.vueApp.config.globalProperties.$toast.add({
                    severity: 'error',
                    summary: 'Delete Failed',
                    detail: errorMessage,
                    life: 5000,
                });
            }
        };

        const getOwnerEmail = (ownerUserId?: string | null): string => {
            if (!ownerUserId) return '';
            return ownerEmailById.value[ownerUserId] || '';
        };

        const getOwnerDisplay = (resource: AgentBase): string => {
            const email = getOwnerEmail(resource.owner_user_id);
            if (email) return email;
            if (resource.created_by) return resource.created_by;
            return 'N/A';
        };

        // Lifecycle
        onMounted(() => {
            loadAgents();
            checkContributorRoles();
        });

        return {
            agents,
            loading,
            error,
            searchByName,
            hasAgentsContributorRole,
            hasPromptsContributorRole,
            filteredAgents,
            loadAgents,
            getPrimaryRole,
            formatExpirationDate,
            clearSearch,
            onEditAgent,
            onDeleteAgent,
            handleDeleteAgent,
            agentToDelete,
            getOwnerEmail,
            getOwnerDisplay,
            SCROLL_HEIGHT
        };
    },
});
</script>

<style lang="scss" scoped>
/* Access Denied Styles */
.access-denied-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100vw;
	height: 100vh;
	background-color: var(--primary-bg, #fff);
	display: flex;
	justify-content: center;
	align-items: center;
	z-index: 9999;
}

.access-denied-container {
	text-align: center;
	padding: 2rem;
	max-width: 500px;
	margin: 0 auto;
}

.access-denied-icon {
	margin-bottom: 1.5rem;
}

.access-denied-title {
	font-size: 2rem;
	font-weight: 600;
	color: var(--primary-color, #131833);
	margin-bottom: 1rem;
	margin-top: 0;
}

.access-denied-message {
	font-size: 1.1rem;
	color: var(--secondary-text, #666);
	line-height: 1.6;
	margin: 0;
}
</style>
