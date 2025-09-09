<template>
    <header role="banner">
        <NavBarSettings />
    </header>

    <main id="main-content">

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
                        to="/create-agent"
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
                                {{ slotProps.data.resource.description || 'No description' }}
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
                                {{ slotProps.data.resource.created_by || 'Unknown' }}
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
                            <div style="position: relative; display: flex; justify-content: center; align-items: center;">
                                <Button icon="pi pi-ellipsis-h" class="p-button-text p-button-rounded" @click="toggleActions(slotProps)" aria-label="Actions" />
                                <div v-if="slotProps.data.showActions" class="csm-popover-actions">
                                    <Button label="Edit" icon="pi pi-pencil" class="p-button-text w-full" @click="onEditAgent(slotProps.data)" />
                                </div>
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
        </div>
    </main>
</template>


<script lang="ts">
import api from '@/js/api';
import type { AgentBase, ResourceProviderGetResult } from '@/js/types';
import '@/styles/agents.scss';
import { computed, defineComponent, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import NavBarSettings from '~/components/NavBarSettings.vue';
import '@/styles/loading.scss';

import Button from 'primevue/button';
import Column from 'primevue/column';
import DataTable from 'primevue/datatable';

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
    },

    setup() {
        // Reactive state with ref
        const agents = ref<ResourceProviderGetResult<AgentBase>[]>([]);
        const loading = ref(false);
        const error = ref<string | null>(null);
        const searchByName = ref('');
        const router = useRouter();

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

        const getPrimaryRole = (roles: string[]): string => {
            for (const role of SUPPORTED_ROLES) {
                if (roles.includes(role)) return role;
            }
            return roles[0] || 'Unknown';
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

        // Actions popover logic
        const toggleActions = (slotProps: any) => {
            slotProps.data.showActions = !slotProps.data.showActions;
            if (slotProps.data.showActions) {
                const closePopover = () => {
                    slotProps.data.showActions = false;
                    window.removeEventListener('click', closePopover);
                };
                setTimeout(() => {
                    window.addEventListener('click', closePopover);
                }, 0);
            }
        };
        const onEditAgent = (rowData: any) => {
            // Edit agent: navigate to create-agent with query params
            const agentName = rowData.resource.name;
            router.push({ path: '/create-agent', query: { edit: 'true', agentName } });
        };

        // Lifecycle
        onMounted(() => {
            loadAgents();
        });

        return {
            agents,
            loading,
            error,
            searchByName,
            filteredAgents,
            loadAgents,
            getPrimaryRole,
            formatExpirationDate,
            clearSearch,
            toggleActions,
            onEditAgent,
            SCROLL_HEIGHT
        };
    },
});
</script>
